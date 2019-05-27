using System;
using System.Collections.Generic;
using System.Text;

namespace ChaTRoomApp.Models
{
    public class UserSort : IComparer<IlistItem>
    {
        public int Compare(IlistItem x, IlistItem y)
        {
            if (x.ImageId == "room"&&y.ImageId!="room")
                return -1;
            else if (x is User a && y is User b)
            {
                if (a.OnLineStatus == b.OnLineStatus)
                    return 0;
                else if (a.OnLineStatus == 1 && b.OnLineStatus != 1)
                    return -1;
                else if (b.OnLineStatus == 1 && a.OnLineStatus != 1)
                    return 1;
                else if (b.OnLineStatus == 2 && a.OnLineStatus == 0)
                    return 1;
                else if (a.OnLineStatus == 2 && b.OnLineStatus == 0)
                    return -1;
                else
                    return 0;
            }
            else
                return 0;
        }
    }
}
