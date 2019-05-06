using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Server.Builder;
using Netx.Service;
using ZYSocket.Share;
using System.Buffers;
using ZYSocket.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Netx.Interface;
using Netx.Actor;

namespace Netx.Service.Builder
{
    public class NetxServBuilder : INetxServBuilder
    {
        public IServiceCollection Container { get; }
        public IServiceProvider Provider { get; private set; }
        private readonly ConcurrentDictionary<int, MethodRegister> AsyncServicesRegisterDict;
        public SockServBuilder SockServConfig { get; private set; }

        public NetxServBuilder()
        {
            Container = new ServiceCollection();
            AsyncServicesRegisterDict = new ConcurrentDictionary<int, MethodRegister>();
            Container.AddOptions();
            LoadSocketServer();
            ConfigureDefaults();
        }


        public INetxServBuilder ConfigureDefaults()
        {
            ConfigIIds();
            ConfigureLogSet();
            return this;
        }

        private void LoadSocketServer()
        {
            SockServConfig = new SockServBuilder(Container, p =>
            {
                return new ZYSocket.Server.ZYSocketSuper(p);
            });
        }

        public INetxServBuilder RegisterDescriptors(Action<IServiceCollection> serviceDescriptors)
        {
            serviceDescriptors?.Invoke(Container);
            return this;
        }

        public INetxServBuilder RegisterService(Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
                RegisterService(type);
            return this;
        }

        public INetxServBuilder RegisterService(Type controller_instance_type)
        {
            if (IsCanRegisterService(controller_instance_type))
                Container.Add(ServiceDescriptor.Transient(controller_instance_type, controller_instance_type));
            return this;
        }


        private bool IsCanRegisterService(Type instanceType)
        {
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));
            bool have = false;
            if (instanceType.BaseType == typeof(AsyncController))
            {
                var methods = instanceType.GetMethods();
                foreach (var method in methods)
                    if (method.IsPublic)
                        foreach (var attr in method.GetCustomAttributes(typeof(TAG), true))
                            if (attr is TAG attrcmdtype)
                            {
                                have = true;
                                IsRegisterCmd(attrcmdtype.CmdTag, instanceType, method);
                            }

                return have;
            }
            else if(instanceType.BaseType==typeof(ActorController))
            {
                Container.Add(ServiceDescriptor.Scoped(typeof(ActorController), instanceType));
                return false;
            }
            else
                return false;

        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="instanceType">实例类型</param>
        /// <param name="methodInfo">方法</param>
        private bool IsRegisterCmd(int cmd, Type instanceType, MethodInfo methodInfo)
        {
            if (TypeHelper.IsTypeOfBaseTypeIs(methodInfo.ReturnType, typeof(Task)) || methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == null)
            {
                var sr = new MethodRegister(instanceType, methodInfo);
                AsyncServicesRegisterDict.AddOrUpdate(cmd, sr, (a, b) => sr);
                return true;
            }
            else
                throw new NetxException("RegisterService Return Type Err:{ 0 },Use void, Task or Task<T>", ErrorType.RegisterCmdErr);
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



        public INetxServBuilder ConfigEncode(Func<Encoding> func = null)
        {
            SockServConfig.ConfigEncode(func);
            return this;
        }

        public INetxServBuilder ConfigIAsyncSend(Func<IAsyncSend> func = null)
        {
            SockServConfig.ConfigIAsyncSend(func);
            return this;
        }

        public INetxServBuilder ConfigISend(Func<ISend> func = null)
        {
            SockServConfig.ConfigISend(func);
            return this;
        }

        public INetxServBuilder ConfigMemoryPool(Func<MemoryPool<byte>> func = null)
        {
            SockServConfig.ConfigMemoryPool(func);
            return this;
        }

        public INetxServBuilder ConfigObjFormat(Func<ISerialization> func = null)
        {
            SockServConfig.ConfigObjFormat(func);
            return this;
        }

        public INetxServBuilder ConfigServer(Action<SocketServerOptions> config = null)
        {
            SockServConfig.ConfigServer(config);
            return this;
        }
        

        public INetxServBuilder ConfigureLogSet(Action<ILoggingBuilder> config = null)
        {
            if (config is null)
            {
                Container.AddLogging(p =>
                {
                    p.AddConsole();
                    p.SetMinimumLevel(LogLevel.Trace);
                });
            }
            else
            {
                Container.AddLogging(config);
            }

            return this;
        }

        public INetxServBuilder ConfigureKey(Action<OptionKey> config = null)
        {
            if (config != null)
                Container.Configure<OptionKey>(config);

            return this;
        }


        public INetxServBuilder ConfigIIds(Func<IServiceProvider,IIds> func = null)
        {
            if (func is null)
                Container.AddSingleton<IIds, DefaultMakeIds>();
            else
                Container.AddSingleton<IIds>(func);

            return this;
        }

        public INetxServBuilder ConfigSSL(Action<SslOption> config=null)
        {
            if (config != null)
                Container.Configure<SslOption>(config);
            return this;
        }


        public NetxService Build()
        {
            if (Provider is null)
            {
                Container.TryAdd(ServiceDescriptor.Singleton<ActorRun>(p => new ActorRun(p)));
                Container.TryAdd(ServiceDescriptor.Singleton<IActorGet,ActorRun>());
                Container.TryAdd(ServiceDescriptor.Singleton<NetxService>(p => new NetxService(p)));
                Container.Replace(ServiceDescriptor.Singleton<ConcurrentDictionary<int, MethodRegister>>(AsyncServicesRegisterDict));
                Provider = Container.BuildServiceProvider();
                return Provider.GetRequiredService<NetxService>();
            }else
                return Provider.GetRequiredService<NetxService>();
        }

        public void Dispose()
        {
            if (Provider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
