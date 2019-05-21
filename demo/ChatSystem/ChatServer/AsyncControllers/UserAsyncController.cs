using ChatServer.Model;
using ChatServer.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Loggine;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatServer.AsyncControllers
{
    public class UserAsyncController : AsyncController, IServer
    {
        public ILog Log { get; }

        public UserManager UserLines { get; }

        public bool IsLogOn { get; private set; }

        public UserInfo CurrentUser { get; private set; }

        public UserAsyncController(ILogger<UserAsyncController> logger, UserManager userManager)
        {
            Log = new DefaultLog(logger);
            this.UserLines = userManager;
        }

        /// <summary>
        /// 设置当前用户状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task<bool> SetUserStatus(byte status)
        {
          
            if (await Actor<IActorService>().SetStatus(CurrentUser.UserContent.UserName, status))
            {
                CurrentUser.Status = status;

                foreach (var user in UserLines.UserList.Values)
                {
                    if (user.Token != null && user.Token.IsConnect)
                        user.Token.Get<IClient>().SetUserStats(CurrentUser.UserContent.UserId, status);
                }

                return true;
            }
            else
                return false;
        }
               

        public async Task<(bool, string)> LogOn(string username, string password)
        {
            var (success, user, msg) = await Actor<IActorService>().GetUserNameAndPassword(username, password);

            if (success)
            {
                user.PassWord = null;
                CurrentUser = new UserInfo(user);
                IsLogOn = true;

                foreach (var otheruser in UserLines.UserList.Values)
                {
                    if (otheruser.Token.IsConnect)
                        otheruser.Token.Get<IClient>().UserAdd(user);
                }

                return (success, msg);
            }
            else
            {
                IsLogOn = false;
                return (success, msg);
            }
        }
        

        public async Task<(bool,User)> CheckLogIn()
        {
            if (IsLogOn)
            {
                CurrentUser.Token = Current;

                UserLines.UserList.AddOrUpdate(CurrentUser.UserContent.UserId, CurrentUser, (_, __) => CurrentUser);

                await SetUserStatus(1);

                return (true, CurrentUser.UserContent);
            }
            return (IsLogOn,null);
        }


        public async Task<List<User>> GetUsers()
        {
            if (IsLogOn)
            {
                return await Actor<IActorService>().GetUsers(CurrentUser.UserContent.UserName);

            }
            else
            {
                Get<IClient>().NeedLogOn();
                return null;
            }
        }

        /// <summary>
        /// 说话
        /// </summary>
        /// <param name="userid">-1=对所有人说</param>
        /// <param name="msg">消息</param>
        /// <returns></returns>       
        public async Task<(bool, string)> Say(long userid, string msg)
        {
            if (!IsLogOn)
            {
                Get<IClient>().NeedLogOn();

                return (false, "need logon");
            }
            if (userid == -1)
            {
                var (s, _) = await Actor<IActorService>().SaveMessage(CurrentUser.UserContent.UserId, CurrentUser.UserContent.UserId, 0, msg, true); //公聊

                if (s)
                {
                    foreach (var user in UserLines.UserList.Values)
                        user.Token.Get<IClient>().SayMessage(CurrentUser.UserContent.UserId, CurrentUser.UserContent.NickName, 0, msg, TimeHelper.GetTime());

                    return (true, "success");
                }
                else
                    return (false, "Say fail");
            }
            else
            {
                if (UserLines.UserList.ContainsKey(userid))
                {
                    var (s, _) = await Actor<IActorService>().SaveMessage(CurrentUser.UserContent.UserId, userid, 1, msg, true); //私聊
                    if (s)
                    {
                        UserLines.UserList[userid].Token.Get<IClient>().SayMessage(CurrentUser.UserContent.UserId, CurrentUser.UserContent.NickName, 1, msg, TimeHelper.GetTime());
                        return (true, "success");
                    }
                    else
                        return (false, "Say fail");
                }
                else
                {
                    var (s, _) = await Actor<IActorService>().SaveMessage(CurrentUser.UserContent.UserId, userid, 2, msg, false); //留言

                    if (s)
                        return (true, "success");
                    else
                        return (false, "Say fail");
                }
            }

        }


        public async Task<List<LeavingMsg>> GetLeavingMessage()
        {
            if (!IsLogOn)
            {
                Get<IClient>().NeedLogOn();
                return new List<LeavingMsg>();
            }


            var msglist = (await Actor<IActorService>().GetLeavingMessage(CurrentUser.UserContent.UserId));
            msglist.Sort((a, b) => a.Time.CompareTo(b.Time));
            return msglist;
        }

        /// <summary>
        /// 断线
        /// </summary>
        public async override void Disconnect() //特别要注意 如果 是async void 那么异常将无法捕捉,需要手工添加try
        {

            try
            {
                if (CurrentUser != null)
                {
                    UserLines.UserList.TryRemove(CurrentUser.UserContent.UserId, out _);

                    await SetUserStatus(2);
                }
            }
            catch (Exception er)
            {
                Log.Error(er);
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public async override void Closed() //特别要注意 如果 是async void 那么异常将无法捕捉,需要手工添加try
        {
            try
            {

                if (CurrentUser != null)
                {
                    await SetUserStatus(0);
                }
            }
            catch (Exception er)
            {
                Log.Error(er);
            }
        }

    }
}
