using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

public class UserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> SaveUserAsync(string firstName, string lastName, string email, string password, string state, string organization)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, State, Organization) VALUES (@FirstName, @LastName, @Email, @PasswordHash, @State, @Organization)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.AddWithValue("@Organization", organization);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT PasswordHash FROM dbo.Users WHERE Email = @Email";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
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

    public async Task<bool> ValidateEmailAsync(string email)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT COUNT(*) FROM dbo.Users WHERE Email = @Email";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                await conn.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE Email = @Email";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
