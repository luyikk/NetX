using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netx;

public partial class ModuleWeaver : BaseModuleWeaver
{
 
    FieldDefinition obj;

    public override void Execute()
    {
              

        var allinterface = GetBuildInterfaces();

        foreach (var iface in allinterface)
        {
            var newType = new TypeDefinition(iface.Namespace, iface.Name + "_Builder_Netx_Implementation", TypeAttributes.Public| TypeAttributes.BeforeFieldInit, TypeSystem.ObjectReference);
            newType.Interfaces.Add(new InterfaceImplementation(iface));         
            obj = new FieldDefinition("obj", FieldAttributes.Private, ModuleDefinition.ImportReference(typeof(INetxBuildInterface)));           
            newType.Fields.Add(obj);

            AddConstructor(newType);
            
            var allRpc = iface.GetMethods().Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null);

            if (allRpc.FirstOrDefault(p => p.ContainsGenericParameter) != null)
            {
                LogInfo($"not make build the '{iface.FullName}',is have generic method.");
                continue;
            }

            foreach (var rpc in allRpc)
                AddRpc(newType, rpc);

            ModuleDefinition.Types.Add(newType);

            LogInfo($"Added Packer Type '{newType.FullName}' with Interface '{iface.FullName}'.");
        }


    }

 

    TypeDefinition[] GetBuildInterfaces()
    {
        var all = ModuleDefinition.GetAllTypes();
        return ModuleDefinition.GetAllTypes().Where(p => p.IsInterface && p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "Build") != null).ToArray();
    }



    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }


    void AddConstructor(TypeDefinition newType)
    {
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName|MethodAttributes.HideBySig, TypeSystem.VoidReference);
        var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
     
        method.Parameters.Add(new ParameterDefinition("netx_interface",ParameterAttributes.HasDefault, ModuleDefinition.ImportReference(typeof(INetxBuildInterface))));
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Nop);
        processor.Emit(OpCodes.Nop);
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Stfld, obj);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }



    public MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType)
        {
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis
        };
        if (self.DeclaringType.HasGenericParameters)
            reference.DeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments);
        else
            reference.DeclaringType = self.DeclaringType;


        reference.CallingConvention = self.CallingConvention;

        foreach (var parameter in self.Parameters)
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

        foreach (var genericParameter in self.GenericParameters)
            reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));

        return reference;

    }

    public GenericInstanceMethod MakeGenericInstanceMethod(MethodReference self,params TypeReference[] arguments)
    {
        var reference = new GenericInstanceMethod(self);

        foreach (var item in arguments)
        {
            reference.GenericArguments.Add(item);
        }
       

        return reference;
    }


    void AddRpc(TypeDefinition newType, MethodDefinition irpc)
    {
        var tag = irpc.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG");
        if (tag is null)
            return;
        var cmdx =tag.ConstructorArguments.First().Value;
        var cmd = 0;
        switch (cmdx)
        {
            case Mono.Cecil.CustomAttributeArgument args:
                cmd =(int) args.Value;
                break;
            default:
                cmd = (int)cmdx;
                break;                
        }

       
        var method = new MethodDefinition(irpc.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, irpc.ReturnType);

        var il = method.Body.GetILProcessor();

        var parameters = irpc.Parameters;
        var paramTypes = ParamTypes(parameters.ToArray(), false);

        for(int i=0;i< paramTypes.Length;i++)
        {
            method.Parameters.Add(new ParameterDefinition(parameters[i].Name, parameters[i].Attributes, paramTypes[i]));
        }

        if (irpc.ContainsGenericParameter)
        {
            throw new Exception($"not have generic parameter{irpc.FullName}");
        }

        var returntype= irpc.ReturnType;

        if(returntype.IsGenericInstance)
        {
            var genreturntype = returntype as GenericInstanceType;

            if(genreturntype.Name!= "Task`1")
            {
                throw new Exception($"return type error:{genreturntype.FullName}");
            }

            if (genreturntype.GenericArguments[0].Name == "IResult")
            {

                ParametersArray args = new ParametersArray(this, il, paramTypes);

                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, obj);
                il.Emit(OpCodes.Ldc_I4, cmd);

                GenericArray<System.Object> argsArr = new GenericArray<System.Object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

                for (int i = 0; i < parameters.Count; i++)
                {
                    // args[i] = argi;
                    if (!parameters[i].IsOut)
                    {
                        argsArr.BeginSet(i);
                        args.Get(i);
                        argsArr.EndSet(parameters[i].ParameterType);
                    }
                }
                argsArr.Load();

                var asyncAction = obj.FieldType.Resolve().Methods.First(p => p.Name == "AsyncFunc");

                il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(asyncAction));
                il.Emit(OpCodes.Ret);
                newType.Methods.Add(method);

            }
            else
            {

                ParametersArray args = new ParametersArray(this, il, paramTypes);

                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, obj);
                il.Emit(OpCodes.Ldc_I4, cmd);

                GenericArray<System.Object> argsArr = new GenericArray<System.Object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

                for (int i = 0; i < parameters.Count; i++)
                {
                    // args[i] = argi;
                    if (!parameters[i].IsOut)
                    {
                        argsArr.BeginSet(i);
                        args.Get(i);
                        argsArr.EndSet(parameters[i].ParameterType);
                    }
                }
                argsArr.Load();

                var genericAsyncFunc = obj.FieldType.Resolve().Methods.First(p => p.Name == "AsyncFunc" && p.CallingConvention == MethodCallingConvention.Generic);

                il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(MakeGenericInstanceMethod(genericAsyncFunc, genreturntype.GenericArguments.ToArray())));
                il.Emit(OpCodes.Ret);
                newType.Methods.Add(method);
            }

        }
        else if(returntype.FullName=="System.Void")
        {
            ParametersArray args = new ParametersArray(this, il, paramTypes);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, obj);
            il.Emit(OpCodes.Ldc_I4, cmd);

            GenericArray<System.Object> argsArr = new GenericArray<System.Object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

            for (int i = 0; i < parameters.Count; i++)
            {
                // args[i] = argi;
                if (!parameters[i].IsOut)
                {
                    argsArr.BeginSet(i);
                    args.Get(i);
                    argsArr.EndSet(parameters[i].ParameterType);
                }
            }
            argsArr.Load();

            var Action = obj.FieldType.Resolve().Methods.First(p => p.Name == "Action");

            il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(Action));
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }
        else if(returntype.FullName== "System.Threading.Tasks.Task")
        {

            ParametersArray args = new ParametersArray(this, il, paramTypes);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, obj);
            il.Emit(OpCodes.Ldc_I4, cmd);

            GenericArray<System.Object> argsArr = new GenericArray<System.Object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

            for (int i = 0; i < parameters.Count; i++)
            {
                // args[i] = argi;
                if (!parameters[i].IsOut)
                {
                    argsArr.BeginSet(i);
                    args.Get(i);
                    argsArr.EndSet(parameters[i].ParameterType);
                }
            }
            argsArr.Load();

            var asyncAction = obj.FieldType.Resolve().Methods.First(p => p.Name == "AsyncAction");

            il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(asyncAction));
            il.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }
        else
        {
            ParametersArray args = new ParametersArray(this, il, paramTypes);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, obj);
            il.Emit(OpCodes.Ldc_I4, cmd);

            il.Emit(OpCodes.Ldtoken, irpc.ReturnType);
            var ptype = ModuleDefinition.ImportReference(GetMethodInfo.GetTypeofHandler());
            il.Emit(OpCodes.Call, ptype);
            

            GenericArray<object> argsArr = new GenericArray<object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

            for (int i = 0; i < parameters.Count; i++)
            {
                // args[i] = argi;
                if (!parameters[i].IsOut)
                {
                    argsArr.BeginSet(i);
                    args.Get(i);
                    argsArr.EndSet(parameters[i].ParameterType);
                }
            }
            argsArr.Load();

            var func = obj.FieldType.Resolve().Methods.First(p => p.Name == "Func");
            il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(func));

            var res = new VariableDefinition(irpc.ReturnType);
            method.Body.Variables.Add(res);
            Convert(il, ModuleDefinition.ImportReference(typeof(System.Object)), irpc.ReturnType, false);
            il.Emit(OpCodes.Stloc, res);
            il.Emit(OpCodes.Ldloc, res);

            il.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }
    }

 

    private static TypeReference[] ParamTypes(ParameterDefinition[] parms, bool noByRef)
    {
        TypeReference[] types = new TypeReference[parms.Length];
        for (int i = 0; i < parms.Length; i++)
        {
            types[i] = parms[i].ParameterType;
            if (noByRef && types[i].IsByReference)
                types[i] = types[i].GetElementType();
        }
        return types;
    }


    public override bool ShouldCleanReference => true;
}

