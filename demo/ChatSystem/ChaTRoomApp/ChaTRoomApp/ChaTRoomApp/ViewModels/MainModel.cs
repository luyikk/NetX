using ChaTRoomApp.Models;
using Interfaces;
using Netx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using ChaTRoomApp.View;
using System.Threading;
using ChaTRoomApp.DataBase;
using System.Collections.ObjectModel;

namespace ChaTRoomApp.ViewModels
{
    public class MainModel : INotifyPropertyChanged, IMethodController, IClient, IMessageAdd
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public Command<IlistItem> SelectCommand { get; }

        public INetxSClient Current { get => DependencyService.Get<ClientService>().Client; set { } }

        private bool isvisible;
        public bool IsVisible { get { return isvisible; } set { isvisible = value; PropertyChange(); } }

        public Page MainPage { get; }

        private ObservableRangeCollection<IlistItem> _users;

        public ObservableRangeCollection<IlistItem> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                PropertyChange();
            }
        }

        public User My { get; private set; }


        public event EventHandler<MessageTable> MessageAdd;


        public MainModel(Page page)
        {
            Users = new ObservableRangeCollection<IlistItem>();
            MainPage = page;
            SelectCommand = new Command<IlistItem>(ShowMessage);
        }

        public async void LoadingUser()
        {
            try
            {
                if (My != null)
                    return;
           
                var (succ, user) = await Current.Get<IServer>().CheckLogIn();
                if (succ)
                {
                 

                    My = user;
                    var userlist = await Current.Get<IServer>().GetUsers();
                    Users.Clear();
                    Users.Add(new Room());
                    Users.AddRange(userlist);
                    Users.Sort(1, Users.Count - 1, new UserSort());
                    Current.LoadInstance(this);

                    foreach (var item in await Current.Get<IServer>().GetLeavingMessage())
                    {
                        SayMessage(item.FromUserId, item.NickName, 1, item.MessageContext, item.Time);
                    }

                    IsVisible = true;
                }
                else
                {
                    await MainPage.Navigation.PushModalAsync(new LogOnPage());
                }
            }
            catch (Exception er)
            {

                var select = await MainPage.DisplayActionSheet("ERROR:" + er.Message, "Close", "Retry Connection");

                switch (select)
                {
                    case "Retry Connection":
                        {
                            LoadingUser();
                        }
                        break;
                    default:
                        {
                            Thread.CurrentThread.Abort();
                        }
                        break;
                }
            }
        }

        public void SetUserStats(long userid, byte status)
        {
            var user = Users.FirstOrDefault(p =>
             {
                 if (p is User role)
                 {
                     if (role.UserId == userid)
                         return true;
                 }

                 return false;
             }) as User;

            if (user != null)
            {
                user.OnLineStatus = status;
                Users.Sort(1, Users.Count - 1, new UserSort());
            }
        }

        public async void SayMessage(long fromuserId, string fromusername, byte MsgType, string msg, long time)
        {
            var database = DependencyService.Get<ChaTDataBaseManager>();

            var t_msg = new MessageTable()
            {
                FromId = fromuserId,
                FromName = fromusername,
                TargetId=My.UserId,
                TargetName=My.NickName,
                MessageContext = msg,
                IsRight=fromuserId==My.UserId?false: true,
                MsgType = MsgType,
                Time = TimeHelper.GetTime(time)
            };

            await database.SaveMessage(t_msg);

            MessageAdd?.Invoke(this, t_msg);


        }

        public async void ShowMessage(IlistItem type)
        {
            if (type.ImageId == "room")
            {
               await MainPage.Navigation.PushAsync(new SayShowPage(this,0,nickname:"Public Room"));
            }
            else if(type is User fromuser)
            {
                await MainPage.Navigation.PushAsync(new SayShowPage(this,1, fromuser.UserId,fromuser.NickName));
            }
        }

        public async void NeedLogOn()
        {
            await MainPage.Navigation.PushModalAsync(new LogOnPage());
        }

        public void UserAdd(User newuser)
        {
            var user = Users.FirstOrDefault(p =>
            {
                if (p is User role)
                {
                    if (role.UserId == newuser.UserId)
                        return true;
                }

                return false;
            }) as User;

            if (user == null)
            {
                Users.Add(newuser);
                Users.Sort(1, Users.Count - 1, new UserSort());
            }
        }

    }
}
