using ChatServer.Mode;
using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [Build]
    public interface IActorService
    {
        [TAG(ActorTag.Register)]
        Task<(bool, string)> Register(Users user);

        [TAG(ActorTag.CheckUserNameAndPassword)]
        Task<(bool, Users, string)> GetUserNameAndPassword(string username, string password);

            
    }
}
