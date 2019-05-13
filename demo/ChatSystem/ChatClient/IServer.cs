using Netx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatTag;
namespace ChatClient
{
   

    [Build]
    public interface IServer
    {
        [TAG(ActorTag.Register)]
        Task<(bool, string)> Register(Users user);

        [TAG(ServiceTag.LogOn)]
        Task<(bool, string)> LogOn(string username, string password);

        [TAG(ServiceTag.CheckLogIn)]
        Task<bool> CheckLogIn();

        [TAG(ServiceTag.GetUserList)]
        Task<List<Users>> GetUsers();

        [TAG(ServiceTag.Say)]
        Task<(bool, string)> Say(long userId, string msg);

        [TAG(ServiceTag.GetLeaving)]
        Task<List<LeavingMsg>> GetLeavingMessage();

    }
}
