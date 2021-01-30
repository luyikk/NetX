using Netx;
using System.Threading.Tasks;

namespace TestNetxServer
{
    enum ClientCmdTag
    {
        AddOne = 2000,
        Add = 2001,
        RecursiveTest = 2002,
        Print = 3000,
        Print2 = 5000,
        Run =4000
    }

    /// <summary>
    /// 供服务器主动调用客户端函数
    /// </summary>
    [Build]
    public interface IClientCalling
    {
        [TAG(ClientCmdTag.AddOne)]
        Task<int> AddOne(int a);

        [TAG(ClientCmdTag.Add)]
        Task<int> Add(int a, int b);

        [TAG(ClientCmdTag.RecursiveTest)]
        Task<int> Recursive(int a);

        [TAG(ClientCmdTag.Print)]
        void Print(int a);

        [TAG(ClientCmdTag.Print2)]
        void Print2(int a,string name);

        [TAG(ClientCmdTag.Run)]
        Task Run(string a);
    }
}
