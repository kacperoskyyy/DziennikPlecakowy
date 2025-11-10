using SQLite;

namespace DziennikPlecakowy.Models.Local;
// lokalny model odświeżającego tokena

[Table("refresh_token")]
public class LocalRefreshToken
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
}
