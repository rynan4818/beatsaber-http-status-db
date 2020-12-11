using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus
{
    public class StatusChangedEventArgs : EventArgs
    {
        public ChangedProperties ChangedProperties { get; private set; }
        public string Cause { get; private set; }

        public StatusChangedEventArgs(ChangedProperties changedProperties, string cause)
        {
            this.ChangedProperties = changedProperties;
            this.Cause = cause;
        }
    }

    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
}

