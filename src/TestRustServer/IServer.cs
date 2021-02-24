using Netx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRustServer
{
    public class LogOn
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }


    public class LogOnResult
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        
    }


    /// <summary>
    /// 我们再客户端上面 定义一个 这样的接口,内容随便,关键是 TAG 和参数,你懂的
    /// </summary>
    [Build]
    public interface IServer
    {
        [TAG(1000)]
        Task<int> Add(int a, int b);
        [TAG(800)]
        Task Print(int a);
        [TAG(600)]
        Task Print2(int a, string b);
        [TAG(700)]
        Task RunTest(string a);
        [TAG(5001)]
        void Test(string msg, int i);
        [TAG(1003)]
        Task<int> ToClientAddOne(int a);
        [TAG(1005)]
        Task<int> RecursiveTest(int a);
        [TAG(10000)]
        Task<(bool, string)> LogOn(LogOn data);

        [TAG(10001)]
        Task<LogOnResult> LogOn2((string,string) info);
    }
}
