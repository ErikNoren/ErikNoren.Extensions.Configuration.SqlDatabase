namespace ErikNoren.Extensions.Configuration.SqlServer.Tests;

public class SqlServerReadTests
{
    string ConnectionString => $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={Environment.CurrentDirectory}\data\DemoDatabase.mdf;Integrated Security=True";

    [Fact]
    public void DatabaseSettingsAreLoadable()
    {
        var configurationSource = new SqlServerConfigurationSource
        {
            ConnectionString = ConnectionString,
            CreateQueryDelegate = sqlConn => new("SELECT SettingKey, SettingValue FROM dbo.Settings WHERE IsActive = 1", sqlConn)
        };

        using var cfgProvider = new SqlServerConfigurationProvider(configurationSource);

        cfgProvider.Load();
        
        Assert.True(cfgProvider.TryGet("AppSettings:DbIntSetting", out var value));
        Assert.Equal("769283", value);

        Assert.False(cfgProvider.TryGet("AppSettings:InactiveSetting", out _));
    }
}