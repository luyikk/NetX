using ChaTRoomApp.Models;
using Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace ChaTRoomApp.ViewModels
{
    public class RegisterModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Command<Page> RegisterCommand { get; }
        public Command<Page> BackCommand { get; }


        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string NickName { get; set; }

    

        public RegisterModel()
        {
            RegisterCommand = new Command<Page>(RegisterStart);
            BackCommand = new Command<Page>(async p =>
              {
                  await p.Navigation.PopModalAsync();

              });
        }




        public async void RegisterStart(Page page)
        {
            try
            {
             

                var client = DependencyService.Get<ClientService>().Client;


                var (success, msg) = await client.Get<IServer>().Register(new User
                {
                    NickName=NickName,
                    UserName=UserName,
                    PassWord=PassWord
                });

                if (!success)
                {
                    await page.DisplayAlert("Error", msg, "OK");
                }
                else
                {
                    await page.DisplayAlert("Info", "Register Success!", "OK");
                }
            }
            catch (Netx.NetxException er)
            {
                await page.DisplayAlert("Error", er.ErrorMsg, "OK");
            }
            catch (Exception erx)
            {
                await page.DisplayAlert("Error", erx.Message, "OK");
            }

        }

      
    }
}
