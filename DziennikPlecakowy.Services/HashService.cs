using System.Text;
using DziennikPlecakowy.Interfaces;

namespace DziennikPlecakowy.Services;

public class HashService : IHashService
{
    public HashService() { }
    public string Hash(string input)
    {
        var bytes = new UTF8Encoding().GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
