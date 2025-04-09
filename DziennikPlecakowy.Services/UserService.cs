using MongoDB.Driver;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Shared;

namespace DziennikPlecakowy.Services
{
    public class UserService : IUserService
    {
        private readonly DziennikPlecakowyDbContext _context;
        private readonly IHashService _hash;
        private readonly ICypherService _cypher;

        public UserService(DziennikPlecakowyDbContext context, IHashService hash, ICypherService cypher)
        {
            _context = context;
            _hash = hash;
            _cypher = cypher;
        }

        public async Task<int> UserRegister(UserRegisterRequest userRegister)
        {
            try
            {
                User user = new User
                {
                    Email = userRegister.Email,
                    Username = userRegister.Username,
                    HashedPassword = _hash.Hash(userRegister.Password),
                    CreatedTime = DateTime.Now
                };
                await _context.Users.InsertOneAsync(user);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            try
            {
                var user = await _context.Users
                    .Find(u => u.Email == email)
                    .FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetUserById(string id)
        {
            try
            {
                var user = await _context.Users
                    .Find(u => u.Id == id)
                    .FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<int> UpdateUser(User user)
        {
            try
            {
                var result = await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
                return result.ModifiedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> DeleteUser(User user)
        {
            try
            {
                var result = await _context.Users.DeleteOneAsync(u => u.Id == user.Id);
                return result.DeletedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> DeleteUserById(string id)
        {
            try
            {
                var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
                return result.DeletedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> ChangePassword(User user, string newPassword)
        {
            try
            {
                user.HashedPassword = _hash.Hash(newPassword);
                return await UpdateUser(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> ChangeEmail(User user, string newEmail)
        {
            try
            {
                user.Email = newEmail;
                return await UpdateUser(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> ChangeName(User user, string newUsername)
        {
            try
            {
                user.Username = newUsername;
                return await UpdateUser(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        public bool CheckPassword(User user, string password)
        {
            return user.HashedPassword == _hash.Hash(password);
        }

        public bool IsAdmin(string Id)
        {
            try
            {
                var user = GetUserById(Id).Result;
                if (user == null)
                {
                    return false;
                }
                foreach (var role in user.Roles)
                {
                    if (role == UserRole.Admin)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return false;
            }
        }

        public async Task<int> SetLastLogin(string Id)
        {
            try
            {
                var user = await GetUserById(Id);
                user.LastLoginTime = DateTime.Now;
                return await UpdateUser(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> CheckIsSuperUser(User user)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }
                foreach (var role in user.Roles)
                {
                    if (role == UserRole.SuperUser)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return false;
            }
        }
        public async Task<int> SetSuperUser(string Id)
        {
            try
            {
                var user = await GetUserById(Id);
                if(user ==null)
                {
                    return -1; // User not found
                }
                user.Roles.Add(UserRole.SuperUser);
                return await UpdateUser(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }
    }
}
