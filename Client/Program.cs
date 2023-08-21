using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

class Client
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
        TcpClient client = new TcpClient("localhost", 8888);

        byte[] key = Encoding.ASCII.GetBytes("YourAESKey123456"); // DO NOT use a hardcoded key in production!
        byte[] iv = new byte[16]; // Initialization Vector (IV) should be unique and unpredictable in a real-world scenario.

        string messageToSend = "Hello, server!";

        // Encrypt the message before sending
        string encryptedMessage = Encrypt(messageToSend, key, iv);

        // Send the encrypted message to the server
        byte[] data = Encoding.ASCII.GetBytes(encryptedMessage);
        NetworkStream stream = client.GetStream();
        stream.Write(data, 0, data.Length);

        // Receive and decrypt the server's response
        byte[] responseData = new byte[4096];
        int bytesRead = stream.Read(responseData, 0, responseData.Length);
        string encryptedResponse = Encoding.ASCII.GetString(responseData, 0, bytesRead);
        string decryptedResponse = Decrypt(encryptedResponse, key, iv);

        Console.WriteLine("Server response: " + decryptedResponse);

        stream.Close();
        client.Close();
    }
}
