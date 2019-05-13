using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class WinMain : Form
    {
        public WinMain()
        {
            InitializeComponent();
        }

        private async void WinMain_Load(object sender, EventArgs e)
        {           
            await LoginServer();

            await LoadingUserList();


        }

        private async Task LoadingUserList()
        {
            var userlist = await Dependency.Client.Get<IServer>().GetUsers();

            this.listView1.Items.Clear();

            foreach (var user in userlist)
            {
                ListViewItem item = new ListViewItem(user.NickName);
                item.Tag = user;
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, user.OnLineStatus == 0 ? "离线" : user.OnLineStatus == 2 ? "离开" : "在线"));
                this.listView1.Items.Add(item);
            }
        }

        private async Task LoginServer()
        {
            while (true)
            {
                try
                {
                    if (!await Dependency.Client.Get<IServer>().CheckLogIn())
                    {
                        LogOn logOn = new LogOn();
                        logOn.ShowDialog();

                        if(!await Dependency.Client.Get<IServer>().CheckLogIn())
                        {
                            this.Close();
                        }

                    }
                    else
                        break;
                }
                catch (Netx.NetxException er)
                {
                    MessageBox.Show(er.Message);
                    break;
                }

            }
        }

    }
}
