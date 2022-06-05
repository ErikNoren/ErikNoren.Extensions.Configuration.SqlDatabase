using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public static class SqlServerConfigurationExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, Action<SqlServerConfigurationSource>? configurationSource)
        => builder.Add(configurationSource);
}
