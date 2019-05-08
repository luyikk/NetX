using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxClient
{
  
    public enum CmdTagDef
    {
        AddActor=2000,
        Add=1000,
        RotueToActorAdd=1001,
        Msg=3000
    }

    /// <summary>
    /// 我们再客户端上面 定义一个 这样的接口,内容随便,关键是 TAG 和参数,你懂的
    /// </summary>
    [Build]
    interface IServer
    {
        [TAG(CmdTagDef.AddActor)]
        Task<int> AddActor(int a, int b);

        [TAG(CmdTagDef.Add)]
        Task<int> Add(int a, int b);

        [TAG(CmdTagDef.RotueToActorAdd)]
        Task<int> RotueToAddActor(int a, int b);

        [TAG(CmdTagDef.Msg)]
        void RunMsg(string msg);
    }
}
