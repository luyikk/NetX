using ChatServer.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.ActorControllers
{
    /// <summary>
    /// 用户数据库ACTOR
    /// </summary>
    [ActorOption(1000,1000)]
    public class UserActorController : ActorController, IActorService
    {
        public ILog Log { get; }

        public UserDatabaseContext UserDatabase { get; }

        public UserActorController(ILogger<UserActorController> logger,UserDatabaseContext userDatabaseContext)
        {         
            Log = new DefaultLog(logger);
            UserDatabase = userDatabaseContext;
            UserDatabase.Database.EnsureCreated();
        }


        public Task<bool> CheckUserName(string username)
        {
           
            if (UserDatabase.Users.SingleOrDefault(p => p.UserName == username) ==null)
                return Task.FromResult(true);
            else
                return Task.FromResult(false);
        }



        public async Task<(bool, string)> Register(Users user)
        {            
            var ishave = await CheckUserName(user.UserName);

            if (!ishave)
                return (false, "username is invalid");

            await UserDatabase.Users.AddAsync(user);

            if(await UserDatabase.SaveChangesAsync()>0)
                return (true, "success");
            else
                return (false, "fail");
        }


        [Open(OpenAccess.Internal)]
        public async Task<(bool, string)> SaveMessage(long fromid, long targetid, byte msgtype, string msg, bool issend)
        {
            var message = new Message()
            {
                FromUserId = fromid,
                Time = TimeHelper.GetTime(),
                TargetUserId = targetid,
                MessageContext = msg,
                MsgType = msgtype,
                IsSend = issend
            };

            await UserDatabase.Message.AddAsync(message);         
            return (true, "success");        
        
        }



        [Open(OpenAccess.Internal)] //OpenAccess.Internal 表示无法被客户端直接访问     
        public async Task<(bool, User, string)> GetUserNameAndPassword(string username, string password)
        {

            var user = await UserDatabase.Users.Where(p => p.UserName == username && p.PassWord == password).FirstOrDefaultAsync();

            if (user is null)
                return (false, null, "username or password error");
            else
            {
                var datauser = new User()
                {
                    NickName = user.NickName,
                    OnLineStatus = user.OnLineStatus,                  
                    UserId = user.UserId,
                    UserName = user.UserName
                };

                return (true, datauser, "login successfully");
            }

        }

        [Open(OpenAccess.Internal)]
        public Task<List<User>> GetUsers(string exclude_username)
        {           
            var userlist = from user in UserDatabase.Users
                           where user.UserName != exclude_username
                           select new User (){ UserId=  user.UserId, UserName=user.UserName, NickName= user.NickName, OnLineStatus= user.OnLineStatus };

            return Task.FromResult(userlist.ToList());
        }

        [Open(OpenAccess.Internal)]
        public async Task<bool> SetStatus(string username, byte status)
        {

            var user = await UserDatabase.Users.SingleOrDefaultAsync(p => p.UserName == username);
            if (user is null)
                return false;
            else
            {
                user.OnLineStatus = status;

                if (await UserDatabase.SaveChangesAsync() > 0)
                    return true;
                else
                    return false;
               
            }
        }

        [Open(OpenAccess.Internal)]
        public async Task<List<LeavingMsg>> GetLeavingMessage(long userId)
        {

            var leavingMsgList = UserDatabase.Message.Where(p => p.TargetUserId == userId && p.IsSend == false);                                

            List<LeavingMsg> list = new List<LeavingMsg>();

            foreach (var msg in leavingMsgList)
            {
                await UserDatabase.Entry(msg)
                .Reference(b => b.FromUser)
                .LoadAsync();

                msg.IsSend = true;
                list.Add(new LeavingMsg()
                {
                    FromUserId = msg.FromUserId,
                    MessageContext = msg.MessageContext,
                    MsgType = msg.MsgType,
                    NickName = msg.FromUser.NickName,
                    Time = msg.Time
                });
            }           
         

            return list;
        }

        public async override Task Sleeping()
        {
            if (UserDatabase.ChangeTracker.HasChanges())
            {
                var i = await UserDatabase.SaveChangesAsync();

                if (i > 0)
                    Log.Info($"save {i} row data");
            }
        }

    }
}
