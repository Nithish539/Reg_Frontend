using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

public class UserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void SaveUser(string firstName, string lastName, string email, string password, string state, string organization)
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
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving user: " + ex.Message);
        }
    }

    public bool ValidateUser(string email, string password)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT PasswordHash FROM dbo.Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            byte[] storedHash = (byte[])reader["PasswordHash"];
                            byte[] inputHash = HashPassword(password);

                          
                            return storedHash.SequenceEqual(inputHash);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error validating user: " + ex.Message);
        }

        return false;
    }
    private byte[] HashPassword(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

}