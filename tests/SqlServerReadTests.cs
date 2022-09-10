using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ErikNoren.Extensions.Configuration.SqlServer.Tests;

public class SqlServerReadTests
{
    const string SqlServerConnectionString = @"Server=localhost;Database=Config;User Id=sa;Password=A1234567a;";
    const string MySqlServerConnectionString = @"Server=localhost;Database=Config;Uid=root;Pwd=A1234567a;";
    const string SqlQuery = @"SELECT SettingKey, SettingValue FROM Settings WHERE IsActive = 1";

    [Fact]
    public void DatabaseSettingsAreLoadable_SqlServer()
    {
        using var cfgProvider = BuildConfig<SqlConnection, SqlCommand>(
            () => new SqlConnection(SqlServerConnectionString), db =>
                new SqlCommand(SqlQuery, db));
        
        AssertConfig(cfgProvider);
    }


    [Fact]
    public void DatabaseSettingsAreLoadable_MySql()
    {
        using var cfgProvider = BuildConfig<MySqlConnection, MySqlCommand>(
            () => new MySqlConnection(MySqlServerConnectionString), db =>
                new MySqlCommand(SqlQuery, db));
        
        AssertConfig(cfgProvider);
    }

    [Fact]
    public void DatabaseSettingsAreLoadable_Sqlite()
    {
#if NET
        const string sqliteConnectionString = "Data Source=:memory:;";
        var db = MakeSqlLiteDb(sqliteConnectionString);
        using var cfgProvider = BuildConfig<SqliteConnection, SqliteCommand>(
            () => db, db =>
                new SqliteCommand(SqlQuery, db));
        
        AssertConfig(cfgProvider);
#endif
    }

    private static SqliteConnection MakeSqlLiteDb(string sqliteConnectionString)
    {
        var db = new SqliteConnection(sqliteConnectionString);

        using (var queryCommand = new SqliteCommand(@"create table Settings
(
    Id           int primary key  NOT NULL,
    SettingKey   varchar(50),
    SettingValue varchar(50),
    IsActive     varchar(1)
);
insert into Settings (Id,SettingKey, SettingValue, IsActive)
values
    (1,'AppSettings:DbIntSetting','769283',1);", db))
        {
            db.Open();

            queryCommand.ExecuteNonQuery();
        }

        return db;
    }

    private static void AssertConfig(ConfigurationProvider cfgProvider)
    {
        cfgProvider.Load();

        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }

    private static SqlServerConfigurationProvider<TCconneciton> BuildConfig<TCconneciton, TCommand>(
        Func<TCconneciton> connection, Func<TCconneciton, DbCommand>? command) where TCconneciton : DbConnection =>
        new(new SqlServerConfigurationSource<TCconneciton>()
        {
            DbConnection = connection,
            CreateQueryDelegate = command
        });
}