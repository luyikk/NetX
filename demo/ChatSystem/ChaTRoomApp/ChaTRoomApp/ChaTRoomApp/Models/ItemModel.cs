using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace ChaTRoomApp.Models
{
    public interface IlistItem:INotifyPropertyChanged
    {
        string ImageId { get;  }

        string Name { get;  }

        int HaveNotLook { get;}     

        string OnlineStatus { get;  }
    }

  
}
