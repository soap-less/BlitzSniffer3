using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoNetcode.Pia.Lan.Content.Browse
{
    public class LanContentBrowseReply : LanContent
    {
        public LanSessionInfo SessionInfo
        {
            get;
            set;
        }

        public byte[] CryptoChallengeReply
        {
            get;
            set;
        }

        public LanContentBrowseReply(BinaryDataReader reader) : base(reader)
        {

        }

        protected override void Deserialize(BinaryDataReader reader)
        {
            uint sessionInfoSize = reader.ReadUInt32();
            SessionInfo = new LanSessionInfo(reader);
            CryptoChallengeReply = reader.ReadBytes(0x3a);
        }
    }
}
