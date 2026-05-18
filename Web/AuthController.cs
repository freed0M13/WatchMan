using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Watchman.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string DbPassword = "Watchman!9Qd7npDk";
        private readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "monitor.db");

        private string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Password = DbPassword
            }.ToString();

        public AuthController()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var createUsersCmd = connection.CreateCommand();
            createUsersCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS USERS (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            PasswordHash TEXT NOT NULL,
            Role TEXT NOT NULL DEFAULT 'READ_ONLY',
            LastLogin DATETIME
        );";
            createUsersCmd.ExecuteNonQuery();

            using var updateAdminCmd = connection.CreateCommand();
            updateAdminCmd.CommandText = @"
        UPDATE USERS 
        SET Role = 'SYS_ADMIN' 
        WHERE Username = 'admin';";
            updateAdminCmd.ExecuteNonQuery();

            using var insertAdminCmd = connection.CreateCommand();
            insertAdminCmd.CommandText = @"
        INSERT OR IGNORE INTO USERS (Username, PasswordHash, Role) 
        VALUES ('admin', @hash, 'SYS_ADMIN');";
            insertAdminCmd.Parameters.AddWithValue("@hash", ComputeSha256Hash("admin"));
            insertAdminCmd.ExecuteNonQuery();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
                return BadRequest("Kullanıcı adı ve şifre boş olamaz.");

            string hash = ComputeSha256Hash(req.Password);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Role 
                FROM USERS 
                WHERE Username = @user AND PasswordHash = @hash;";
            command.Parameters.AddWithValue("@user", req.Username);
            command.Parameters.AddWithValue("@hash", hash);

            var role = command.ExecuteScalar()?.ToString();

            if (role != null)
            {
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE USERS 
                    SET LastLogin = CURRENT_TIMESTAMP 
                    WHERE Username = @user;";
                updateCmd.Parameters.AddWithValue("@user", req.Username);
                updateCmd.ExecuteNonQuery();

                return Ok(new
                {
                    Success = true,
                    Username = req.Username,
                    Role = role
                });
            }

            return Unauthorized(new
            {
                Success = false,
                Message = "Geçersiz kullanıcı adı veya şifre."
            });
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = new List<object>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Username, Role, LastLogin 
                FROM USERS;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new
                {
                    Name = reader.GetString(0),
                    Role = reader.GetString(1),
                    LastAuth = reader.IsDBNull(2)
                        ? "Hiç giriş yapmadı"
                        : reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = "offline"
                });
            }

            return Ok(users);
        }

        [HttpPost("users")]
        public IActionResult CreateUser([FromBody] CreateUserRequest req)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO USERS (Username, PasswordHash, Role) 
                    VALUES (@user, @hash, @role);";

                command.Parameters.AddWithValue("@user", req.Username);
                command.Parameters.AddWithValue("@hash", ComputeSha256Hash(req.Password));
                command.Parameters.AddWithValue("@role", req.Role);

                command.ExecuteNonQuery();

                return Ok(new
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla oluşturuldu."
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Kullanıcı eklenemedi. Kullanıcı adı kullanılıyor olabilir."
                });
            }
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using SHA256 sha256Hash = SHA256.Create();

            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}