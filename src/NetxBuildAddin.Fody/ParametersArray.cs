using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public partial class ModuleWeaver
{

    private static readonly OpCode[] s_convOpCodes = new OpCode[] {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Conv_I1,//Boolean = 3,
                OpCodes.Conv_I2,//Char = 4,
                OpCodes.Conv_I1,//SByte = 5,
                OpCodes.Conv_U1,//Byte = 6,
                OpCodes.Conv_I2,//Int16 = 7,
                OpCodes.Conv_U2,//UInt16 = 8,
                OpCodes.Conv_I4,//Int32 = 9,
                OpCodes.Conv_U4,//UInt32 = 10,
                OpCodes.Conv_I8,//Int64 = 11,
                OpCodes.Conv_U8,//UInt64 = 12,
                OpCodes.Conv_R4,//Single = 13,
                OpCodes.Conv_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Nop,//String = 18,
            };

    private static readonly OpCode[] s_ldindOpCodes = new OpCode[] {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Ldind_I1,//Boolean = 3,
                OpCodes.Ldind_I2,//Char = 4,
                OpCodes.Ldind_I1,//SByte = 5,
                OpCodes.Ldind_U1,//Byte = 6,
                OpCodes.Ldind_I2,//Int16 = 7,
                OpCodes.Ldind_U2,//UInt16 = 8,
                OpCodes.Ldind_I4,//Int32 = 9,
                OpCodes.Ldind_U4,//UInt32 = 10,
                OpCodes.Ldind_I8,//Int64 = 11,
                OpCodes.Ldind_I8,//UInt64 = 12,
                OpCodes.Ldind_R4,//Single = 13,
                OpCodes.Ldind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Ldind_Ref,//String = 18,
            };

    private static readonly OpCode[] s_stindOpCodes = new OpCode[] {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Stind_I1,//Boolean = 3,
                OpCodes.Stind_I2,//Char = 4,
                OpCodes.Stind_I1,//SByte = 5,
                OpCodes.Stind_I1,//Byte = 6,
                OpCodes.Stind_I2,//Int16 = 7,
                OpCodes.Stind_I2,//UInt16 = 8,
                OpCodes.Stind_I4,//Int32 = 9,
                OpCodes.Stind_I4,//UInt32 = 10,
                OpCodes.Stind_I8,//Int64 = 11,
                OpCodes.Stind_I8,//UInt64 = 12,
                OpCodes.Stind_R4,//Single = 13,
                OpCodes.Stind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Stind_Ref,//String = 18,
            };

    // TypeCode does not exist in ProjectK or ProjectN.
    // This lookup method was copied from PortableLibraryThunks\Internal\PortableLibraryThunks\System\TypeThunks.cs
    // but returns the integer value equivalent to its TypeCode enum.
    private int GetTypeCode(TypeReference type)
    {
        if (type == null)
            return 0;   // TypeCode.Empty;

        if (type == ModuleDefinition.ImportReference(typeof(bool)))
            return 3;   // TypeCode.Boolean;

        if (type == ModuleDefinition.ImportReference(typeof(char)))
            return 4;   // TypeCode.Char;

        if (type == ModuleDefinition.ImportReference(typeof(sbyte)))
            return 5;   // TypeCode.SByte;

        if (type == ModuleDefinition.ImportReference(typeof(byte)))
            return 6;   // TypeCode.Byte;

        if (type == ModuleDefinition.ImportReference(typeof(short)))
            return 7;   // TypeCode.Int16;

        if (type == ModuleDefinition.ImportReference(typeof(ushort)))
            return 8;   // TypeCode.UInt16;

        if (type == ModuleDefinition.ImportReference(typeof(int)))
            return 9;   // TypeCode.Int32;

        if (type == ModuleDefinition.ImportReference(typeof(uint)))
            return 10;  // TypeCode.UInt32;

        if (type == ModuleDefinition.ImportReference(typeof(long)))
            return 11;  // TypeCode.Int64;

        if (type == ModuleDefinition.ImportReference(typeof(ulong)))
            return 12;  // TypeCode.UInt64;

        if (type == ModuleDefinition.ImportReference(typeof(float)))
            return 13;  // TypeCode.Single;

        if (type == ModuleDefinition.ImportReference(typeof(double)))
            return 14;  // TypeCode.Double;

        if (type == ModuleDefinition.ImportReference(typeof(decimal)))
            return 15;  // TypeCode.Decimal;

        if (type == ModuleDefinition.ImportReference(typeof(DateTime)))
            return 16;  // TypeCode.DateTime;

        if (type == ModuleDefinition.ImportReference(typeof(string)))
            return 18;  // TypeCode.String;

        var ttype = Type.GetType(type.FullName);

        if (ttype != null && ttype.GetTypeInfo().IsEnum)
            return GetTypeCode(ModuleDefinition.ImportReference(Enum.GetUnderlyingType(ttype)));

        return 1;   // TypeCode.Object;
    }

    private class ParametersArray
    {
        private readonly ILProcessor _il;
        private readonly TypeReference[] _paramTypes;
        private readonly ModuleWeaver _moduleweaver;
        internal ParametersArray(ModuleWeaver moduleweaver, ILProcessor il, TypeReference[] paramTypes)
        {
            _moduleweaver = moduleweaver;
            _il = il;
            _paramTypes = paramTypes;
        }

        internal void Get(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void BeginSet(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void EndSet(int i, TypeReference stackType)
        {
            Debug.Assert(_paramTypes[i].IsByReference);
            TypeReference argType = _paramTypes[i].GetElementType();

            _moduleweaver.Convert(_il, stackType, argType, false);
            _moduleweaver.Stind(_il, argType);
        }
    }


    private class GenericArray<T>
    {
        private readonly ILProcessor _il;
        private readonly VariableDefinition _lb;
        private readonly ModuleWeaver _moduleweaver;
        internal GenericArray(ModuleWeaver moduleweaver, ILProcessor il, int len)
        {
            _moduleweaver = moduleweaver;
            _il = il;
            _lb = new VariableDefinition(_moduleweaver.ModuleDefinition.ImportReference(typeof(T[])));
            il.Body.Variables.Add(_lb);

            il.Emit(OpCodes.Ldc_I4, len);
            il.Emit(OpCodes.Newarr, _moduleweaver.ModuleDefinition.ImportReference(typeof(T)));
            il.Emit(OpCodes.Stloc, _lb);
        }

        internal void Load()
        {
            _il.Emit(OpCodes.Ldloc, _lb);
        }

        internal void Get(int i)
        {
            _il.Emit(OpCodes.Ldloc, _lb);
            _il.Emit(OpCodes.Ldc_I4, i);
            _il.Emit(OpCodes.Ldelem_Ref);
        }

        internal void BeginSet(int i)
        {
            _il.Emit(OpCodes.Ldloc, _lb);
            _il.Emit(OpCodes.Ldc_I4, i);
        }

        internal void EndSet(TypeReference stackType)
        {
            _moduleweaver.Convert(_il, stackType, _moduleweaver.ModuleDefinition.ImportReference(typeof(T)), false);
            _il.Emit(OpCodes.Stelem_Ref);
        }
    }

    public List<Instruction> Convert(ILProcessor il, List<Instruction> opCodes, TypeReference source, TypeReference target, bool isAddress)
    {
        Debug.Assert(!target.IsByReference);
        if (target == source)
            return opCodes;


        if (source.IsByReference)
        {
            Debug.Assert(!isAddress);
            TypeReference argType = source.GetElementType();

            OpCode opCode = s_ldindOpCodes[GetTypeCode(argType)];
            if (!opCode.Equals(OpCodes.Nop))
            {
                opCodes.Add(il.Create(opCode));
            }
            else
            {
                opCodes.Add(il.Create(OpCodes.Ldobj, argType));
            }

            Convert(il, opCodes, argType, target, isAddress);
            return opCodes;
        }
        if (target.IsValueType)
        {
            if (source.IsValueType)
            {
                OpCode opCode = s_convOpCodes[GetTypeCode(target)];
                Debug.Assert(!opCode.Equals(OpCodes.Nop));
                opCodes.Add(il.Create(opCode));
            }
            else
            {
                Debug.Assert(IsAssignableFrom(source, target));
                opCodes.Add(il.Create(OpCodes.Unbox_Any, ModuleDefinition.ImportReference(target)));
                if (!isAddress)
                {
                    OpCode opCode = s_ldindOpCodes[GetTypeCode(target)];
                    if (!opCode.Equals(OpCodes.Nop))
                    {
                        opCodes.Add(il.Create(opCode));
                    }
                    else
                    {
                        opCodes.Add(il.Create(OpCodes.Ldobj, target));
                    }
                }
            }
        }
        else if (IsAssignableFrom(target, source))
        {
            if (source.IsValueType)
            {
                if (isAddress)
                {
                    OpCode opCode = s_ldindOpCodes[GetTypeCode(source)];
                    if (!opCode.Equals(OpCodes.Nop))
                    {
                        il.Emit(opCode);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldobj, source);
                    }
                }
                opCodes.Add(il.Create(OpCodes.Box, ModuleDefinition.ImportReference(source)));
            }
        }
        else
        {

            if (target.IsGenericParameter)
            {
                opCodes.Add(il.Create(OpCodes.Unbox_Any, ModuleDefinition.ImportReference(target)));
            }
            else
            {
                opCodes.Add(il.Create(OpCodes.Castclass, ModuleDefinition.ImportReference(target)));
            }
        }

        return opCodes;
    }

    private void Convert(ILProcessor il, TypeReference source, TypeReference target, bool isAddress)
    {
        Debug.Assert(!target.IsByReference);
        if (target == source)
            return;


        if (source.IsByReference)
        {
            Debug.Assert(!isAddress);
            TypeReference argType = source.GetElementType();
            Ldind(il, argType);
            Convert(il, argType, target, isAddress);
            return;
        }
        if (target.IsValueType)
        {
            if (source.IsValueType)
            {
                OpCode opCode = s_convOpCodes[GetTypeCode(target)];
                Debug.Assert(!opCode.Equals(OpCodes.Nop));
                il.Emit(opCode);
            }
            else
            {
                Debug.Assert(IsAssignableFrom(source, target));
                il.Emit(OpCodes.Unbox, ModuleDefinition.ImportReference(target));
                if (!isAddress)
                    Ldind(il, target);
            }
        }
        else if (IsAssignableFrom(target, source))
        {
            if (source.IsValueType)
            {
                if (isAddress)
                    Ldind(il, source);
                il.Emit(OpCodes.Box, ModuleDefinition.ImportReference(source));
            }
        }
        else
        {
            Debug.Assert(IsAssignableFrom(source, target) || target.Resolve().IsInterface || source.Resolve().IsInterface);
            if (target.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, ModuleDefinition.ImportReference(target));
            }
            else
            {
                il.Emit(OpCodes.Castclass, ModuleDefinition.ImportReference(target));
            }
        }
    }

    public bool IsAssignableFrom(TypeReference souce, TypeReference target)
    {
        try
        {
            if (target == null || souce == null)
            {
                return false;
            }
            if (!souce.FullName.Equals(target.FullName))
            {
                if (IsSubclassOf(target, souce))
                {
                    return true;
                }
                if (souce.Resolve().BaseType.Resolve().IsInterface)
                {
                    return ImplementInterface(target, souce);

                }
                if (!souce.IsGenericParameter)
                {
                    return false;
                }
                TypeReference[] genericParameterConstraints = souce.GenericParameters.ToArray();
                for (int i = 0; i < genericParameterConstraints.Length; i++)
                {
                    if (!IsAssignableFrom(genericParameterConstraints[i], target))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        catch (Exception er)
        {
            WriteInfo(er.ToString());
            return false;
        }
    }

    public bool IsSubclassOf(TypeReference souce, TypeReference target)
    {
        TypeReference baseType = souce;
        if (!(baseType.FullName.Equals(target.FullName)))
        {
            while (baseType != null)
            {
                if (baseType.FullName.Equals(target.FullName))
                {
                    return true;
                }
                baseType = baseType.Resolve().BaseType;
            }
            return false;
        }
        return false;
    }

    internal bool ImplementInterface(TypeReference source, TypeReference ifaceType)
    {
        for (TypeReference type = source; type != null; type = type.Resolve().BaseType)
        {
            InterfaceImplementation[] interfaces = type.Resolve().Interfaces.ToArray();
            if (interfaces != null)
            {
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if ((interfaces[i].InterfaceType.FullName.Equals(ifaceType.FullName)) || ((interfaces[i] != null) && ImplementInterface(interfaces[i].InterfaceType, ifaceType)))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    private void Ldind(ILProcessor il, TypeReference type)
    {
        OpCode opCode = s_ldindOpCodes[GetTypeCode(type)];
        if (!opCode.Equals(OpCodes.Nop))
        {
            il.Emit(opCode);
        }
        else
        {
            il.Emit(OpCodes.Ldobj, type);
        }
    }

    private void Stind(ILProcessor il, TypeReference type)
    {
        OpCode opCode = s_stindOpCodes[GetTypeCode(type)];
        if (!opCode.Equals(OpCodes.Nop))
        {
            il.Emit(opCode);
        }
        else
        {
            il.Emit(OpCodes.Stobj, type);
        }
    }
}

