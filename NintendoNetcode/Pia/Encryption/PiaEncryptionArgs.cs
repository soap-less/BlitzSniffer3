namespace NintendoNetcode.Pia.Encryption
{
    public abstract class PiaEncryptionArgs
    {
        public byte[] SessionKey
        {
            get;
            private set;
        }

        protected PiaEncryptionArgs(byte[] key)
        {
            SessionKey = key;
        }

        public abstract byte[] GetNonce(byte connectionId, byte[] headerNonce);

    }
}
