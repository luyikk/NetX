using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx;
using Netx.Interface;
namespace STS
{
    [Build]
    public interface IServer
    {
        [TAG(1)]
        Task<bool> LogOn(string username, string password);
    }
}
