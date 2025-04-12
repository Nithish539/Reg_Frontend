using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Reg_Frontend.Models
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> SaveUserAsync(UserDto user)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"INSERT INTO Users (FirstName, LastName, Email, Password, State, Organization) 
                              VALUES (@FirstName, @LastName, @Email, @Password, @State, @Organization)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@State", user.State);
                    cmd.Parameters.AddWithValue("@Organization", string.IsNullOrEmpty(user.Organization) ? (object)DBNull.Value : user.Organization);

                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    "SELECT * FROM Users WHERE LOWER(LTRIM(RTRIM(Email))) = @Email", connection);
                command.Parameters.AddWithValue("@Email", email.Trim().ToLower());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Email = reader["Email"].ToString(),
                          Password = reader["Password"].ToString()
                        };
                    }
                }

                return null;
            }
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<bool> ResetPasswordAsync(string email, string hashedPassword)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"UPDATE Users 
                              SET Password = @Password, ResetToken = NULL, TokenExpiry = NULL 
                              WHERE LOWER(Email) = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            return await GetUserByEmailAsync(email) != null;
        }

        public async Task<string> GenerateAndStoreResetTokenAsync(string email)
        {
            var token = GenerateSecureToken();
            var expiry = DateTime.UtcNow.AddMinutes(30);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"UPDATE Users 
                                 SET ResetToken = @Token, TokenExpiry = @Expiry 
                                 WHERE LOWER(Email) = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@Expiry", expiry);
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0 ? token : null;
                }
            }
        }

        public async Task<bool> IsResetTokenValidAsync(string email, string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    @"SELECT COUNT(*) 
                      FROM Users 
                      WHERE LOWER(Email) = @Email 
                      AND ResetToken = @Token 
                      AND TokenExpiry > GETUTCDATE()", connection);

                command.Parameters.AddWithValue("@Email", email.Trim().ToLower());
                command.Parameters.AddWithValue("@Token", token);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[16];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        internal async Task GetUserByResetTokenAsync(string token)
        {
      
        }
    }
}
