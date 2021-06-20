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

    public class Foo
    {
        public int v1 { get; set; }
        public string v2 { get; set; }
        public List<int> v3 { get; set; }
        public (float,double,string) v4 { get; set; }

        public Foo()
        {
            v1 = 1;
            v2 = "2";
            v3 = new List<int> { 3, 4 };
            v4 = (5.0f, 6.111, "6");
        }

        public override bool Equals(object obj)
        {
            if (obj is Foo t)
            {
                return t.v1 == this.v1 && t.v2 == this.v2 && t.v3.Count == this.v3.Count && t.v4 == this.v4;
            }
            else
                return false;

        }

        

    }


    /// <summary>
    /// 我们再客户端上面 定义一个 这样的接口,内容随便,关键是 TAG 和参数,你懂的
    /// </summary>
    [Build]
    public interface IServer
    {

        [TAG(2)]
        Task<(string, string, string)> test_string((string, string, string) v);

        [TAG(3)]
        Task<(List<byte>, List<byte>, List<byte>)> test_buff((List<byte>, List<byte>, List<byte>) v);

        [TAG(4)]
        Task<Foo> test_struct(Foo f);

        [TAG(5)]
        Task<(long, ulong, float, double)> test_base_type((long, ulong, float, double) v);


        [TAG(6)]
        Task<(bool, sbyte, byte, short, ushort, int, uint)> test_base_type((bool, sbyte, byte, short, ushort, int, uint) v);

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
