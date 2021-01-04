using System;

namespace NintendoNetcode.Pia.Encryption
{
    public class PiaInetEncryptionArgs : PiaEncryptionArgs
    {
        private uint GatheringId;

        public PiaInetEncryptionArgs(byte[] key, uint gatheringid) : base(key)
        {
            GatheringId = gatheringid;
        }

        public override byte[] GetNonce(byte connectionId, byte[] headerNonce)
        {
            byte[] nonce = new byte[12];

            nonce[0] = connectionId;

            nonce[1] = (byte)((GatheringId >> 16) & 0xFF);
            nonce[2] = (byte)((GatheringId >> 8) & 0xFF);
            nonce[3] = (byte)(GatheringId & 0xFF);

            Array.Copy(headerNonce, 0, nonce, 4, 8);

            return nonce;
        }

    }
}
