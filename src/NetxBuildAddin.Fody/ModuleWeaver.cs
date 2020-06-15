using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Netx;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class ModuleWeaver : BaseModuleWeaver
{

    FieldDefinition obj;

    public override void Execute()
    {


        var allinterface = GetBuildInterfaces();

        Dictionary<string, MethodDefinition> MethodDict = new Dictionary<string, MethodDefinition>();

        foreach (var iface in allinterface)
        {
            var newType = new TypeDefinition(iface.Namespace, iface.Name + "_Builder_Netx_Implementation", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, TypeSystem.ObjectReference);
            newType.Interfaces.Add(new InterfaceImplementation(iface));
            obj = new FieldDefinition("obj", FieldAttributes.Private, ModuleDefinition.ImportReference(typeof(INetxBuildInterface)));
            newType.Fields.Add(obj);

            var method = AddConstructor(newType);

            var allRpc = iface.Methods.Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null);

            if (allRpc.FirstOrDefault(p => p.ContainsGenericParameter) != null)
            {
                LogInfo($"not make build the '{iface.FullName}',is have generic method.");
                continue;
            }

            foreach (var rpc in allRpc)
                AddRpc(newType, rpc);


            foreach (var ibase in iface.Interfaces)
            {
                var baseallRpc = ibase.InterfaceType.Resolve().Methods.Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null);

                if (baseallRpc.FirstOrDefault(p => p.ContainsGenericParameter) != null)
                {
                    LogInfo($"not make build the '{iface.FullName}',is have generic method.");
                    continue;
                }

                foreach (var rpc in baseallRpc)
                    AddRpc(newType, rpc);
            }

            var implementationMethod = AddGetImplementation(newType, iface, method);

            MethodDict.Add(iface.FullName, implementationMethod);
            ModuleDefinition.Types.Add(newType);

            LogInfo($"Added Packer Type '{newType.FullName}' with Interface '{iface.FullName}'.");


        }



        var app = GetApp();
        if (app != null)
        {
            var alldef = GetAllClass();

            foreach (var def in alldef)
            {
                foreach (var method in def.GetMethods())
                {
                    try
                    {
                        //foreach (var item in method.Body.Instructions)
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            var item = method.Body.Instructions[i];

                            if (item.OpCode == OpCodes.Callvirt || item.OpCode == OpCodes.Call)
                            {
                                if (item.Operand is GenericInstanceMethod reference)
                                {
                                    if (reference.IsGenericInstance == true
                                        && reference.HasGenericParameters == false
                                        && reference.HasGenericArguments == true && reference.Parameters.Count == 0 && reference.GenericArguments.Count == 1)
                                    {
                                        var call = reference.GenericArguments[0].FullName;

                                        if (MethodDict.ContainsKey(call))
                                        {
                                            var li = method.Body.GetILProcessor();
                                            var newcall = li.Create(OpCodes.Call, MethodDict[call]);
                                            li.Replace(item, newcall);
                                        }
                                    }
                                }

                            }

                        }
                    }
                    catch
                    {

                    }
                }

            }

        }

        var actors = GetActorControllers();

        foreach (var item in actors)
        {
            AddMethod(item);
        }

        foreach (var item in GetAsyncControllers())
        {
            AddMethod(item);
        }

        foreach (var item in GetBuildClass())
        {
            AddMethod(item);
        }
    }






    TypeDefinition GetApp()
    {
        return ModuleDefinition.GetAllTypes().FirstOrDefault(p => p.IsClass && p.BaseType?.FullName == "Xamarin.Forms.Application");
    }

    TypeDefinition[] GetAllClass()
    {
        var all = ModuleDefinition.GetAllTypes();
        return all.Where(p => p.IsClass).ToArray();
    }



    TypeDefinition[] GetBuildClass()
    {

        return ModuleDefinition.GetAllTypes().Where(p => p.IsClass && p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "Build") != null).ToArray();
    }



    TypeDefinition[] GetAsyncControllers()
    {
        var all = ModuleDefinition.GetAllTypes();
        return all.Where(p => p.IsClass && p.BaseType?.Name == "AsyncController").ToArray();
    }


    TypeDefinition[] GetActorControllers()
    {
        var all = ModuleDefinition.GetAllTypes();
        return all.Where(p => p.IsClass && p.BaseType?.Name == "ActorController").ToArray();
    }


    TypeDefinition[] GetBuildInterfaces()
    {

        var all = ModuleDefinition.GetAllTypes();
        return ModuleDefinition.GetAllTypes().Where(p => p.IsInterface && p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "Build") != null).ToArray();
    }


    void AddMethod(TypeDefinition typeDefinition)
    {
        if (typeDefinition.Methods.FirstOrDefault(p => p.Name == "Runs__Make") != null)
            return;

        var NewMethod = new MethodDefinition("Runs__Make", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, ModuleDefinition.ImportReference(typeof(object)));

        var il = NewMethod.Body.GetILProcessor();

        NewMethod.Parameters.Add(new ParameterDefinition("tag", ParameterAttributes.HasDefault, ModuleDefinition.ImportReference(typeof(Int32))));
        NewMethod.Parameters.Add(new ParameterDefinition("args", ParameterAttributes.HasDefault, ModuleDefinition.ImportReference(typeof(object[]))));


        Dictionary<int, MethodReference> methods = new Dictionary<int, MethodReference>();


        foreach (var ifacer in typeDefinition.Interfaces)
        {
            var iface = ifacer.InterfaceType.Resolve();

            if (iface != null)
            {
                var build = iface.CustomAttributes.FirstOrDefault(p => p.AttributeType.Name == "Build");
                if (build != null)
                {

                    LogInfo("USE INTERFACE DEF  Runs__Make");

                    foreach (var method in iface.Methods.Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null))
                    {
                        var tag = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG");
                        if (tag is null)
                            return;
                        var cmdx = tag.ConstructorArguments.First().Value;
                        var cmd = 0;
                        switch (cmdx)
                        {
                            case Mono.Cecil.CustomAttributeArgument args:
                                cmd = (int)args.Value;
                                break;
                            default:
                                cmd = (int)cmdx;
                                break;
                        }


                        var childmethod = typeDefinition.Methods.FirstOrDefault(p => CheckMethod(p, method));

                        if (childmethod != null)
                        {
                            if (typeDefinition.GenericParameters.Count > 0)
                            {
                                var types = typeDefinition.GenericParameters.ToArray();
                                var genericmethod = ModuleDefinition.ImportReference(MakeGeneric(childmethod, types));
                                methods[cmd] = genericmethod;
                            }
                            else
                                methods[cmd] = childmethod;
                        }
                        else
                        {
                            LogError("not find " + method.Name);
                        }
                    }
                }
            }
            else
            {
                string dllname = System.Environment.CurrentDirectory + "/" + ifacer.InterfaceType.Scope.Name + ".dll";
                try
                {
                    var module = Mono.Cecil.ModuleDefinition.ReadModule(dllname);

                    var ifacec = module?.GetType(ifacer.InterfaceType.FullName);

                    ifacec = module.ImportReference(ifacec).Resolve();

                    var build = ifacec.CustomAttributes.FirstOrDefault(p => p.AttributeType.Name == "Build");
                    if (build != null)
                    {

                        LogInfo("USE INTERFACE DEF  Runs__Make");

                        foreach (var method in ifacec.Methods.Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null))
                        {
                            var tag = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG");
                            if (tag is null)
                                return;
                            var cmdx = tag.ConstructorArguments.First().Value;
                            var cmd = 0;
                            switch (cmdx)
                            {
                                case Mono.Cecil.CustomAttributeArgument args:
                                    cmd = (int)args.Value;
                                    break;
                                default:
                                    cmd = (int)cmdx;
                                    break;
                            }

                            var childmethod = typeDefinition.Methods.FirstOrDefault(p => CheckMethod(p, method));
                            if (childmethod != null)
                                methods[cmd] = childmethod;
                            else
                                LogError("not find " + method.Name);

                        }
                    }

                }
                catch (System.IO.FileNotFoundException)
                {
                    LogError("not find " + dllname);
                }
            }
        }


        foreach (var method in typeDefinition.GetMethods().Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null))
        {
            var tag = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG");
            if (tag is null)
                return;
            var cmdx = tag.ConstructorArguments.First().Value;
            var cmd = 0;
            switch (cmdx)
            {
                case Mono.Cecil.CustomAttributeArgument args:
                    cmd = (int)args.Value;
                    break;
                default:
                    cmd = (int)cmdx;
                    break;
            }

            methods[cmd] = method;

        }


        if (methods.Count > 0)
        {
            List<MakeBne> benList = new List<MakeBne>();

            foreach (var methodkv in methods)
            {
                MakeBne bne = new MakeBne(il)
                {
                    Cmd = methodkv.Key,
                    Method = methodkv.Value,
                    ModuleWeaver = this
                };
                benList.Add(bne);
            }


            for (int i = 0; i < benList.Count; i++)
            {
                if (i + 1 < benList.Count)
                {
                    benList[i].Next = benList[i + 1].Start;
                }
                else
                {
                    benList[i].IsEnd = true;
                }
            }


            foreach (var p in benList)
            {
                foreach (var item in p.MakeCode())
                {
                    il.Append(item);
                }
            }

            if (benList.Count > 0)
                typeDefinition.Methods.Add(NewMethod);
        }
    }


    private bool CheckMethod(MethodDefinition A, MethodDefinition B)
    {
        if (A.Name != B.Name)
            return false;
        if (A.Parameters.Count != B.Parameters.Count)
            return false;

        for (int i = 0; i < A.Parameters.Count; i++)
        {
            if (A.Parameters[i].ParameterType.FullName != B.Parameters[i].ParameterType.FullName)
                return false;
        }

        return true;
    }

    public List<Instruction> MakeBneCall(ILProcessor processor, MethodDefinition method, int cmd, Instruction next)
    {
        var codes = new List<Instruction>
        {
           processor.Create(OpCodes.Ldarg_1),
           processor.Create(OpCodes.Ldc_I4,cmd),
           processor.Create(OpCodes.Bne_Un_S,next),
           processor.Create(OpCodes.Ldarg_0)
        };

        int i = 0;
        foreach (var parmeter in method.Parameters)
        {
            codes.Add(processor.Create(OpCodes.Ldarg_2));
            codes.Add(processor.Create(OpCodes.Ldc_I4, i));
            codes.Add(processor.Create(OpCodes.Ldelem_Ref));
            Convert(processor, codes, ModuleDefinition.ImportReference(typeof(object)), parmeter.ParameterType, false);
        }

        codes.Add(processor.Create(OpCodes.Callvirt, method));
        codes.Add(processor.Create(OpCodes.Ret));

        return codes;
    }




    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }


    MethodDefinition AddGetImplementation(TypeDefinition newType, TypeDefinition infice, MethodDefinition ctor)
    {
        var method = new MethodDefinition("GetImplementation", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, infice);
        method.Parameters.Add(new ParameterDefinition("netx_interface", ParameterAttributes.HasDefault, ModuleDefinition.ImportReference(typeof(INetxBuildInterface))));
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Newobj, ctor);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
        return method;
    }

    MethodDefinition AddConstructor(TypeDefinition newType)
    {
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, TypeSystem.VoidReference);
        var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());

        method.Parameters.Add(new ParameterDefinition("netx_interface", ParameterAttributes.HasDefault, ModuleDefinition.ImportReference(typeof(INetxBuildInterface))));
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
        return method;
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

    public GenericInstanceMethod MakeGenericInstanceMethod(MethodReference self, params TypeReference[] arguments)
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
        var cmdx = tag.ConstructorArguments.First().Value;
        var cmd = 0;
        switch (cmdx)
        {
            case Mono.Cecil.CustomAttributeArgument args:
                cmd = (int)args.Value;
                break;
            default:
                cmd = (int)cmdx;
                break;
        }


        var method = new MethodDefinition(irpc.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, irpc.ReturnType);

        var il = method.Body.GetILProcessor();

        var parameters = irpc.Parameters;
        var paramTypes = ParamTypes(parameters.ToArray(), false);

        for (int i = 0; i < paramTypes.Length; i++)
        {
            method.Parameters.Add(new ParameterDefinition(parameters[i].Name, parameters[i].Attributes, paramTypes[i]));
        }

        if (irpc.ContainsGenericParameter)
        {
            throw new Exception($"not have generic parameter{irpc.FullName}");
        }

        var returntype = irpc.ReturnType;

        if (returntype.IsGenericInstance)
        {
            var genreturntype = returntype as GenericInstanceType;

            if (genreturntype.Name != "Task`1")
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
        else if (returntype.FullName == "System.Void")
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
        else if (returntype.FullName == "System.Threading.Tasks.Task")
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

