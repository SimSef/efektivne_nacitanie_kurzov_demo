using Dapper;
using Microsoft.Data.SqlClient;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseSchemaAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (!string.IsNullOrWhiteSpace(databaseName))
        {
            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
            await masterConnection.OpenAsync();

            const string createDatabaseSql = @"
                IF DB_ID(@dbName) IS NULL
                BEGIN
                    DECLARE @name sysname = @dbName;
                    DECLARE @sql nvarchar(max) = 'CREATE DATABASE [' + @name + ']';
                    EXEC(@sql);
                END";

            await masterConnection.ExecuteAsync(createDatabaseSql, new { dbName = databaseName });
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string createSchemaSql = @"
            IF OBJECT_ID('dbo.Events', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Events (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
                    ProviderEventID BIGINT NOT NULL,
                    EventName NVARCHAR(200) NOT NULL,
                    EventDate DATETIME2(3) NOT NULL
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_Events_ProviderEventID'
                AND object_id = OBJECT_ID('dbo.Events')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_Events_ProviderEventID
                    ON dbo.Events(ProviderEventID);
            END;

            IF OBJECT_ID('dbo.Odds', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Odds (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Odds PRIMARY KEY,
                    ProviderOddsID BIGINT NOT NULL,
                    EventId INT NOT NULL,
                    OddsName NVARCHAR(100) NOT NULL,
                    OddsRate DECIMAL(9,3) NOT NULL,
                    Status NVARCHAR(32) NOT NULL,
                    CONSTRAINT FK_Odds_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_Odds_ProviderOddsID'
                AND object_id = OBJECT_ID('dbo.Odds')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_Odds_ProviderOddsID
                    ON dbo.Odds(ProviderOddsID);
            END;
            ";

        await connection.ExecuteAsync(createSchemaSql);
    }
}
