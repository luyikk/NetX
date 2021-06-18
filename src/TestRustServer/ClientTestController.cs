using Netx;
using Netx.Loggine;
using System;
using System.Threading.Tasks;

namespace TestRustServer
{

    public class ClientTestController : MethodControllerBase  //or IMethodController
    {
        public string Name { get; set; }

        [TAG(2000)]
        public Task<int> AddOne(int i)
        {
            return Task.FromResult(i + 1);
        }

        [TAG(3000)]
        public void Print(int i)
        {
            Console.WriteLine(i);
        } 

        [TAG(4000)]
        public Task<string> Run(string name)
        {
            Console.WriteLine("name:{0}",name);
            Name = name;
            return Task.FromResult(name);
        }

        [TAG(5000)]
        public Task Print2(int i, string s)
        {
            Console.WriteLine($"{i}-{s}-{Name}");
            return Task.CompletedTask;
        }

        [TAG(2002)]
        public async Task<int> RecursiveTest(int a)
        {
            a -= 1;
            if (a > 0)
                return await Current.Get<IServer>().RecursiveTest(a);
            else
                return a;
        }
    }
}
