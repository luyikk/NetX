using ChatClient;
using Netx;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Interfaces
{


    [Build]
    public interface IServer
    {

        [TAG(10000)] //注意这里是Actor中提供的服务 因为Actor就提供了一个public的服务供客户端调用我就不另外写个接口文件了
        Task<(bool, string)> Register(User user);

        [TAG(5002)]
        Task<(bool, string)> LogOn(string username, string password);

        [TAG(5003)]
        Task<(bool, User)> CheckLogIn();

        [TAG(5004)]
        Task<List<User>> GetUsers();

        [TAG(5005)]
        Task<(bool, string)> Say(long userId, string msg);

        [TAG(5006)]
        Task<List<LeavingMsg>> GetLeavingMessage();

       

    }
}
