using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Actor;
using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ZYSocket.Interface;

namespace Netx.Actor.Builder
{
    public class ActorBuilder : IActorBuilder
    {
        public IServiceCollection Container { get; }
        public IServiceProvider? Provider { get; private set; }

        public ActorBuilder(IServiceCollection? serviceDescriptors = null)
        {
            if (serviceDescriptors is null)
                Container = new ServiceCollection();
            else
                Container = serviceDescriptors;

            Container.AddScoped<IActorRun, ActorRun>();
            Container.AddOptions();           
            ConfigDefualt();

        }

        private void ConfigDefualt()
        {
            ConfigIIds();
            ConfigObjFormat();
            ConfigureLogSet();
            ConfigureActorScheduler();
        }

        /// <summary>
        /// 配置Id生产器
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public IActorBuilder ConfigIIds(Func<IServiceProvider, IIds>? func = null)
        {
            if (func is null)
                Container.AddSingleton<IIds, DefaultMakeIds>();
            else
                Container.AddSingleton<IIds>(func);

            return this;
        }


        /// <summary>
        /// 注册DI
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public IActorBuilder RegisterDescriptors(Action<IServiceCollection>? serviceDescriptors)
        {
            serviceDescriptors?.Invoke(Container);
            return this;
        }

        /// <summary>
        /// 注册控制器
        /// </summary>
        /// <param name="controller_instance_type">控制器</param>
        /// <returns></returns>
        public IActorBuilder RegisterService<ActorType>()
            where ActorType : ActorController
        {
            Container.AddSingleton<ActorController, ActorType>();
            return this;
        }

        /// <summary>
        /// 注册DLL
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public IActorBuilder RegisterService(Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
                if (type.BaseType == typeof(ActorController))
                    RegisterService(type);
            return this;
        }

        /// <summary>
        /// 根据类型注册
        /// </summary>
        /// <param name="controller_instance_type">控制器类型</param>
        /// <returns></returns>
        public IActorBuilder RegisterService(Type controller_instance_type)
        {
            Container.Add(ServiceDescriptor.Singleton(typeof(ActorController), controller_instance_type));
            return this;
        }

        /// <summary>
        /// 配置对象序列化接口
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public IActorBuilder ConfigObjFormat(Func<ISerialization>? func = null)
        {
            if (func is null)
                Container.AddSingleton<ISerialization>(p => new ZYSocket.FiberStream.ProtobuffObjFormat());
            else
                Container.AddSingleton<ISerialization>(p => func());

            return this;
        }

        /// <summary>
        /// 配置日记，默认支持控制台输出
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IActorBuilder ConfigureLogSet(Action<ILoggingBuilder>? config = null)
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

        /// <summary>
        /// 配置调度器
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public IActorBuilder ConfigureActorScheduler(Func<IServiceProvider, ActorScheduler>? func = null)
        {
            if (func is null)
                Container.AddScoped<ActorScheduler>(_ => ActorScheduler.LineByLine);
            else
                Container.AddScoped<ActorScheduler>(func);

            return this;
        }

        /// <summary>
        /// 支持Lambda 方式
        /// </summary>
        /// <returns></returns>
        public IActorBuilder UseActorLambda()
        {
            this.RegisterService<LambdaController>();
            return this;
        }

        /// <summary>
        /// 配置日记输出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IActorBuilder AddActorEvent<T>() where T : ActorEventBase
        {
            Container.AddSingleton<ActorEventBase, T>();
            return this;
        }

        /// <summary>
        /// 编译
        /// </summary>
        /// <returns></returns>
        public IActorRun Build()
        {
            lock (this)
            {
                if (Provider is null)
                {                   
                    Provider = Container.BuildServiceProvider();
                }
            }

            return Provider.GetRequiredService<IActorRun>();
        }



        public void Dispose()
        {
            if (Provider is IDisposable iprovider)
                iprovider.Dispose();           
        }
    }
}
