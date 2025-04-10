using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class UserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Save new user using UserDto
    public async Task<bool> SaveUserAsync(UserDto user)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                string query = @"
                    INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, State, Organization)
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @State, @Organization)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@State", user.State);
                    cmd.Parameters.AddWithValue("@Organization", string.IsNullOrWhiteSpace(user.Organization) ? DBNull.Value : (object)user.Organization);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SaveUserAsync(UserDto)] Error: " + ex.Message);
            Console.WriteLine("[SaveUserAsync(UserDto)] StackTrace: " + ex.StackTrace);
            return false;
        }
    }

    // Validate user credentials
    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT PasswordHash FROM dbo.Users WHERE LOWER(Email) = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
                    await conn.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        string hashedPassword = result.ToString();
                        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ValidateUserAsync] Error: " + ex.Message);
            return false;
        }
    }

    // Check if email is already registered
    public async Task<bool> ValidateEmailAsync(string email)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM dbo.Users WHERE LOWER(Email) = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
                    await conn.OpenAsync();

                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ValidateEmailAsync] Error: " + ex.Message);
            return false;
        }
    }

    // Reset user password
    public async Task<bool> ResetPasswordAsync(string email, string newHashedPassword)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE LOWER(Email) = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@PasswordHash", newHashedPassword);
                    await conn.OpenAsync();

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ResetPasswordAsync] Error: " + ex.Message);
            return false;
        }
    }

    // Get user info by email
    public async Task<User> GetUserByEmailAsync(string email)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT FirstName, LastName, Email, PasswordHash, State, Organization FROM dbo.Users WHERE LOWER(Email) = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["PasswordHash"].ToString(),
                                State = reader["State"].ToString(),
                                Organization = reader["Organization"]?.ToString()
                            };
                        }
                        return null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[GetUserByEmailAsync] Error: " + ex.Message);
            return null;
        }
    }
}
