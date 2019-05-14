using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Netx.Client
{
    public abstract class NetxAsyncRegisterInstance : NetxClientBase
    {
        private readonly Lazy<Dictionary<int, InstanceRegister>> methodInstanceDict;

        public Dictionary<int, InstanceRegister> MethodInstanceDict { get => methodInstanceDict.Value; }

        public NetxAsyncRegisterInstance(IServiceProvider container)
        : base(container)
        {
            methodInstanceDict = new Lazy<Dictionary<int, InstanceRegister>>();
        }

        /// <summary>
        /// 加载实例
        /// </summary>
        /// <param name="instance"></param>
        public virtual void LoadInstance(object instance)
        {
            var instancetype = instance.GetType();


            foreach (var ainterface in instancetype.GetInterfaces())
            {
                var methods = ainterface.GetMethods();

                foreach (var method in methods)
                    foreach (var attr in method.GetCustomAttributes< TAG >(true))
                        IsRegisterCmd(instance, attr.CmdTag, instancetype, method);
            }
                        

            {
                var methods = instancetype.GetMethods();
                foreach (var method in methods)
                    if (method.IsPublic)
                        foreach (var attr in method.GetCustomAttributes<TAG>(true))                            
                                IsRegisterCmd(instance, attr.CmdTag, instancetype, method);

            }
           
        }

        /// <summary>
        /// 删除实例
        /// </summary>
        /// <param name="instacne"></param>
        public virtual void RemoveInstance(object instacne)
        {
            var removetable = new List<int>();

            foreach (var item in MethodInstanceDict)
                if (item.Value.Instance == instacne)
                    removetable.Add(item.Key);

            foreach (var key in removetable)
                MethodInstanceDict.Remove(key);

        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="instanceType">实例类型</param>
        /// <param name="methodInfo">方法</param>
        private void IsRegisterCmd(object instance, int cmd, Type instanceType, MethodInfo methodInfo)
        {
            if (IsTypeOfBaseTypeIs(methodInfo.ReturnType, typeof(Task)) || methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == null)
            {
                var sr = new InstanceRegister(instance,instanceType, methodInfo);
                if (!MethodInstanceDict.ContainsKey(cmd))
                {
                    Log.Info($"Add cmd:{cmd} to {sr}");
                    MethodInstanceDict.Add(cmd, sr);
                }
                else
                {
                    Log.Info($"Replace cmd:{cmd} to {sr}");
                    MethodInstanceDict[cmd] = sr;
                }
            }
            else
                Log.Error($"RegisterService Return Type {methodInfo.Name} Err,Use void, Task or Task<T>");
        }

        /// <summary>
        /// 检测是否是此类型或者父类是否是此类型
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>结果</returns>
        private bool IsTypeOfBaseTypeIs(Type type, Type targetType)
        {
            if (type == targetType)
                return true;

            if (type.BaseType == null)
                return false;

            if (type.BaseType == targetType)
                return true;
            else
                return IsTypeOfBaseTypeIs(type.BaseType, targetType);
        }



    }
}
