using ChaTRoomApp.DataBase;
using ChaTRoomApp.View;
using Interfaces;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ChaTRoomApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ChaTDataBaseManager>();
            DependencyService.Register<ClientService>();
            var client = DependencyService.Get<ClientService>();
            client.ConfigInstance("32km.com", 3000);

            MainPage = new NavigationPage(new MainPage());
         
     
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
