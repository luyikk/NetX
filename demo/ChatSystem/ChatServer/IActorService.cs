using ChatServer.Model;
using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatTag;

namespace ChatServer
{
    [Build]
    public interface IActorService
    {
        [TAG(ActorTag.Register)]
        Task<(bool, string)> Register(Users user);

        [TAG(ActorTag.CheckUserNameAndPassword)]
        Task<(bool, Users, string)> GetUserNameAndPassword(string username, string password);

        [TAG(ActorTag.GetUsers)]
        Task<List<Users>> GetUsers(string exclude_username);

        [TAG(ActorTag.SetStatus)]
        Task<bool> SetStatus(string username, int status);
    }
}
