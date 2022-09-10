using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace ErikNoren.Extensions.Configuration.SqlServer.Tests;

public class SqlServerReadTests
{
    string SqlServerConnectionString => @"Server=localhost;Database=Config;User Id=sa;Password=A1234567a;";
    string MySqlServerConnectionString => @"Server=localhost;Database=Config;Uid=root;Pwd=A1234567a;";

    [Fact]
    public void DatabaseSettingsAreLoadable_SqlServer()
    {
        using var cfgProvider =
            new SqlServerConfigurationProvider<SqlConnection>(new SqlServerConfigurationSource<SqlConnection>()
            {
                DbConnection = () => new SqlConnection(SqlServerConnectionString),
                CreateQueryDelegate = db =>
                    new SqlCommand("SELECT SettingKey, SettingValue FROM dbo.Settings WHERE IsActive = 1", db)
            });

        cfgProvider.Load();

        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }


    [Fact]
    public void DatabaseSettingsAreLoadable_MySql()
    {
        using var cfgProvider =
            new SqlServerConfigurationProvider<MySqlConnection>(new SqlServerConfigurationSource<MySqlConnection>()
            {
                DbConnection = () => new MySqlConnection(MySqlServerConnectionString),
                CreateQueryDelegate = db =>
                    new MySqlCommand("SELECT SettingKey, SettingValue FROM Settings WHERE IsActive = 1", db)
            });

        cfgProvider.Load();

        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }

    [Fact]
    public void DatabaseSettingsAreLoadable_Sqlite()
    {
        const string sqliteConnectionString = "Data Source=:memory:;";
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

        using var cfgProvider =
            new SqlServerConfigurationProvider<SqliteConnection>(new SqlServerConfigurationSource<SqliteConnection>()
            {
                DbConnection = () => db,
                CreateQueryDelegate = db =>
                    new SqliteCommand("SELECT SettingKey, SettingValue FROM Settings WHERE IsActive = 1", db)
            });

        cfgProvider.Load();

        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }
}