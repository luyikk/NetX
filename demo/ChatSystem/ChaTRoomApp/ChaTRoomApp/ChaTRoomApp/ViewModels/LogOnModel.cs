using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System;
using Interfaces;
using Xamarin.Forms.Internals;
using ChaTRoomApp.View;
using ChaTRoomApp.DataBase;

namespace ChaTRoomApp.ViewModels
{

    [Preserve(true, false)]
    public class LogOnModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Command<object> LogOnCommand { get; }
        public Command<Page> RegisterCommand { get; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string ErrorMsg { get; set; }

        private bool islogon;

        public bool IsLogOn { get { return islogon; } set { islogon = value; PropertyChange(); } }

        public Page currentPage { get; }

        private bool isSave;
        public bool IsSave
        {
            get { return isSave; }
            set
            {
                isSave = value;
                PropertyChange();

                if(!isSave)
                    if (File.Exists(pwdFile))
                        File.Delete(pwdFile);
            }
        }

        readonly string pwdFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "usernameAndPassword.txt");


        public LogOnModel(Page page)
        {
            LogOnCommand = new Command<object>(LogOn);
            RegisterCommand = new Command<Page>(Register);            

            currentPage = page;

            CheckLogOn();
        }


        private async void CheckLogOn()
        {
            try
            {
                var user = await DependencyService.Resolve<ChaTDataBaseManager>().GetSaveUser();

                if(user!=null)
                {
                    UserName = user.UserName;
                    PassWord = user.PassWord;
                    PropertyChange(nameof(UserName));
                    PropertyChange(nameof(PassWord));
                    IsSave = true;
                }
            }
            catch
            {

            }
        }

        private async void SavePassWord()
        {
            await DependencyService.Get<ChaTDataBaseManager>().ClecrUser();
            await DependencyService.Get<ChaTDataBaseManager>().SaveUser(new Models.SaveUser()
            {
                UserName = UserName,
                PassWord = PassWord
            });
        }


        public async void LogOn(object page)
        {
            Page tpage = page as Page;

            try
            {
                
                IsLogOn = true;

                var client = DependencyService.Get<ClientService>().Client;

                if (!client.IsConnect)
                    await client.OpenAsync();
            
                var (success, msg) = await client.Get<IServer>().LogOn(UserName, PassWord);

                if (success)
                {
                    if (IsSave)
                        SavePassWord();
                    else
                        await DependencyService.Get<ChaTDataBaseManager>().ClecrUser();

                    await currentPage.Navigation.PopModalAsync();
                }
                else
                {
                    ErrorMsg = msg;                   

                    await tpage?.DisplayAlert("Error", msg, "OK");
                }

                IsLogOn = false;
            }
            catch (Netx.NetxException er)
            {
                ErrorMsg = er.Message;
                IsLogOn = false;
                await tpage?.DisplayAlert("Error", ErrorMsg, "OK");
            }
            catch(Exception erx)
            {
                ErrorMsg = erx.Message;
                IsLogOn = false;
                await tpage?.DisplayAlert("Error", ErrorMsg, "OK");
            }
        }

        public async void Register(Page page)
        {

            await page.Navigation.PushModalAsync(new RegisterUser());
        }

    }
}
