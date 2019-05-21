using Interfaces;
using Netx;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class WinMain : Form, IMethodController, IClient
    {
        public INetxSClient Current { get=>Dependency.Client; set { } }

        public T Get<T>() => Current.Get<T>();

        public User My { get; private set; }

        public WinMain()
        {
            InitializeComponent();
        }

        private async void WinMain_Load(object sender, EventArgs e)
        {
            await LoginServer();
            await LoadingUserList();
            await GetLGetLeaving();
            Dependency.Client.LoadInstance(this);
        }


        private async Task GetLGetLeaving()
        {
            foreach (var item in await Get<IServer>().GetLeavingMessage())
            {
                SayMessage(item.FromUserId, item.NickName, item.MsgType, item.MessageContext, item.Time);
            }
        }

        private async Task LoadingUserList()
        {
            try
            {
                var userlist = await Get<IServer>().GetUsers();

                this.listView1.Items.Clear();

                var select = this.comboBox1.SelectedIndex;
                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("ALL");

                foreach (var user in userlist)
                {
                    ListViewItem item = new ListViewItem(user.NickName)
                    {
                        Tag = user
                    };
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, user.OnLineStatus == 0 ? "Offline" : user.OnLineStatus == 2 ? "Leave" : "Online"));
                    this.listView1.Items.Add(item);
                    this.comboBox1.Items.Add(user);
                }

                if (select < this.comboBox1.Items.Count && select != -1)
                {
                    this.comboBox1.SelectedIndex = select;
                }
                else
                {
                    this.comboBox1.SelectedIndex = 0;
                }
            }
            catch (NetxException er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private async Task LoginServer()
        {

            try
            {
                var (success, my) = await Get<IServer>().CheckLogIn();
                if (!success)
                {
                    LogOn logOn = new LogOn();
                    logOn.ShowDialog();

                    (success, my) = await Get<IServer>().CheckLogIn();

                    if (!success)                    
                        this.Close();                     
                    else                    
                        My = my;                    
                }
                else                
                    My = my;
                 
            }
            catch (Netx.NetxException er)
            {
                MessageBox.Show(er.Message);
                this.Close(); 
            }

        }


        public void UserAdd(User newuser)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Tag is User user)
                    {
                        if (user.UserId == newuser.UserId)
                            return;
                    }
                }

                ListViewItem newitem = new ListViewItem(newuser.NickName)
                {
                    Tag = newuser
                };
                newitem.SubItems.Add(new ListViewItem.ListViewSubItem(newitem, newuser.OnLineStatus == 0 ? "Offline" : newuser.OnLineStatus == 2 ? "Leave" : "Online"));
                this.listView1.Items.Add(newitem);
                this.comboBox1.Items.Add(newuser);

            }));
        }


        public void SetUserStats(long userid, byte status)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {

                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Tag is User user)
                    {
                        if (user.UserId == userid)
                        {
                            user.OnLineStatus = status;
                            item.SubItems[1].Text = user.OnLineStatus == 0 ? "Offline" : user.OnLineStatus == 2 ? "Leave" : "Online";
                        }
                    }
                }
            }));
        }


        public void SayMessage(long fromuserId, string fromusername, byte msgType, string msg, long time = 0)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {

                switch (msgType)
                {
                    case 0:
                        {
                            this.richTextBox1.AppendText($"[A]{fromusername} {DateTime.Now.ToString("T")} \r\n{msg}\r\n");
                        }
                        break;
                    case 1:
                        {
                            this.richTextBox1.AppendText($"       [P]{fromusername} {DateTime.Now.ToString("T")} \r\n           {msg}\r\n");
                        }
                        break;
                    case 2:
                        {
                            if (time == 0)
                                this.richTextBox1.AppendText($" [L]{fromusername} {DateTime.Now.ToString("T")} \r\n   {msg}\r\n");
                            else
                                this.richTextBox1.AppendText($" [L]{fromusername} {TimeHelper.GetTime(time).ToString("T")} \r\n   {msg}\r\n");
                        }
                        break;
                }
            }));
        }


        public void NeedLogOn()
        {
            this.BeginInvoke(new EventHandler(async (a, b) =>
            {
                await LoginServer();
                await LoadingUserList();
                await GetLGetLeaving();
            }));
        }



        private async void Button1_Click(object sender, EventArgs e)
        {
            var select = this.comboBox1.SelectedItem;

            try
            {
                string msg = this.textBox1.Text;

                long userid = -1;

                if (select is User user)
                {
                    userid = user.UserId;
                    this.richTextBox1.AppendText($"  ->{user.NickName} {DateTime.Now.ToString("T")}\r\n   {msg}\r\n");
                }


                await Get<IServer>().Say(userid, msg);


            }
            catch (NetxException er)
            {
                MessageBox.Show(er.Message);
            }

        }
    }
}
