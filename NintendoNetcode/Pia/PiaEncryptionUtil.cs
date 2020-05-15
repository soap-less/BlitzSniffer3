using System;
using System.Security.Cryptography;

namespace NintendoNetcode.Pia
{
    public static class PiaEncryptionUtil
    {
        public static byte[] GenerateLanSessionKey(byte[] sessionParam, byte[] gameKey)
        {
            byte[] param = new byte[sessionParam.Length];
            Array.Copy(sessionParam, param, sessionParam.Length);

            param[31]++;

            HMACSHA256 hmacInstance = new HMACSHA256(gameKey);
            byte[] hmac = hmacInstance.ComputeHash(param);

            byte[] sessionKey = new byte[16];
            Array.Copy(hmac, sessionKey, 16);

            return sessionKey;
        }

    }
}
