using ChatServer.Model;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYSQL;
using ChatTag;

namespace ChatServer.ActorControllers
{
    /// <summary>
    /// 用户数据库ACTOR
    /// </summary>
    public class UserActorController:ActorController
    {
        public ILog Log { get; }

        public UserActorController(ILogger<UserActorController> logger)
        {
            ZYSQL.SqlInstance.Instance.InstallConfig(new DataConnectConfig[]
            {
                new DataConnectConfig()
                {
                    Name="DefautConnectionString",
                    ConnectionString="Data Source=|DataDirectory|./UserDatabase.db3;Pooling=true;FailIfMissing=false",
                    SqlType="SQLite",
                    MaxCount=100
                }
             });

            Log = new DefaultLog(logger);
        }

        [TAG(ActorTag.CheckUserName)]
        public async Task<bool> CheckUserName(string username)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                var r= await obj.SqlExecuteScalarAsync("SELECT 1 FROM Users WHERE UserName=@UserName", new System.Data.SQLite.SQLiteParameter("@UserName", username));

                if (r is null)
                    return true;
                else
                    return false;
            }
        }


        [TAG(ActorTag.Register)]
        public async Task<(bool, string)> Register(Users user)
        {
            try
            {
                var ishave = await CheckUserName(user.UserName);

                if (!ishave)
                    return (false, "username is invalid");

                using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
                {
                    var p = await obj.SqlExcuteUpdateOrInsertOrDeleteObjectAsync<Users>("INSERT INTO Users(UserName,NickName,PassWord,OnLineStatus)VALUES(@UserName,@NickName,@PassWord,@OnLineStatus)", user);

                    if (p > 0)
                        return (true, null);
                    else
                        return (false, "fail");
                }
            }
            catch (Exception er)
            {
                return (false, er.ToString());
            }
        }


        [Open(OpenAccess.Internal)]
        [TAG(ActorTag.SaveMsg)]
        public async Task<(bool, string)> SaveMessage(long fromid, long targetid, byte msgtype, string msg, bool issend)
        {
            var message = new Message()
            {
                FromUserId = fromid,
                Time=TimeHelper.GetTime(),
                TargetUserId = targetid,
                MessageContext = msg,
                MsgType = msgtype,
                IsSend = issend
            };

            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
               var r= await obj.SqlExcuteUpdateOrInsertOrDeleteObjectAsync<Message>("INSERT INTO Message(Time,MsgType,FromUserId,TargetUserId,MessageContext,IsSend)VALUES(@Time,@MsgType,@FromUserId,@TargetUserId,@MessageContext,@IsSend)", message);

                if (r == 1)                
                    return (true, "success");                
                else
                    return (false, "insert fail");
            }

        }
    


        [Open(OpenAccess.Internal)] //OpenAccess.Internal 表示无法被客户端直接访问
        [TAG(ActorTag.CheckUserNameAndPassword)]
        public async Task<(bool,Users,string)> LogOnUser(string username,string password)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                var r = await obj.SqlExcuteSelectFirstAsync<Users>("SELECT UserId,UserName,NickName,OnLineStatus FROM Users WHERE UserName=@UserName AND PassWord=@PassWord"
                    , new System.Data.SQLite.SQLiteParameter("@UserName", username)
                    ,new System.Data.SQLite.SQLiteParameter("@PassWord",password)
                    );

                if (r is null)
                    return (false, null,"username or password error");               
                else
                    return (true, r,"login successfully");
            }
        }

        [Open(OpenAccess.Internal)]
        [TAG(ActorTag.GetUsers)]
        public async Task<List<Users>> GetUsers(string exclude_username)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                return await obj.SqlExcuteSelectObjectAsync<Users>("SELECT UserId,UserName,NickName,OnLineStatus FROM Users WHERE UserName!=@UserName", new System.Data.SQLite.SQLiteParameter("@UserName", exclude_username));
            }
        }

        [Open(OpenAccess.Internal)]
        [TAG(ActorTag.SetStatus)]
        public async Task<bool> SetStatus(string username,byte status)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                var i = await obj.SqlExecuteNonQueryAsync("UPDATE Users SET OnLineStatus=@OnLineStatus WHERE UserName=@UserName"
                    , new System.Data.SQLite.SQLiteParameter("@UserName", username)
                    , new System.Data.SQLite.SQLiteParameter("@OnLineStatus", status)
                    );

                if (i == 1)
                    return true;
                else
                    return false;

            }
        }

        [Open(OpenAccess.Internal)]
        [TAG(ActorTag.LeavingMessage)]
        public async Task<List<LeavingMsg>> GetLeavingMessage(long userId)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                var lmsgs= await obj.SqlExcuteSelectObjectAsync<LeavingMsg>("SELECT a.Time,a.MsgType,a.FromUserId,b.NickName,a.MessageContext FROM Message AS a LEFT JOIN Users AS b ON a.FromUserId=b.UserId WHERE a.IsSend=0 AND a.TargetUserId=@TargetUserId"
                    , new System.Data.SQLite.SQLiteParameter("@TargetUserId", userId));



                obj.SqlExecuteNonQuery("UPDATE Message SET IsSend=1 WHERE TargetUserId=@TargetUserId", new System.Data.SQLite.SQLiteParameter("@TargetUserId", userId));

                return lmsgs;
            }
        }

    }
}
