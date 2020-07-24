using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx;
using Netx.Client;
namespace Client
{
    public class ClientController : MethodControllerBase, IClient
    {
        public async Task<int> Add(int a, int b)
        {
            await Task.Delay(2000);
            return a + b;
        }
    }
}
