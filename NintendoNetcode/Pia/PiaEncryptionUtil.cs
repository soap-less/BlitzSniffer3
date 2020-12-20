using System;
using System.Security.Cryptography;

namespace NintendoNetcode.Pia
{
    public static class PiaEncryptionUtil
    {
        public static byte[] BlitzGameKey = { 0xee, 0x18, 0x2a, 0x63, 0xe2, 0x16, 0xcd, 0xb1, 0xf5, 0x1a, 0xd4, 0xbe, 0xd8, 0xcf, 0x65, 0x08 };

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
