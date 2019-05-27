using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChaTRoomApp.Models
{
    public class User: IlistItem
    {
    
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string PassWord { get; set; }
        public byte OnLineStatus { get; set; }

        public string OnlineStatus { get => OnLineStatus == 0 ? "Offline" : OnLineStatus == 2 ? "Leave" : "Online"; }
        public string ImageId { get => "role"; }
        public string Name { get => NickName; }
        public int HaveNotLook { get => 0;  }

        public event PropertyChangedEventHandler PropertyChanged;

        public void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



        public override string ToString()
        {
            return NickName; 
        }
    }
}
