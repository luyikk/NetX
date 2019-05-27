using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChaTRoomApp.Models
{
    public class Room : IlistItem
    {

        public string ImageId => "room";

        public string Name => "Public Room";

        public int HaveNotLook => 0;

        public string OnlineStatus => "";

        public event PropertyChangedEventHandler PropertyChanged;

        public void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
