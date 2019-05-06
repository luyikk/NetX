using System;
using System.Collections.Generic;
using System.Text;
using Netx.Loggine;

namespace Netx.Service
{
    public abstract class AsyncController
    {
        internal AsyncToken Async { get; set; }       

        protected AsyncToken Current{get=>Async; }
        
     
        /// <summary>
        /// 断线处理
        /// </summary>
        public virtual void Disconnect()
        {

        }

        /// <summary>
        /// 彻底结束
        /// </summary>
        public virtual void Closed()
        {

        }
      

    }
}
