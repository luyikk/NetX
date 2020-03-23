using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxServer
{
    enum ClientCmdTag
    {
        AddOne=2000,
        Add=2001,
        RecursiveTest=2002,
       
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
        Task<int> Add(int a,int b);

        [TAG(ClientCmdTag.RecursiveTest)]
        Task<int> Recursive(int a);

    
      
    }
}
