using Interfaces;
using Netx;
using Netx.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/client.pfx", "testPassword");

        static async Task Main(string[] args)
        {
            var icontainer = new NetxSClientBuilder()              
               .ConfigSSL(p => //设置SSL加密
                {
                   p.Certificate = certificate;
                   p.IsUse = true;
               })
                .ConfigCompress(p => p.Mode = Netx.CompressType.None)
               // .ConfigSessionStore(() => new Netx.Client.Session.SessionFile()) //如何保存Session需要下次打开 不用登陆
               .ConfigConnection(p => //配置连接
                {
                   p.Host = "127.0.0.1"; //IP
                    p.Port = 3000;  //端口
                    p.ServiceName = "MessageService"; //服务名称
                    p.VerifyKey = "123123"; //密码
                });

            var Current = icontainer.Build(); //生成一个CLIENT

        Re:
            var (success, my) = await Current.Get<IServer>().CheckLogIn();
            var rand = new Random();
         

            if (!success)
            {
                my = new User
                {
                    UserName = $"user_{rand.Next(1, 1000000)}",
                    PassWord = "123123",
                    NickName = $"{rand.Next(1, 1000000)}"
                };

                var (check_register, msg)= await Current.Get<IServer>().Register(my);

                if (!check_register)
                {
                    Console.WriteLine(msg);
                    return;
                }

                (success, msg) = await Current.Get<IServer>().LogOn(my.UserName, my.PassWord);

                if (!success)
                {
                    Console.WriteLine(msg);
                    return;
                }
                goto Re;
            }



            for (int i = 0; i < 1000; i++)
            {
                await Current.Get<IServer>().Say(-1, "11111111");
            }

        }
    }
}
