using Interfaces;
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
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                var service = Dependency.Client.Get<IServer>();
                var (success, msg) = await service.Register(new Users()
                {
                    UserName = this.textBox1.Text,
                    PassWord = this.textBox2.Text,
                    NickName = this.textBox3.Text
                });

                if (success)
                {
                    MessageBox.Show("Register success!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show(msg);
                }
            }
            catch (Netx.NetxException er)
            {
                MessageBox.Show(er.Message);
            }
        }
    }
}
