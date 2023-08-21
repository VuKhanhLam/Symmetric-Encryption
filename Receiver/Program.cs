using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Receiver
{
    class Program
    {
        private static string Decrypt(string encryptedText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                string plaintext = null;
                using (var msDecrypt = new System.IO.MemoryStream(encryptedBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return plaintext;
            }
        }
        static void Main(string[] args)
        {
            Console.Title = "RECEIVER";
            var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 1308));
            listener.Listen(10);
            Console.WriteLine($"Connect successfully started at {listener.LocalEndPoint} ");
            
        byte[] key = Encoding.ASCII.GetBytes("YourAESKey123456");
        byte[] iv = new byte[16];
            while (true)
            {
                var worker = listener.Accept();
                var stream = new NetworkStream(worker);
                var reader = new StreamReader(stream);
                
                byte[] data = new byte[4096];
                int bytesRead = stream.Read(data, 0, data.Length);

                string encryptedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);

                Console.WriteLine( $"Your encrypted message: {encryptedMessage}");
                string decryptedMessage = Decrypt(encryptedMessage, key, iv);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Received message from sender: " + decryptedMessage);
                Console.ResetColor();

                var writer = new StreamWriter(stream) { AutoFlush = true };
                worker.Close();
            }
        }
    }
}