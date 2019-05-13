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




        public bool IsLogOn { get; private set; }
        public UserInfo CurrentUser { get; private set; }


        /// <summary>
        /// 设置当前用户状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task<bool> SetUserStatus(int status)
        {
            if (await Actor<IActorService>().SetStatus(CurrentUser.UserContent.UserName, status))
            {
                CurrentUser.Status = status;
                return true;
            }
            else
                return false;
        }



        [TAG(ServiceTag.LogOn)]
        public async Task<(bool,string)> LogOn(string username,string password)
        {
            var (success, user, msg) = await Actor<IActorService>().GetUserNameAndPassword(username, password);

            if (success)
            {
                CurrentUser =new UserInfo(user);
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
                CurrentUser.Token = Current;
                await SetUserStatus(1);


            }
            return IsLogOn;
        }

        [TAG(ServiceTag.GetUserList)]
        public async Task<List<Users>> GetUsers()
        {
            if (IsLogOn)
            {
                return await Actor<IActorService>().GetUsers(CurrentUser.UserContent.UserName);
              
            }
            else
                throw new Exception("not login");
        }


        /// <summary>
        /// 断线
        /// </summary>
        public async override void Disconnect()
        {
            await SetUserStatus(2);
        }

        /// <summary>
        /// 退出
        /// </summary>
        public async override void Closed()
        {
            await SetUserStatus(3);
            CurrentUser.Token = null;
        }

    }
}
