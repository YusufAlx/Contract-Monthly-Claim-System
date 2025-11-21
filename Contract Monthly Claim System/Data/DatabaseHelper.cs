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

            using var cmd = connection.CreateCommand();
            // Base tables
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Claims (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TrackingID TEXT UNIQUE,
                    LecturerName TEXT,
                    HourlyRate REAL,
                    HoursWorked REAL,
                    MonthlyClaim REAL,
                    Status TEXT DEFAULT 'Pending',
                    ApprovedBy TEXT,
                    ApprovedAt TEXT,
                    ApprovalReason TEXT
                );

                CREATE TABLE IF NOT EXISTS Documents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClaimID INTEGER,
                    FileName TEXT,
                    FilePath TEXT,
                    FOREIGN KEY(ClaimID) REFERENCES Claims(Id)
                );";
            cmd.ExecuteNonQuery();

            // Additional tables: Users, Invoices, AuditLog
            EnsureAdditionalTables(connection);
        }

        private static void EnsureAdditionalTables(SQLiteConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE,
                    FullName TEXT,
                    PasswordHash TEXT,
                    Role TEXT
                );

                CREATE TABLE IF NOT EXISTS Invoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClaimID INTEGER,
                    InvoiceNumber TEXT UNIQUE,
                    Amount REAL,
                    GeneratedAt TEXT,
                    FilePath TEXT,
                    FOREIGN KEY(ClaimID) REFERENCES Claims(Id)
                );

                CREATE TABLE IF NOT EXISTS AuditLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntityType TEXT,
                    EntityId INTEGER,
                    Action TEXT,
                    PerformedBy TEXT,
                    PerformedAt TEXT,
                    Details TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        // -------- Claims & Documents -------------
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
            cmd.Parameters.AddWithValue("@Status", claim.Status ?? "Pending");

            var id = Convert.ToInt32(cmd.ExecuteScalar());
            InsertAudit("Claim", id, "Created", claim.LecturerName, $"Claim created with tracking {claim.TrackingID}");
            return id;
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

            InsertAudit("Document", document.ClaimID, "Uploaded", document.FileName, $"Uploaded file {document.FileName}");
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
                    Status = reader["Status"].ToString(),
                };
            }
            return null;
        }

        public static List<Document> GetDocumentsByClaimId(int claimId)
        {
            var list = new List<Document>();
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, ClaimID, FileName, FilePath FROM Documents WHERE ClaimID = @cid;";
            cmd.Parameters.AddWithValue("@cid", claimId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Document
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ClaimID = Convert.ToInt32(reader["ClaimID"]),
                    FileName = reader["FileName"].ToString(),
                    FilePath = reader["FilePath"].ToString()
                });
            }
            return list;
        }

        public static List<Claim> GetAllClaims()
        {
            var list = new List<Claim>();
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, TrackingID, LecturerName, HourlyRate, HoursWorked, MonthlyClaim, Status FROM Claims ORDER BY Id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Claim
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
            return list;
        }

        // -------- Audit helpers -------------
        public static void InsertAudit(string entityType, int entityId, string action, string performedBy, string details = null)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO AuditLog (EntityType, EntityId, Action, PerformedBy, PerformedAt, Details)
                                VALUES (@et, @id, @action, @by, @at, @details);";
            cmd.Parameters.AddWithValue("@et", entityType);
            cmd.Parameters.AddWithValue("@id", entityId);
            cmd.Parameters.AddWithValue("@action", action);
            cmd.Parameters.AddWithValue("@by", performedBy ?? "system");
            cmd.Parameters.AddWithValue("@at", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@details", details ?? "");
            cmd.ExecuteNonQuery();
        }

        // -------- Invoice helpers -------------
        public static int InsertInvoice(int claimId, string invoiceNumber, double amount, string filePath)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Invoices (ClaimID, InvoiceNumber, Amount, GeneratedAt, FilePath)
                                VALUES (@cid, @inv, @amt, @gen, @fp);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@cid", claimId);
            cmd.Parameters.AddWithValue("@inv", invoiceNumber);
            cmd.Parameters.AddWithValue("@amt", amount);
            cmd.Parameters.AddWithValue("@gen", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@fp", filePath);
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            InsertAudit("Invoice", id, "Created", "system", $"Invoice {invoiceNumber} created for claim {claimId}");
            return id;
        }

        public static List<(int Id, string InvoiceNumber, double Amount, string FilePath, string GeneratedAt, int ClaimID)> GetInvoices(string fromIso = null, string toIso = null)
        {
            var list = new List<(int, string, double, string, string, int)>();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, InvoiceNumber, Amount, FilePath, GeneratedAt, ClaimID FROM Invoices ORDER BY Id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5)));
            }
            return list;
        }

        public static List<(int Id, string InvoiceNumber, double Amount, string FilePath, string GeneratedAt)> GetInvoicesByClaimId(int claimId)
        {
            var list = new List<(int, string, double, string, string)>();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, InvoiceNumber, Amount, FilePath, GeneratedAt FROM Invoices WHERE ClaimID=@cid;";
            cmd.Parameters.AddWithValue("@cid", claimId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetString(3), reader.GetString(4)));
            }
            return list;
        }

        // -------- Approval with audit -------------
        public static void ApproveClaimWithAudit(int claimId, string approver, string reason = null)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Claims SET Status = 'Approved', ApprovedBy = @by, ApprovedAt = @at, ApprovalReason = @reason WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@by", approver);
            cmd.Parameters.AddWithValue("@at", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@reason", reason ?? "");
            cmd.Parameters.AddWithValue("@id", claimId);
            cmd.ExecuteNonQuery();
            InsertAudit("Claim", claimId, "Approved", approver, reason);
        }

        public static void RejectClaimWithAudit(int claimId, string approver, string reason = null)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Claims SET Status = 'Rejected', ApprovedBy = @by, ApprovedAt = @at, ApprovalReason = @reason WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@by", approver);
            cmd.Parameters.AddWithValue("@at", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@reason", reason ?? "");
            cmd.Parameters.AddWithValue("@id", claimId);
            cmd.ExecuteNonQuery();
            InsertAudit("Claim", claimId, "Rejected", approver, reason);
        }
    }
}