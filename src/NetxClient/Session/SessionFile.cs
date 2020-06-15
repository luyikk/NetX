using Netx.Interface;
using System.IO;

namespace Netx.Client.Session
{
    /// <summary>
    /// Session 保存文件
    /// </summary>
    public class SessionFile : ISessionStore
    {
        public string SessionFilePath { get; set; }

        public SessionFile(string? sessionpath = null)
        {
            if (sessionpath == null) //默认保存到当前用户的temp文件夹
                sessionpath = Path.GetTempPath();

            SessionFilePath = Path.Combine(sessionpath, "session_data");
        }

        public long GetSessionId()
        {
            if (File.Exists(SessionFilePath))
            {
                using BinaryReader read = new BinaryReader(new MemoryStream(File.ReadAllBytes(SessionFilePath)));
                return read.ReadInt64();
            }
            else
                return 0;
        }

        public void SaveSessionId(long sessionid)
        {
            using BinaryWriter wr = new BinaryWriter(new FileStream(SessionFilePath, FileMode.Create));
            wr.Write(sessionid);
        }
    }
}
