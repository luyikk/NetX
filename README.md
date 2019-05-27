# NetX
**一款使用[ZYSOCKET-V](https://github.com/luyikk/ZYSOCKET-V)实现的 Actor+RPC 服务,使用它你可以随随便便做出高性能服务器,以及彻底解决各种锁的问题**

**使用方式:
NUGET方式:**

    NetX服务器: Install-Package NetxServer
    NetX客户端: Install-Package NetxClient
    NetX客户端(手机版可用于IOS): Install-Package NetxClient-Portable
    NetX代理网关:暂未开发
    NetX共享ActorService:暂未开发
    NETXActor(ACTOR本机运行版) : Install-Package NetxActor
    
**源代码:[github src](https://github.com/luyikk/NetX)**


NetX原理结构图:
![NetX原理结构图](https://github.com/luyikk/NetX/blob/master/Images/NetX1.png)
OR

![NetX原理结构图2](https://github.com/luyikk/NetX/blob/master/Images/NetX6.png)

通过此框架我们可以轻松的构建 如图这样的服务网络,他的性能非常好的,大概比[Orleans](https://github.com/dotnet/orleans)性能高出5倍以上,内存只需要[orleans](https://github.com/dotnet/orleans)5分之一.功能强大,Actor,RPC,Event Sourcing,Wake up to sleep,负载均衡,服务路由,服务器主动调用客户端...等等功能.可实现Orleans无法实现的功能.

## 下面逐步介绍各个功能模块:

**NetXServer:**

NetXServer结构图:
![NetXServer结构图](https://github.com/luyikk/NetX/blob/master/Images/NetX2.png)

在Server 中 我们一共有2种控制器,一种是RPC控制器,一种是ACTOR控制器.

RPC控制器:
可以定制每个链接的服务.他们是每个链接一个控制器.各个链接他们的RPC控制器是独立的.对于当前连接来说每个RPC控制器的功能是线程安全的. 但是,如果多个RPC控制室他们之间共享的数据,例如数据集合,它们线程是不安全的.如果要全局线程安全的话,所以我们这里需要一个ACTOR控制器.RPC控制器可以直接调用Actor控制器,或者直接调用Client定义的功能函数.

ACTOR控制器:
此控制器是全局线程安全的,每个ACTOR控制器都被一个叫ActorRun的Actor管理类所管理着,他们是实例全局唯一的.Actor容器中含有各种功能,例如 沉睡,唤醒,事件存储等. 他可以被RPC控制器,其他的Actor控制器,或者客户端直接调用.

这2种控制器都是通过TAG 管理服务的.所谓的TAG就是一个int类型的标签.它用来表示所有的开发性功能函数.TAG全局唯一的.甚至这2种控制器都不能重复定义. 因为使用了TAG.所以我们的Actor他们没有继承关系,每个ACTOR都是独立运行的.所以没有ACTOR功能路由,因为不需要,我们只需要通过 TAG就可以直接 一对一的功能调用.

所有的功能模块都定义在DependencyInjection中.在控制器编写时它非常像 Asp.net Core 写一个控制器.我们可以随时从DependencyInjection拿取一个组件.

最后他们是通过ZYSOCKET-V 也就是我第5代SOCKET框架通讯的.从而保障了服务器的性能.

DEMO里面的例子启动一个服务器:
   
            var server = new NetxServBuilder()
                 .ConfigBase(p =>
                 {
                     p.ServiceName = "MessageService"; //服务名
                     p.VerifyKey = "123123";  //密码
                     p.ClearSessionTime = 60000; //Session清理时间
                 })
                  .ConfigSSL(p =>  //配置SSL加密
                  {
                      p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                          $"{new System.IO.DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent}/server.pfx",
                          "testPassword");
                      p.IsUse = true;
                  })
                  .ConfigureLogSet(p=> //设置日记
                  {
                      p.AddConsole();
                      p.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); //过滤EF日记
                  })
                 .ConfigNetWork(p =>
                 {
                    // p.Host = "any";  //监听所有IP
                     p.Port = 3000; //服务端口
                 })
                 .RegisterService(Assembly.GetExecutingAssembly()) //加载当前DLL里面的所有控制器
                 .RegisterDescriptors(p=>p.AddSingleton<UserManager, UserManager>()) //添加用户管理器
                 .RegisterDescriptors(p=>p.AddDbContext<UserDatabaseContext>(option=> //设置SQL
                 {
                     option.UseSqlite("Data Source=UserDatabase.db3");

                 }))
                 .Build();

           server.Start(); //启动服务

我们可以定义 一组Actor控制器以及一组RPC控制器,它们的定义方式为:

DEMO里面RPC控制器的例子:

      public class UserAsyncController : AsyncController, IServer
       {
         .
         .
	             public UserAsyncController(ILogger<UserAsyncController> logger, UserManager userManager)
                  {
                        Log = new DefaultLog(logger);
                        this.UserLines = userManager;
                  }
       
        .
        .
        .
       }

这里需要注意我们把所有提供的服务写在了IServer中

    [Build]
    public interface IServer
    {
    
        [TAG(5002)]
        Task<(bool, string)> LogOn(string username, string password);
    
        [TAG(5003)]
        Task<(bool, User)> CheckLogIn();
    
        [TAG(5004)]
        Task<List<User>> GetUsers();
    
        [TAG(5005)]
        Task<(bool, string)> Say(long userId, string msg);
    
        [TAG(5006)]
        Task<List<LeavingMsg>> GetLeavingMessage();
    
    }

我们的控制器需要实现此接口的功能,实现完成后,客户端只要拿到此IServer接口,就可以直接调用服务器的功能模块了.

关于DEMO中Actor控制器的定义:

    [ActorOption(1000,10000)] //这里最大列队为1000,如果10秒没有任务将进入沉睡
    public class UserActorController : ActorController, IActorService
    {
        public ILog Log { get; }
    
        public UserDatabaseContext UserDatabase { get; }
    
        public UserActorController(ILogger<UserActorController> logger,UserDatabaseContext userDatabaseContext)
        {         
            Log = new DefaultLog(logger);
            UserDatabase = userDatabaseContext;
            UserDatabase.Database.EnsureCreated();
        }
        /// <summary>
        /// 沉睡的时候保存数据
        /// </summary>
        /// <returns></returns>
        public async override Task Sleeping()
        {
            if (UserDatabase.ChangeTracker.HasChanges())
            {
                var i = await UserDatabase.SaveChangesAsync();

                if (i > 0)
                    Log.Info($"save {i} row data");
            }
        }

同样我们这里需要继承IActorService,这样的话 我们的RPC,CLIENT或者其他的ACTOR就可以通过IActorService 来调用此控制器中的功能了.

详情请看[DEMO Chat Room](https://github.com/luyikk/NetX/tree/master/demo/ChatSystem)

其他详细的功能演示,我陆续会更新

**关于NetXClient:**

NetXClient结构图:
![NetXClient结构图](https://github.com/luyikk/NetX/blob/master/Images/NetX3.png)

我们可以通过NetXClient调用 Server的RPC服务,ACTOR服务,以及提供服务给服务器调用.
NetXClient提供给服务器调用的服务是线程安全的.

关于设置一个NetXClient例子:

    var icontainer = new NetxSClientBuilder()
                    .ConfigureLogSet(p =>
                    {
                        p.AddDebug().SetMinimumLevel(LogLevel.Trace); //添加DEBUG日记输出
                    })
                    .ConfigSSL(p => //设置SSL加密
                    {
                        p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                            $"{System.Windows.Forms.Application.StartupPath}/client.pfx"
                            , "testPassword");
                        p.IsUse = true;
                    })
                    .ConfigSessionStore(() => new Netx.Client.Session.SessionFile()) //如何保存Session需要下次打开 不用登陆
                    .ConfigConnection(p => //配置连接
                    {
                        p.Host = "127.0.0.1"; //IP
                        p.Port = 3000;  //端口
                        p.ServiceName = "MessageService"; //服务名称
                        p.VerifyKey = "123123"; //密码
                    });
    
       var Client= icontainer.Build(); //生成一个CLIENT

还记得上面的IServer吗?

调用服务器是需要通过TAG的,至于你CLIENT的IServer不一定要和服务器一致,我们甚至可以换个名字,或者把IActorServer和IServer2个接口的功能整合在一起. 但要注意的是TAG和 参数必须和服务器提供的一致哦

关于服务的调用方式图解:
![关于服务的调用方式图解](https://github.com/luyikk/NetX/blob/master/Images/NetX5.png)



我们通过IServer调用服务器:

    var service = Client.Get<IServer>();
    var (success, msg) = await service.LogOn("username", "password");

对就是那么简单.通用的方式一样用于调用Actor服务
如果提供可以服务给服务器调用呢?

      public partial class WinMain : Form, IMethodController, IClient
      {
        public INetxSClient Current { get=>Dependency.Client; set { } }

例如我们这个WINFROM 窗口类需要提供给服务器直接调用.我们首先要继承IMethodController接口,实现 `public INetxSClient Current { get=>Dependency.Client; set { } }`
因为每次服务器调用此类服务的时候会设置Current,所以{get;set;}即可,但是[ChaT Room Client Demo](https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/WinMain.cs)里面的这个Current有特殊的需要,所以我们就这样写吧.
[IClient](https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/Interfaces/IClient.cs) 定义了我们提供哪些方法供服务器直接调用.
例如我们搞个方法,给服务器设置用户的状态,用于显示:

     public void SetUserStats(long userid, byte status)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {

                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Tag is User user)
                    {
                        if (user.UserId == userid)
                        {
                            user.OnLineStatus = status;
                            item.SubItems[1].Text = user.OnLineStatus == 0 ? "Offline" : user.OnLineStatus == 2 ? "Leave" : "Online";
                        }
                    }
                }
            }));
        }
  
详情我们可以查看
[ClientDemo](https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/)


**NetX代理网关和NetX共享ActorService 我还没有时间开发,但是通过Server和CLIENT我们已经可以实现很多功能了!**
下面是 网关图解:
![NetXProxy](https://github.com/luyikk/NetX/blob/master/Images/NetX4.png)

**NETXActor:**
我提供了一个 ActorRun 本机版,你可以单独拿出来使用就像 AKKA.NET. 他的性能是AKKA.NET本机运行的3倍以上,达到了3M TPS, 也就是1秒钟可以处理300W个事务.

关于使用方式:
我看可以参考集成测试:
[Actor集成测试](https://github.com/luyikk/NetX/tree/master/src/Test/ActorTest)

最后我朋友看我最近在做ACTOR框架,所以他也尝试了写了一个. 据说本机性能高出了AKKA.NET 一截. 所以我把他的DEMO拿来了,添加了我的NETXActor
Src:[AKKA.NET VS event next VS NetX](https://github.com/luyikk/NetX/tree/master/src/Test/AkkaNetTpsTest)

下面是结果图:
![AKKA.NET VS event next VS NetX:](https://github.com/luyikk/NetX/blob/master/Images/Test.png)
在并行的情况下基本上都是能达到2.4M TPS的,如果是单线程测试模式也就是ActorTest里面的例子,可以达到3.2M TPC

我的电脑是I7 4770K 哦
