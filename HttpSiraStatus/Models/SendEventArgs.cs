using HttpSiraStatus.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpSiraStatus
{
    public class SendEventArgs
    {
        public JSONNode JsonNode { get; private set; }
        public SendEventArgs(JSONNode node)
        {
            this.JsonNode = node;
        }
    }

    public delegate void SendEventHandler(object sender, SendEventArgs e);
}
