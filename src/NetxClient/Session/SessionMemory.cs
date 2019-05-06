using Netx.Interface;

namespace Netx.Client.Session
{
    /// <summary>
    /// Session  内存版
    /// </summary>
    public class SessionMemory : ISessionStore
    {
        private long session;

        public long GetSessionId()
        {
            return session;
        }

        public void SaveSessionId(long sessionid)
        {
            session = sessionid;
        }
    }
}
