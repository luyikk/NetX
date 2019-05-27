using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace ChaTRoomApp.Models
{
    public class MessageTable
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public long FromId { get; set; }

        public string FromName { get; set; }

        public long TargetId { get; set; }

        public string TargetName { get; set; }

        public bool IsRight { get; set; }


        public string MessageContext { get; set; }

        public byte MsgType { get; set; }

    }
}
