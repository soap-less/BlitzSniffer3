﻿using NintendoNetcode.Enl.Record;
using System;

namespace BlitzSniffer.Enl
{
    public class SystemInfoReceivedEventArgs : EventArgs
    {
        public EnlSystemInfoRecord Record
        {
            get;
            private set;
        }

        public SystemInfoReceivedEventArgs(EnlSystemInfoRecord record)
        {
            Record = record;
        }

    }
}
