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
}