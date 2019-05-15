using System;
using System.Collections.Generic;

namespace ChatServer.Models
{




    public partial class Users
    {
        public Users()
        {
            MessageFromUser = new HashSet<Message>();
            MessageTargetUser = new HashSet<Message>();
        }

        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string PassWord { get; set; }
        public byte OnLineStatus { get; set; }

        public virtual ICollection<Message> MessageFromUser { get; set; }
        public virtual ICollection<Message> MessageTargetUser { get; set; }
    }
}
