namespace Netx.Interface
{
    public interface ISessionStore
    {
        long GetSessionId();

        void SaveSessionId(long sessionid);
    }
}

