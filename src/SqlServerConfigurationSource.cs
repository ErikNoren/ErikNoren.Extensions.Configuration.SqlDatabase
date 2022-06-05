using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public class SqlServerConfigurationSource : IConfigurationSource
{
    public string? ConnectionString { get; set; }

    public TimeSpan? RefreshInterval { get; set; }

    public Func<SqlConnection, SqlCommand>? CreateQueryDelegate { get; set; }

    public Func<SqlDataReader, KeyValuePair<string, string?>> GetSettingFromReaderDelegate { get; set; } = DefaultGetSettingFromReaderDelegate;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new SqlServerConfigurationProvider(this);


    //The default implementation requires the setting key and value to be the first and second fields in the reader, respectively.
    //If the index of these columns will be different, set a custom delegate in the GetSettingFromReaderDelegate property.
    //The provider will ensure the returned KeyValuePair does not contain a null or whitespace Key.
    private static KeyValuePair<string, string?> DefaultGetSettingFromReaderDelegate(SqlDataReader sqlDataReader)
    {
        string  settingName  = string.Empty;
        string? settingValue = null;

        if (!sqlDataReader.IsDBNull(0))
            settingName = sqlDataReader.GetString(0);

        if (!sqlDataReader.IsDBNull(1))
            settingValue = sqlDataReader.GetString(1);
        
        return new KeyValuePair<string, string?>(settingName, settingValue);
    }
}
