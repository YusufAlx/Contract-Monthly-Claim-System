using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Contract_Monthly_Claim_System.Data
{
    public static class DatabaseHelper
    {
        private static readonly string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private static readonly string dbPath = Path.Combine(dbFolder, "claims.db");
        private static readonly string connectionString = $"Data Source={dbPath};Version=3;";

        static DatabaseHelper()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string createClaimsTable = @"
                CREATE TABLE IF NOT EXISTS Claims (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TrackingID TEXT UNIQUE,
                    LecturerName TEXT,
                    HourlyRate REAL,
                    HoursWorked REAL,
                    MonthlyClaim REAL,
                    Status TEXT DEFAULT 'Pending'
                );";

            string createDocumentsTable = @"
                CREATE TABLE IF NOT EXISTS Documents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClaimID INTEGER,
                    FileName TEXT,
                    FilePath TEXT,
                    FOREIGN KEY(ClaimID) REFERENCES Claims(Id)
                );";

            using var cmd = new SQLiteCommand(createClaimsTable, connection);
            cmd.ExecuteNonQuery();

            cmd.CommandText = createDocumentsTable;
            cmd.ExecuteNonQuery();
        }

        public static int InsertClaim(Claim claim)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = @"INSERT INTO Claims (TrackingID, LecturerName, HourlyRate, HoursWorked, MonthlyClaim, Status)
                           VALUES (@TrackingID, @LecturerName, @HourlyRate, @HoursWorked, @MonthlyClaim, @Status);
                           SELECT last_insert_rowid();";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@TrackingID", claim.TrackingID);
            cmd.Parameters.AddWithValue("@LecturerName", claim.LecturerName);
            cmd.Parameters.AddWithValue("@HourlyRate", claim.HourlyRate);
            cmd.Parameters.AddWithValue("@HoursWorked", claim.HoursWorked);
            cmd.Parameters.AddWithValue("@MonthlyClaim", claim.MonthlyClaim);
            cmd.Parameters.AddWithValue("@Status", claim.Status);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void InsertDocument(Document document)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = @"INSERT INTO Documents (ClaimID, FileName, FilePath)
                           VALUES (@ClaimID, @FileName, @FilePath);";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ClaimID", document.ClaimID);
            cmd.Parameters.AddWithValue("@FileName", document.FileName);
            cmd.Parameters.AddWithValue("@FilePath", document.FilePath);
            cmd.ExecuteNonQuery();
        }

        public static List<Claim> GetAllClaims()
        {
            var claims = new List<Claim>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM Claims ORDER BY Id DESC;";
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                claims.Add(new Claim
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    TrackingID = reader["TrackingID"].ToString(),
                    LecturerName = reader["LecturerName"].ToString(),
                    HourlyRate = Convert.ToDouble(reader["HourlyRate"]),
                    HoursWorked = Convert.ToDouble(reader["HoursWorked"]),
                    MonthlyClaim = Convert.ToDouble(reader["MonthlyClaim"]),
                    Status = reader["Status"].ToString()
                });
            }

            return claims;
        }

        public static Claim GetClaimByTrackingID(string trackingId)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM Claims WHERE TrackingID = @TrackingID;";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@TrackingID", trackingId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Claim
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    TrackingID = reader["TrackingID"].ToString(),
                    LecturerName = reader["LecturerName"].ToString(),
                    HourlyRate = Convert.ToDouble(reader["HourlyRate"]),
                    HoursWorked = Convert.ToDouble(reader["HoursWorked"]),
                    MonthlyClaim = Convert.ToDouble(reader["MonthlyClaim"]),
                    Status = reader["Status"].ToString()
                };
            }

            return null;
        }

        public static List<Document> GetDocumentsByClaimId(int claimId)
        {
            var documents = new List<Document>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM Documents WHERE ClaimID = @ClaimID;";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ClaimID", claimId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                documents.Add(new Document
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ClaimID = Convert.ToInt32(reader["ClaimID"]),
                    FileName = reader["FileName"].ToString(),
                    FilePath = reader["FilePath"].ToString()
                });
            }

            return documents;
        }

        public static void UpdateClaimStatus(int claimId, string newStatus)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "UPDATE Claims SET Status = @Status WHERE Id = @Id;";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Status", newStatus);
            cmd.Parameters.AddWithValue("@Id", claimId);
            cmd.ExecuteNonQuery();
        }
    }
}
