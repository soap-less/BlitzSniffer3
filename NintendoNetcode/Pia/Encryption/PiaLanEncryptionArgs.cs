using System;

namespace NintendoNetcode.Pia.Encryption
{
    public class PiaLanEncryptionArgs : PiaEncryptionArgs
    {
        private byte[] SourceIp;

        public PiaLanEncryptionArgs(byte[] key, byte[] sourceIp) : base(key)
        {
            SourceIp = sourceIp;
        }

        public override byte[] GetNonce(byte connectionId, byte[] headerNonce)
        {
            byte[] nonce = new byte[12];

            Array.Copy(SourceIp, nonce, 4);
            nonce[4] = connectionId;
            Array.Copy(headerNonce, 1, nonce, 5, 7);

            return nonce;
        }

    }
}
