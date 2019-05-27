using ChaTRoomApp.DataBase;
using ChaTRoomApp.Models;
using Interfaces;
using Netx;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace ChaTRoomApp.ViewModels
{
    public class SayShowModel: INotifyPropertyChanged
    {
        public ObservableRangeCollection<MessageTable> ListMessages { get; }

        public INetxSClient Current { get; }

        public ILog Log { get; }
     

        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyChange([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _outText;
        public string OutText { get { return _outText; } set { _outText = value; PropertyChange(); } }

        public Command SendCommand { get; }

        public Command SelectItemCommand { get; }

        public byte type { get; }

        public long fromId { get; }

        public ListView ShowListView { get; }


        public SayShowModel(ListView listView, IMessageAdd messageAdd,byte type,long fromId = -1,string target=null)
        {
            ListMessages = new ObservableRangeCollection<MessageTable>();
            messageAdd.MessageAdd += MessageAdd_MessageAdd;

            Current=DependencyService.Get<ClientService>().Client;
            Log = new DefaultLog(Current.GetLogger<SayShowModel>());
            Loading(type, fromId);

            this.type = type;
            this.fromId = fromId;
            this.ShowListView = listView;
            SelectItemCommand =new Command(() =>{ });
            SendCommand = new Command(async () =>
              {
                  try
                  {
                      if (!string.IsNullOrEmpty(OutText))
                      {
                          var (succ, msg) = await Current.Get<IServer>().Say(fromId, OutText);

                          if (!succ)
                              Log.Error(msg);
                          else
                          {
                              var database = DependencyService.Get<ChaTDataBaseManager>();

                              if (type != 0)
                              {
                                  var t_msg = new MessageTable()
                                  {
                                      FromId = messageAdd.My.UserId,
                                      FromName = messageAdd.My.NickName,
                                      TargetId = fromId,
                                      TargetName = target,
                                      MessageContext = OutText,
                                      MsgType = type,
                                      Time = DateTime.Now
                                  };

                                  await database.SaveMessage(t_msg);

                                  ListMessages.Add(t_msg);
                                  MoveEnd();
                              }

                              OutText = null;

                          }
                      }

                  }
                  catch (Exception er)
                  {
                      Log.Error(er);
                  }

              });
        }

        private void MoveEnd()
        {
            if (ListMessages.Count > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowListView.ScrollTo(ListMessages[ListMessages.Count - 1], ScrollToPosition.End, false);
                });
            }
        }

        private async void Loading(byte type,long fromId)
        {
            try
            {
                var database = DependencyService.Get<ChaTDataBaseManager>();

                var msglist = await database.GetMessage(type, fromId);

                ListMessages.AddRange(msglist);
                MoveEnd();
            }
            catch(Exception er)
            {
                Log.Error(er);
            }
        }

        private void MessageAdd_MessageAdd(object sender, MessageTable e)
        {
            if (e.MsgType == type && type == 0)
            {
                ListMessages.Add(e);
            }
            else if (e.FromId == fromId)
            {
                ListMessages.Add(e);
            }

            MoveEnd();

        }
    }
}
