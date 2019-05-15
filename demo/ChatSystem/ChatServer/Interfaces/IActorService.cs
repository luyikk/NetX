using ChatServer.Model;
using ChatServer.Models;
using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    [Build]
    public interface IActorService
    {
        [TAG(10000)]
        Task<(bool, string)> Register(Users user);

        [TAG(10001)]
        Task<(bool, User, string)> GetUserNameAndPassword(string username, string password);

        [TAG(10002)]
        Task<List<User>> GetUsers(string exclude_username);

        [TAG(10003)]
        Task<bool> SetStatus(string username, byte status);

        [TAG(10004)]
        Task<(bool, string)> SaveMessage(long fromid, long targetid, byte msgtype, string msg, bool issend);

        [TAG(10005)]
        Task<List<LeavingMsg>> GetLeavingMessage(long userId);
    }
}
