using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace ErikNoren.Extensions.Configuration.SqlServer.Tests;

public class SqlServerReadTests
{
    string SqlServerConnectionString => @"Server=localhost;Database=Config;User Id=sa;Password=A1234567a;";
    string MySqlServerConnectionString => @"Server=localhost;Database=Config;Uid=root;Pwd=A1234567a;";

    [Fact]
    public void DatabaseSettingsAreLoadable_SqlServer()
    {
        var sql = new SqlConnection(SqlServerConnectionString);
        using var cfgProvider =
            new SqlServerConfigurationProvider<SqlConnection>(new SqlServerConfigurationSource<SqlConnection>()
            {
                DbConnection = sql,
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
        var sql = new MySqlConnection(MySqlServerConnectionString);
        using var cfgProvider =
            new SqlServerConfigurationProvider<MySqlConnection>(new SqlServerConfigurationSource<MySqlConnection>()
            {
                DbConnection = sql,
                CreateQueryDelegate = db =>
                    new MySqlCommand("SELECT SettingKey, SettingValue FROM Settings WHERE IsActive = 1", db)
            });

        cfgProvider.Load();

        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }
}