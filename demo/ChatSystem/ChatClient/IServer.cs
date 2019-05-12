using Netx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public enum ServerTag
    {
        Register=1,
        LogOn = 1001,
        CheckLogIn = 1002
    }


    [Build]
    public interface IServer
    {
        [TAG(ServerTag.Register)]
        Task<(bool, string)> Register(Users user);

        [TAG(ServerTag.LogOn)]
        Task<(bool, string)> LogOn(string username, string password);

        [TAG(ServerTag.CheckLogIn)]
        Task<bool> CheckLogIn();
    }
}
