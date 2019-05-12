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
