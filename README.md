---


---

<h1 id="netx">NetX</h1>
<p>一款实用SOCKET实现的 Actor+RPC 服务<br>
NUGET:</p>
<pre><code>NetX服务器: Install-Package NetxServer
NetX客户端: Install-Package NetxClient
NetX客户端(手机版可用于IOS): Install-Package NetxClient-Portable
NetX代理网关:暂未开发
NetX共享ActorService:暂未开发
NETXActor(ACTOR本机运行版) : Install-Package NetxActor
</code></pre>
<p><img src="https://github.com/luyikk/NetX/blob/master/Images/NetX1.png" alt="NetX原理结构图"></p>
<p>通过此框架我们可以轻松的构建 如图这样的服务网络,他的性能非常好的,大概比<a href="https://github.com/dotnet/orleans">Orleans</a>性能高出5倍以上,内存只需要<a href="https://github.com/dotnet/orleans">orleans</a>5分之一.功能强大,Actor,RPC,Event Sourcing,Wake up to sleep,负载均衡,服务路由,服务器主动调用客户端…等等功能.可实现Orleans无法实现的功能.</p>
<h2 id="下面逐步介绍各个功能模块">下面逐步介绍各个功能模块:</h2>
<p><strong>NetXServer:</strong><br>
<img src="https://github.com/luyikk/NetX/blob/master/Images/NetX2.png" alt="NetXServer结构图"></p>
<p>在Server 中 我们一共有2种控制器,一种是RPC控制器,一种是ACTOR控制器.</p>
<p>RPC控制器:<br>
可以定制每个链接的服务.他们是每个链接一个控制器.各个链接他们的RPC控制器是独立的.对于当前连接来说每个RPC控制器的功能是线程安全的. 但是,如果多个RPC控制室他们之间共享的数据,例如数据集合,它们线程是不安全的.如果要全局线程安全的话,所以我们这里需要一个ACTOR控制器.RPC控制器可以直接调用Actor控制器,或者直接调用Client定义的功能函数.</p>
<p>ACTOR控制器:<br>
此控制器是全局线程安全的,每个ACTOR控制器都被一个叫ActorRun的Actor管理类所管理着,他们是实例全局唯一的.Actor容器中含有各种功能,例如 沉睡,唤醒,事件存储等. 他可以被RPC控制器,其他的Actor控制器,或者客户端直接调用.</p>
<p>这2种控制器都是通过TAG 管理服务的.所谓的TAG就是一个int类型的标签.它用来表示所有的开发性功能函数.TAG全局唯一的.甚至这2种控制器都不能重复定义. 因为使用了TAG.所以我们的Actor他们没有继承关系,每个ACTOR都是独立运行的.所以没有ACTOR功能路由,因为不需要,我们只需要通过 TAG就可以直接 一对一的功能调用.</p>
<p>所有的功能模块都定义在DependencyInjection中.在控制器编写时它非常像 <a href="http://Asp.net">Asp.net</a> Core 写一个控制器.我们可以随时从DependencyInjection拿取一个组件.</p>
<p>最后他们是通过ZYSOCKET-V 也就是我第5代SOCKET框架通讯的.从而保障了服务器的性能.</p>
<p>DEMO里面的例子启动一个服务器:</p>
<pre><code>        var server = new NetxServBuilder()
             .ConfigBase(p =&gt;
             {
                 p.ServiceName = "MessageService"; //服务名
                 p.VerifyKey = "123123";  //密码
                 p.ClearSessionTime = 60000; //Session清理时间
             })
              .ConfigSSL(p =&gt;  //配置SSL加密
              {
                  p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                      $"{new System.IO.DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent}/server.pfx",
                      "testPassword");
                  p.IsUse = true;
              })
              .ConfigureLogSet(p=&gt; //设置日记
              {
                  p.AddConsole();
                  p.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); //过滤EF日记
              })
             .ConfigNetWork(p =&gt;
             {
                // p.Host = "any";  //监听所有IP
                 p.Port = 3000; //服务端口
             })
             .RegisterService(Assembly.GetExecutingAssembly()) //加载当前DLL里面的所有控制器
             .RegisterDescriptors(p=&gt;p.AddSingleton&lt;UserManager, UserManager&gt;()) //添加用户管理器
             .RegisterDescriptors(p=&gt;p.AddDbContext&lt;UserDatabaseContext&gt;(option=&gt; //设置SQL
             {
                 option.UseSqlite("Data Source=UserDatabase.db3");

             }))
             .Build();

       server.Start(); //启动服务
</code></pre>
<p>我们可以定义 一组Actor控制器以及一组RPC控制器,它们的定义方式为:</p>
<p>DEMO里面RPC控制器的例子:</p>
<pre><code>  public class UserAsyncController : AsyncController, IServer
   {
     .
     .
             public UserAsyncController(ILogger&lt;UserAsyncController&gt; logger, UserManager userManager)
              {
                    Log = new DefaultLog(logger);
                    this.UserLines = userManager;
              }
   
    .
    .
    .
   }
</code></pre>
<p>这里需要注意我们把所有提供的服务写在了IServer中</p>
<pre><code>[Build]
public interface IServer
{

    [TAG(5002)]
    Task&lt;(bool, string)&gt; LogOn(string username, string password);

    [TAG(5003)]
    Task&lt;(bool, User)&gt; CheckLogIn();

    [TAG(5004)]
    Task&lt;List&lt;User&gt;&gt; GetUsers();

    [TAG(5005)]
    Task&lt;(bool, string)&gt; Say(long userId, string msg);

    [TAG(5006)]
    Task&lt;List&lt;LeavingMsg&gt;&gt; GetLeavingMessage();

}
</code></pre>
<p>我们的控制器需要实现此接口的功能,实现完成后,客户端只要拿到此IServer接口,就可以直接调用服务器的功能模块了.</p>
<p>关于DEMO中Actor控制器的定义:</p>
<pre><code>[ActorOption(1000,10000)] //这里最大列队为1000,如果10秒没有任务将进入沉睡
public class UserActorController : ActorController, IActorService
{
    public ILog Log { get; }

    public UserDatabaseContext UserDatabase { get; }

    public UserActorController(ILogger&lt;UserActorController&gt; logger,UserDatabaseContext userDatabaseContext)
    {         
        Log = new DefaultLog(logger);
        UserDatabase = userDatabaseContext;
        UserDatabase.Database.EnsureCreated();
    }
    /// &lt;summary&gt;
    /// 沉睡的时候保存数据
    /// &lt;/summary&gt;
    /// &lt;returns&gt;&lt;/returns&gt;
    public async override Task Sleeping()
    {
        if (UserDatabase.ChangeTracker.HasChanges())
        {
            var i = await UserDatabase.SaveChangesAsync();

            if (i &gt; 0)
                Log.Info($"save {i} row data");
        }
    }
</code></pre>
<p>同样我们这里需要继承IActorService,这样的话 我们的RPC,CLIENT或者其他的ACTOR就可以通过IActorService 来调用此控制器中的功能了.</p>
<p>详情请看<a href="https://github.com/luyikk/NetX/tree/master/demo/ChatSystem">DEMO Chat Room</a></p>
<p>其他详细的功能演示,我陆续会更新</p>
<p><strong>关于NetXClient:</strong></p>
<p><img src="https://github.com/luyikk/NetX/blob/master/Images/NetX3.png" alt="NetXClient结构图"></p>
<p>我们可以通过NetXClient调用 Server的RPC服务,ACTOR服务,以及提供服务给服务器调用.<br>
NetXClient提供给服务器调用的服务是线程安全的.</p>
<p>关于设置一个NetXClient例子:</p>
<pre><code>var icontainer = new NetxSClientBuilder()
                .ConfigureLogSet(p =&gt;
                {
                    p.AddDebug().SetMinimumLevel(LogLevel.Trace); //添加DEBUG日记输出
                })
                .ConfigSSL(p =&gt; //设置SSL加密
                {
                    p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        $"{System.Windows.Forms.Application.StartupPath}/client.pfx"
                        , "testPassword");
                    p.IsUse = true;
                })
                .ConfigSessionStore(() =&gt; new Netx.Client.Session.SessionFile()) //如何保存Session需要下次打开 不用登陆
                .ConfigConnection(p =&gt; //配置连接
                {
                    p.Host = "127.0.0.1"; //IP
                    p.Port = 3000;  //端口
                    p.ServiceName = "MessageService"; //服务名称
                    p.VerifyKey = "123123"; //密码
                });

   var Client= icontainer.Build(); //生成一个CLIENT
</code></pre>
<p>还记得上面的IServer吗?<br>
我们通过IServer调用服务器:</p>
<pre><code>var service = Client.Get&lt;IServer&gt;();
var (success, msg) = await service.LogOn("username", "password");
</code></pre>
<p>对就是那么简单.通用的方式一样用于调用Actor服务<br>
如果提供可以服务给服务器调用呢?</p>
<pre><code>  public partial class WinMain : Form, IMethodController, IClient
  {
    public INetxSClient Current { get=&gt;Dependency.Client; set { } }
</code></pre>
<p>例如我们这个WINFROM 窗口类需要提供给服务器直接调用.我们首先要继承IMethodController接口,实现 <code>public INetxSClient Current { get=&gt;Dependency.Client; set { } }</code><br>
因为每次服务器调用此类服务的时候会设置Current,所以{get;set;}即可,但是<a href="https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/WinMain.cs">ChaT Room Client Demo</a>里面的这个Current有特殊的需要,所以我们就这样写吧.<br>
<a href="https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/Interfaces/IClient.cs">IClient</a> 定义了我们提供哪些方法供服务器直接调用.<br>
例如我们搞个方法,给服务器设置用户的状态,用于显示:</p>
<pre><code> public void SetUserStats(long userid, byte status)
    {
        this.BeginInvoke(new EventHandler((a, b) =&gt;
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
</code></pre>
<p>详情我们可以查看<br>
<a href="https://github.com/luyikk/NetX/blob/master/demo/ChatSystem/ChatClient/">ClientDemo</a></p>
<p><img src="https://github.com/luyikk/NetX/blob/master/Images/NetX5.png" alt="关于服务的调用方式图解"></p>
<p><strong>NetX代理网关和NetX共享ActorService 我还没有时间开发,但是通过Server和CLIENT我们已经可以实现很多功能了!</strong><br>
下面是 网关图解:<br>
<img src="https://github.com/luyikk/NetX/blob/master/Images/NetX4.png" alt="NetXProxy"></p>
<p><strong>NETXActor:</strong><br>
我提供了一个 ActorRun 本机版,你可以单独拿出来使用就像 <a href="http://AKKA.NET">AKKA.NET</a>. 他的性能是AKKA.NET本机运行的3倍以上,达到了3M TPS, 也就是1秒钟可以处理300W个事务.</p>
<p>关于使用方式:<br>
我看可以参考集成测试:<br>
<a href="https://github.com/luyikk/NetX/tree/master/src/Test/ActorTest">Actor集成测试</a></p>
<p>最后我朋友看我最近在做ACTOR框架,所以他也尝试了写了一个. <a href="http://xn--AKKA-ps5fz6p1z2a05fmmj9ds631akr1a9p4b.NET">据说本机性能高出了AKKA.NET</a> 一截. 所以我把他的DEMO拿来了,添加了我的NETXActor<br>
Src:<a href="https://github.com/luyikk/NetX/tree/master/src/Test/AkkaNetTpsTest">AKKA.NET VS event next VS NetX</a></p>
<p>下面是结果图:<br>
<img src="https://github.com/luyikk/NetX/blob/master/Images/Test.png" alt="AKKA.NET VS event next VS NetX:"></p>

