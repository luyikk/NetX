using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Netx;
using Netx.Client;

namespace ChaTRoomApp
{
    public class ClientService:IDisposable
    {
        public NetxSClient Client { get; private set; }
        public IServiceProvider service { get; set; }

        public ClientService()
        {

        }

       public void ConfigInstance(string ip,int port)
        {
            Dispose();

            var clientbuilder = new NetxSClientBuilder()

                   .ConfigConnection(option =>
                   {
                       option.Host = ip;
                       option.Port = port;
                       option.ServiceName = "MessageService";
                       option.VerifyKey = "123123";
                       option.ConnectedTimeOut = 10000;
                   })
                   .ConfigureLogSet(p =>
                   {

                   })
                   .ConfigSessionStore(() => new Netx.Client.Session.SessionFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
                   .ConfigSSL(option =>
                   {
                       var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ClientService)).Assembly;
                       using (Stream stream = assembly.GetManifestResourceStream("ChaTRoomApp.client.pfx"))
                       {
                           using (BinaryReader read = new BinaryReader(stream))
                           {
                               var data = read.ReadBytes((int)stream.Length);

                               option.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(data, "testPassword");
                               option.IsUse = true;
                           }
                       }

                   });



            Client = clientbuilder.Build();
            service = clientbuilder.Provider;
        }

        public void Dispose()
        {
            if (Client is IDisposable disposable)
                disposable?.Dispose();

            if (service is IDisposable servicedisposable)
                servicedisposable?.Dispose();
        }
    }
}
