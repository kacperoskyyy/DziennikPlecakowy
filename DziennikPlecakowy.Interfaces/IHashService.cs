namespace DziennikPlecakowy.Interfaces;

public interface IHashService
{
    public string Hash(string input);
    public bool Verify(string input, string storedHash);
    public string HashShortToken(string token);
    bool IsLegacyMd5Hash(string stored);
    string HashLegacyMd5(string input);

}
