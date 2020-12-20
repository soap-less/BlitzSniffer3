using NintendoNetcode.Pia.Lan.Content.Browse.Crypto;
using System;
using System.IO;
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

        public static LanCryptoContentChallenge GenerateLanCryptoChallenge(byte[] broadcastAddress, long counter, byte[] gameKey)
        {
            byte[] counterArray = BitConverter.GetBytes(counter);

            if (BitConverter.IsLittleEndian)
            {
                // Convert to network order
                // Array.Reverse(broadcastAddress); // already in network order
                Array.Reverse(counterArray);
            }

            byte[] nonce = new byte[12];
            Array.Copy(broadcastAddress, nonce, 4);
            Array.Copy(counterArray, 0, nonce, 4, 8);

            RNGCryptoServiceProvider secureRandom = new RNGCryptoServiceProvider();

            byte[] challengeKey = new byte[16];
            secureRandom.GetBytes(challengeKey);

            byte[] challengeData = new byte[256];
            secureRandom.GetBytes(challengeData);

            byte[] encryptionKey;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                aes.Key = gameKey;
                aes.IV = new byte[16]; // empty IV for ECB

                using (MemoryStream encryptedStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(challengeKey);
                    }

                    encryptionKey = encryptedStream.ToArray();
                }
            }

            byte[] ciphertext = new byte[256];
            byte[] tag = new byte[16];

            using (AesGcm aesGcm = new AesGcm(encryptionKey))
            {
                aesGcm.Encrypt(nonce, challengeData, ciphertext, tag);
            }

            return new LanCryptoContentChallenge()
            {
                Version = 1, // old InetAddress format without IPv6 support
                CryptoEnabled = true,
                Counter = counter,
                ChallengeKey = challengeKey,
                Tag = tag,
                Data = ciphertext
            };
        }

    }
}
