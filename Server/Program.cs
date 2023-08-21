using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

class Server
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

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();

        byte[] key = Encoding.ASCII.GetBytes("YourAESKey123456"); // DO NOT use a hardcoded key in production!
        byte[] iv = new byte[16]; // Initialization Vector (IV) should be unique and unpredictable in a real-world scenario.

        Console.WriteLine("Server started. Waiting for connections...");

        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Client connected.");

        NetworkStream stream = client.GetStream();

        byte[] data = new byte[4096];
        int bytesRead = stream.Read(data, 0, data.Length);
        string encryptedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);

        // Decrypt the received message
        string decryptedMessage = Decrypt(encryptedMessage, key, iv);

        Console.WriteLine("Received message from client: " + decryptedMessage);

        // Respond to the client with an encrypted message
        string responseMessage = "Hello, client!";
        string encryptedResponse = Encrypt(responseMessage, key, iv);

        byte[] responseData = Encoding.ASCII.GetBytes(encryptedResponse);
        stream.Write(responseData, 0, responseData.Length);

        stream.Close();
        client.Close();
        server.Stop();
    }
}
