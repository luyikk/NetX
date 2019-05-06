using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Interface
{
    public interface ISessionStore
    {
        long GetSessionId();

        void SaveSessionId(long sessionid); 
    }
}

