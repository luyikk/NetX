using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netx
{
    public class MethodRegister
    {
        public Type InstanceType { get; }

        public MethodInfo Method { get; }

        public Type[] ArgsType { get; }

        public Type ReturnType { get; }

        public ReturnTypeMode ReturnMode { get; }

        public int ArgsLen { get => ArgsType.Length; }


        public MethodRegister(Type instenceType, MethodInfo method)
        {
            this.InstanceType = instenceType;
            this.Method = method;

            ArgsType = (from p in Method.GetParameters()
                        select p.ParameterType).ToArray();

            ReturnType = method.ReturnType;

            if (ReturnType == null || ReturnType == typeof(void))
                ReturnMode = ReturnTypeMode.Null;
            else if (ReturnType == typeof(Task))
                ReturnMode = ReturnTypeMode.Task;
            else
                ReturnMode = ReturnTypeMode.TaskValue;
        }

        public override string ToString()
        {
            var str = new StringBuilder(ReturnType.Name);
            str.Append(" ");
            str.Append(InstanceType.FullName);
            str.Append(".");
            str.Append(Method.Name + "(");

            int i = 0;
            foreach (var item in Method.GetParameters())
            {
                str.Append(item.ParameterType.Name);
                str.Append(" ");
                str.Append(item.Name);

                i++;
                if (i < ArgsType.Length)
                    str.Append(",");
            }

            str.Append(")");

            return str.ToString();
        }
    }
}
