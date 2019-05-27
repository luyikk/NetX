using System;
using System.Collections.Generic;
using System.Text;
using SQLitePCL;
using SQLite;

namespace ChaTRoomApp.Models
{
   
    public class SaveUser
    {
        [PrimaryKey]
        public string UserName { get; set; }

        public string PassWord { get; set; }
    }
}
