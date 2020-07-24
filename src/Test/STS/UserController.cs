using Client;
using Microsoft.Extensions.Logging;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace STS
{
    public class UserController : AsyncController,  IServer
    {
        private readonly ILogger<UserController> logger;

        public UserController(ILogger<UserController> logger)
        {
            this.logger = logger;
        }

        public async Task<bool> LogOn(string username, string password)
        {
            if (username == "test" && password == "123123")
            {
                //var res = await Get<IClient>().Add(1, 2);

                var res = await Current.Get<IClient>().Add(1, 2);

                logger.LogInformation("{0}", res);
                return true;
            }
            else
                return false;
        }
    }
}
