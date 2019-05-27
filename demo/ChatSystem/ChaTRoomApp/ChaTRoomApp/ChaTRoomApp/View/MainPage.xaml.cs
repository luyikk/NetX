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
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
         
            this.BindingContext = new MainModel(this);
        }

        protected override void OnAppearing()
        {
            (BindingContext as MainModel)?.LoadingUser();
            base.OnAppearing();
        }
    }
}