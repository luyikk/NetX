using ChaTRoomApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ChaTRoomApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SayShowPage : ContentPage
    {
        public SayShowPage(IMessageAdd messageAdd,byte type,long fromId=-1,string nickname="")
        {
            InitializeComponent();

            this.Title = nickname;

            this.BindingContext = new SayShowModel(MessagesListView,messageAdd, type, fromId, nickname);

            this.Send.Source =ImageSource.FromResource("ChaTRoomApp.Icon.Send.png", typeof(SayShowPage).GetTypeInfo().Assembly);
                      
            this.MessagesListView.SelectionMode = ListViewSelectionMode.None;
            this.MessagesListView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
          
        }

        //private async void MessagesListView_Focused(object sender, FocusEventArgs e)
        //{
        //    await Task.Delay(100);
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        MessagesListView.ScrollTo(null, ScrollToPosition.End, false);
        //    });
        //}

       

      
    }
}