using System.Security.Cryptography;
using System.Text;
using DziennikPlecakowy.Interfaces;

public class HashService : IHashService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string input)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var pbkdf2 = new Rfc2898DeriveBytes(input, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);

        var hash = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, hash, 0, SaltSize);
        Buffer.BlockCopy(key, 0, hash, SaltSize, KeySize);

        return Convert.ToBase64String(hash);
    }

    public bool Verify(string input, string storedHash)
    {
        if (IsLegacyMd5Hash(storedHash))
        {
            return HashLegacyMd5(input) == storedHash;
        }

        var hashBytes = Convert.FromBase64String(storedHash);

        var salt = new byte[SaltSize];
        var key = new byte[KeySize];

        Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(hashBytes, SaltSize, key, 0, KeySize);

        var pbkdf2 = new Rfc2898DeriveBytes(input, salt, Iterations, HashAlgorithmName.SHA256);
        var testKey = pbkdf2.GetBytes(KeySize);

        return CryptographicOperations.FixedTimeEquals(key, testKey);
    }

    public bool IsLegacyMd5Hash(string stored)
    {
        return stored.Length == 24;
    }

    public string HashLegacyMd5(string input)
    {
        using var md5 = MD5.Create();
        return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }

    public string HashShortToken(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}
