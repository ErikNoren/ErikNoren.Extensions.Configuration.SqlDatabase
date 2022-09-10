using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public class SqlServerConfigurationSource<TDb> : IConfigurationSource where TDb: DbConnection
{

    public Func<TDb> DbConnection { get; set; }

    public TimeSpan? RefreshInterval { get; set; }

    public Func<TDb, DbCommand>? CreateQueryDelegate { get; set; }

    public Func<IDataReader, KeyValuePair<string, string?>> GetSettingFromReaderDelegate { get; set; } =
        DefaultGetSettingFromReaderDelegate;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new SqlServerConfigurationProvider<TDb>(this);


    //The default implementation requires the setting key and value to be the first and second fields in the reader, respectively.
    //If the index of these columns will be different, set a custom delegate in the GetSettingFromReaderDelegate property.
    //The provider will ensure the returned KeyValuePair does not contain a null or whitespace Key.
    private static KeyValuePair<string, string?> DefaultGetSettingFromReaderDelegate(IDataReader sqlDataReader)
    {
        string settingName = string.Empty;
        string? settingValue = null;

        if (!sqlDataReader.IsDBNull(0))
            settingName = sqlDataReader.GetString(0);

        if (!sqlDataReader.IsDBNull(1))
            settingValue = sqlDataReader.GetString(1);

        return new KeyValuePair<string, string?>(settingName, settingValue);
    }
}