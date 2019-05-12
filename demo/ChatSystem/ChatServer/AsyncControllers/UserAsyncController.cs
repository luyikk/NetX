using ChatServer.Mode;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Loggine;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.AsyncControllers
{
    public class UserAsyncController : AsyncController
    {
        public ILog Log { get;  }


        public UserAsyncController(ILogger<UserAsyncController> logger)
        {
            Log = new DefaultLog(logger);
          
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
        public Task<bool> CheckLogIn()
        {
            return Task.FromResult(IsLogOn);
        }
    }
}
