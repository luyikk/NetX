using Microsoft.Extensions.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netx
{
    public class MethodRegister
    {
        public Type InstanceType { get; }

        public ObjectMethodExecutor Method { get; }

        public Type[] ArgsType { get; }

        public Type ReturnType { get; }

        public ReturnTypeMode ReturnMode { get; }

        public int ArgsLen { get => ArgsType.Length; }


        public MethodRegister(Type instenceType, MethodInfo method)
        {
            this.InstanceType = instenceType;
            this.Method = ObjectMethodExecutor.Create(method, instenceType.GetTypeInfo());

            ArgsType = (from p in method.GetParameters()
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
            str.Append(Method.MethodInfo.Name + "(");

            int i = 0;
            foreach (var item in Method.MethodInfo.GetParameters())
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
