using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class UserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> SaveUserAsync(string firstName, string lastName, string email, string password, string state, string organization)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                   INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, State, Organization)
                   VALUES (@FirstName, @LastName, @Email, @PasswordHash, @State, @Organization)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    byte[] hashedPassword = HashPassword(password);

                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@State", string.IsNullOrEmpty(state) ? DBNull.Value : (object)state);
                    cmd.Parameters.AddWithValue("@Organization", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization);

                    conn.Open();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving user: " + ex.Message);
            return false;
        }
    }

    public void SaveUser(string firstName, string lastName, string email, string password, string state, string organization)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand("INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, State, Organization) VALUES (@FirstName, @LastName, @Email, @PasswordHash, @State, @Organization)", connection))
            {
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(password));
                command.Parameters.AddWithValue("@State", state);
                command.Parameters.AddWithValue("@Organization", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization);

                command.ExecuteNonQuery();
            }
        }
    }


    public bool ValidateUser(string email, string password)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand("SELECT PasswordHash FROM dbo.Users WHERE Email = @Email", connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    string hashedPassword = result.ToString();
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                return false;
            }
        }
    }


    private byte[] HashPassword(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
