using ChatServer.Model;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Loggine;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatTag;

namespace ChatServer.AsyncControllers
{
    public class UserAsyncController : AsyncController
    {
        public ILog Log { get;  }


        public UserAsyncController(ILogger<UserAsyncController> logger)
        {
            Log = new DefaultLog(logger);
          
        }

        public async override void Disconnect()
        {
            await Actor<IActorService>().SetStatus(CurrentUser.UserName, 2);
        }

        public async override void Closed()
        {
            await Actor<IActorService>().SetStatus(CurrentUser.UserName, 0);
        }

        public bool IsLogOn { get; private set; }
        public Users CurrentUser { get; private set; }


        [TAG(ServiceTag.LogOn)]
        public async Task<(bool,string)> LogOn(string username,string password)
        {
            var (success, user, msg) = await Actor<IActorService>().GetUserNameAndPassword(username, password);

            if (success)
            {
                CurrentUser = user;
                IsLogOn = true;
                return (success, msg);
            }
            else
            {
                IsLogOn = false;
                return (success, msg);
            }
        }

        [TAG(ServiceTag.CheckLogIn)]
        public async Task<bool> CheckLogIn()
        {
            if (IsLogOn)
            {
                await Actor<IActorService>().SetStatus(CurrentUser.UserName, 1);
            }
            return IsLogOn;
        }

        [TAG(ServiceTag.GetUserList)]
        public async Task<List<Users>> GetUsers()
        {
            if (IsLogOn)
            {
                return await Actor<IActorService>().GetUsers(CurrentUser.UserName);
              
            }
            else
                throw new Exception("not login");
        }
    }
}
