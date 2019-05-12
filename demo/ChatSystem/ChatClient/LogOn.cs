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
    public partial class LogOn : Form
    {
        public LogOn()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Register register = new Register();
            register.ShowDialog();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            var service= Dependency.Client.Get<IServer>();

            try
            {
               var (success,msg)= await  service.LogOn(this.textBox1.Text, this.textBox2.Text);

                if (success)
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show(msg);
                }

            }
            catch(Netx.NetxException er)
            {
                MessageBox.Show(er.Message);
            }
        }

    }
}
