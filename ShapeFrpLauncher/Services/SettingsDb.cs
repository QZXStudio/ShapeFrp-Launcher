using System;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace AvaloniaApplication1.Services;

internal static class SettingsDb
{
    private static readonly string DbDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".QZXFrp", "DataBase", "AppSettings");

    private static readonly string DbPath = Path.Combine(DbDir, "Settings.db");

    private static string ConnectionString => $"Data Source={DbPath}";

    static SettingsDb()
    {
        Directory.CreateDirectory(DbDir);

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS "Settings.FrpcRelease" (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GitHubTokenConfig TEXT NOT NULL DEFAULT '',
                Source INTEGER NOT NULL DEFAULT 0
            );
            """;
        cmd.ExecuteNonQuery();

        // 兼容旧表：补 Source 列
        using var migrate = conn.CreateCommand();
        migrate.CommandText = """
            ALTER TABLE "Settings.FrpcRelease" ADD COLUMN Source INTEGER NOT NULL DEFAULT 0;
            """;
        try { migrate.ExecuteNonQuery(); } catch { /* 列已存在 */ }

        // 确保至少有一行数据
        using var check = conn.CreateCommand();
        check.CommandText = @"SELECT COUNT(1) FROM ""Settings.FrpcRelease""";
        if ((long)(check.ExecuteScalar() ?? 0) == 0)
        {
            using var insert = conn.CreateCommand();
            insert.CommandText = @"INSERT INTO ""Settings.FrpcRelease"" (GitHubTokenConfig) VALUES ('')";
            insert.ExecuteNonQuery();
        }
    }

    public static string? ReadToken()
    {
        try
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT GitHubTokenConfig FROM ""Settings.FrpcRelease"" WHERE Id = 1";
            var result = cmd.ExecuteScalar() as string;

            if (string.IsNullOrEmpty(result)) return null;

            var bytes = Convert.FromBase64String(result);
            return Encoding.UTF8.GetString(bytes);
        }
        catch { return null; }
    }

    public static void WriteToken(string? token)
    {
        try
        {
            var encoded = string.IsNullOrEmpty(token)
                ? ""
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE ""Settings.FrpcRelease"" SET GitHubTokenConfig = @token WHERE Id = 1";
            cmd.Parameters.AddWithValue("@token", encoded);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public static ReleaseSourceType ReadSource()
    {
        try
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Source FROM ""Settings.FrpcRelease"" WHERE Id = 1";
            return (ReleaseSourceType)(cmd.ExecuteScalar() as long? ?? 0);
        }
        catch { return ReleaseSourceType.GitHub; }
    }

    public static void WriteSource(ReleaseSourceType source)
    {
        try
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE ""Settings.FrpcRelease"" SET Source = @source WHERE Id = 1";
            cmd.Parameters.AddWithValue("@source", (int)source);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }
}
