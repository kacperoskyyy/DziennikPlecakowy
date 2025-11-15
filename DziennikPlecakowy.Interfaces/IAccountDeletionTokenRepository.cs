using DziennikPlecakowy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Interfaces;

public interface IAccountDeletionTokenRepository
{
    Task AddAsync(AccountDeletionToken token);
    Task<AccountDeletionToken?> GetByHashedTokenAsync(string hashedToken);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteAllByUserIdAsync(string userId);
}
