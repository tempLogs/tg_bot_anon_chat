using System.Data.SQLite;

namespace tg_bot_anon_chat
{
    class DatabaseManager
    {
        private static readonly string dbFile = @"..\..\..\data\database.db";
        private static readonly string specialChar = "h34qfdi62d3k";

        public static void InitializeDB()
        {
            if (!File.Exists(dbFile))
                SQLiteConnection.CreateFile(dbFile);

            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER UNIQUE,
                Status TEXT
            );";

            string createMessagesTable = @"
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChatId TEXT,
                SenderId INTEGER,
                Message TEXT,
                Timestamp TEXT
            );";

            string createReportsTable = @"
            CREATE TABLE IF NOT EXISTS Reports (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                Description TEXT,
                Chat TEXT,
                Status TEXT
            );";

            using var cmd1 = new SQLiteCommand(createUsersTable, connection);
            using var cmd2 = new SQLiteCommand(createMessagesTable, connection);
            using var cmd3 = new SQLiteCommand(createReportsTable, connection);
            cmd1.ExecuteNonQuery();
            cmd2.ExecuteNonQuery();
            cmd3.ExecuteNonQuery();
        }

        public static bool CheckUserExists(long userId)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            return (long)command.ExecuteScalar() > 0;
        }

        public static bool AddUser(long userId, string status)
        {
            if (CheckUserExists(userId)) return false;

            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "INSERT INTO Users (UserId, Status) VALUES (@UserId, @Status);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Status", status);
            command.ExecuteNonQuery();
            return true;
        }

        public static void ChangeUserStatus(long userId, string status)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "UPDATE Users SET Status = @Status WHERE UserId = @UserId;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@UserId", userId);
            command.ExecuteNonQuery();
        }

        public static string CheckUserStatus(long userId)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "SELECT Status FROM Users WHERE UserId = @UserId;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            return command.ExecuteScalar()?.ToString() ?? "No Status";
        }

        public static void SaveMessage(long senderId, long receiverId, string message)
        {
            string chatId = senderId < receiverId ? $"{senderId}-{receiverId}" : $"{receiverId}-{senderId}";

            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string insertQuery = "INSERT INTO Messages (ChatId, SenderId, Message, Timestamp) VALUES (@ChatId, @SenderId, @Message, @Timestamp);";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@ChatId", chatId);
            command.Parameters.AddWithValue("@SenderId", senderId);
            command.Parameters.AddWithValue("@Message", message);
            command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
            command.ExecuteNonQuery();
        }

        private static string GetChatHistory(long senderId, long receiverId)
        {
            string chatId = senderId < receiverId ? $"{senderId}-{receiverId}" : $"{receiverId}-{senderId}";

            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string selectQuery = "SELECT SenderId, Message, Timestamp FROM Messages WHERE ChatId = @ChatId ORDER BY Timestamp;";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@ChatId", chatId);
            using var reader = command.ExecuteReader();

            string? allMessages = null;

            while (reader.Read())
            {
                allMessages += $"{chatId} [{reader["Timestamp"]}]; SenderId: {reader["SenderId"]}: {reader["Message"]}{specialChar}{specialChar}";
            }
            return allMessages ?? "No data.";
        }

        public static void ReportUser(long reporter, long suspect, string description)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "INSERT INTO Reports (UserId, Description, Chat, Status) VALUES (@UserId, @Description, @Chat, @Status);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", suspect);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Chat", GetChatHistory(reporter, suspect));
            command.Parameters.AddWithValue("@Status", "Active");
            command.ExecuteNonQuery();
        }

        public static bool CheckReportsExists()
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "SELECT COUNT(*) FROM Reports;";

            using var command = new SQLiteCommand(query, connection);
            return (long)command.ExecuteScalar() > 0;
        }

        public static bool CheckReportExists(long userId)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string query = "SELECT COUNT(*) FROM Reports WHERE UserId = @UserId;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            return (long)command.ExecuteScalar() > 0;
        }

        public static string GetReports()
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string selectQuery = "SELECT UserId, Description, Chat FROM Reports WHERE Status = @Status;";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@Status", "Active");
            using var reader = command.ExecuteReader();

            string? allReported = null;

            while (reader.Read())
            {
                allReported += $"Suspected: [{reader["UserId"]}]; Description: {reader["Description"]}; Chat:{specialChar}{reader["Chat"]}{specialChar}{specialChar}";
            }
            return allReported ?? "No data.";
        }

        public static void CancelReport(long userId)
        {
            using var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string selectQuery = "UPDATE Reports SET Status = @Status WHERE UserId = @UserId;";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Status", "Disable");
            command.ExecuteNonQuery();
        }
    }
}
