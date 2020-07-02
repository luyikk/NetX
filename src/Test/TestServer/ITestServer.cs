using Netx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetxTestServer
{
    [Build]
    public interface ITestServer
    {
        [TAG(1000)]
        Task<HelloReply> SayHello(HelloRequest msg);

        [TAG(1001)]
        Task<User> Register(string name, string email, string password, string title, string city);

        [TAG(1002)]
        Task<List<User>> List(int count);
    }


    public class User
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Title { get; set; }

        public string City { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class HelloRequest
    {
        public string Name { get; set; }
    }

    public class HelloReply
    {
        public string Message { get; set; }
    }
}
