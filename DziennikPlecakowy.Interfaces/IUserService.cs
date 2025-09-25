using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    public interface IUserService
    {
        public Task<int> UserRegister(UserRegisterRequest userRegister);
        public Task<User?> GetUserByEmail(string email);
        public Task<User?> GetUserById(string id);
        public Task<int> UpdateUser(User user);
        public Task<int> DeleteUser(User user);
        public Task<int> ChangePassword(User user, string newPassword);
        public Task<int> ChangeEmail(User user, string newEmail);
        public Task<int> ChangeName(User user, string newUsername);
        public bool CheckPassword(User user, string password);
        public Task<int> SetLastLogin(string Id);
        public Task<int> SetAdmin(User user);
    }
}
