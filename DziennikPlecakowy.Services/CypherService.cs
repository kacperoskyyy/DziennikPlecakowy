using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Shared;
using Microsoft.Extensions.Configuration;


namespace DziennikPlecakowy.Services
{
    public class CypherService : ICypherService
    {
        private static string? _key;
        private static string? _iv;
        public CypherService(IConfiguration _config)
        {
            //TODO: Odczytanie klucza i wektora inicjalizacyjnego z pliku
            //TODO: Obsługa błędów
            //TODO: Obsługa braku pliku
            //TODO: Obługa pustego klucza lub wektora inicjalizacyjnego
            _key = _config["Cypher:Key"];
            _iv = _config["Cypher:IV"];
        }
        //Metoda do zaszyfrowania tekstu
        public string Encrypt(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_key);
                aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(textBytes, 0, textBytes.Length);
                        csEncrypt.Close();
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        //Metoda do odszyfrowania tekstu
        public string Decrypt(string text)
        {
            byte[] textBytes = Convert.FromBase64String(text);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_key);
                aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(textBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
