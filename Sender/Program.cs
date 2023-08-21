using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Sender
{
    class Program
    {
        private static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes;
                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }
                return Convert.ToBase64String(encryptedBytes);
            }
        }
        static void Main(string[] args)
        {
            Console.Title = "SENDER";
            Console.OutputEncoding = Encoding.UTF8;
            var address = IPAddress.Parse("127.0.0.1");
            var serverEndpoint = new IPEndPoint(address, 1308);
            while (true)
            {
                Console.Write("INPUT: ");
                var request = Console.ReadLine();
                var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                client.Connect(serverEndpoint);
                var stream = new NetworkStream(client);
                stream.Flush();
                byte[] key = Encoding.ASCII.GetBytes("YourAESKey123456"); 
                byte[] iv = new byte[16]; 
                string encryptedMessage = Encrypt(request, key, iv);
                byte[] data = Encoding.ASCII.GetBytes(encryptedMessage);
                stream.Write(data, 0, data.Length);
                client.Close();
            }
        }
    }
}