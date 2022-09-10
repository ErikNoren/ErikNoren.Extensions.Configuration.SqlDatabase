using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public static class SqlServerConfigurationExtensions
{
    public static IConfigurationBuilder AddSqlServerType<T>(this IConfigurationBuilder builder, Action<SqlServerConfigurationSource<T>>? configurationSource) where T : DbConnection =>
        builder.Add(configurationSource);
}
