using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxServer
{
    public enum ActorCmdTag
    {
        Add=2000
    }

    [Build]
    public interface IActorService
    {
        [TAG(ActorCmdTag.Add)]
        Task<int> Add(int a, int b);
    }
}
