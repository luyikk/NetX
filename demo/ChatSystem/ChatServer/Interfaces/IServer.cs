using ChatServer.Model;
using Netx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Interfaces
{
   

    [Build]
    public interface IServer
    {
      
        [TAG(5002)]
        Task<(bool, string)> LogOn(string username, string password);

        [TAG(5003)]
        Task<bool> CheckLogIn();

        [TAG(5004)]
        Task<List<Users>> GetUsers();

        [TAG(5005)]
        Task<(bool, string)> Say(long userId, string msg);

        [TAG(5006)]
        Task<List<LeavingMsg>> GetLeavingMessage();

    }
}
