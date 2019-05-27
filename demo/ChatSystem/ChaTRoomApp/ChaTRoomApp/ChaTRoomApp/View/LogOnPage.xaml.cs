using ChaTRoomApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ChaTRoomApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogOnPage : ContentPage
    {
        public LogOnPage()
        {
            InitializeComponent();

            this.BindingContext = new LogOnModel(this);
          
        }
    }
}