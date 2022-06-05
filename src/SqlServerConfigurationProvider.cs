using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider, IDisposable
{
    public SqlServerConfigurationSource Source { get; }

    private readonly Timer? _refreshTimer = null;

    public SqlServerConfigurationProvider(SqlServerConfigurationSource source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        Source = source;

        if (Source.RefreshInterval.HasValue)
            _refreshTimer = new Timer(_ => ReadDatabaseSettings(true), null, Timeout.Infinite, Timeout.Infinite);
    }

    public override void Load()
    {
        if (string.IsNullOrWhiteSpace(Source.ConnectionString))
            return;

        ReadDatabaseSettings(false);

        if (_refreshTimer != null && Source.RefreshInterval.HasValue)
            _refreshTimer.Change(Source.RefreshInterval.Value, Source.RefreshInterval.Value);
    }

    private void ReadDatabaseSettings(bool isReload)
    {
        if (string.IsNullOrWhiteSpace(Source.ConnectionString) || Source.CreateQueryDelegate == null)
            return;

        try
        {
            using var sqlConnection = new SqlConnection(Source.ConnectionString);

            var queryCommand = Source.CreateQueryDelegate(sqlConnection);

            if (queryCommand == null)
                return;

            using (queryCommand)
            {
                sqlConnection.Open();

                using var reader = queryCommand.ExecuteReader();

                var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

                while (reader.Read())
                {
                    try
                    {
                        var setting = Source.GetSettingFromReaderDelegate(reader);

                        //Configuration keys must contain a value
                        if (!string.IsNullOrWhiteSpace(setting.Key))
                            settings[setting.Key] = setting.Value;
                    }
                    catch (Exception readerEx)
                    {
                        System.Diagnostics.Debug.WriteLine(readerEx);
                    }
                }
                
                reader.Close();

                if (!isReload || !SettingsMatch(Data, settings))
                {
                    Data = settings;

                    if (isReload)
                        OnReload();
                }
            }
        }
        catch (Exception sqlEx)
        {
            System.Diagnostics.Debug.WriteLine(sqlEx);
        }
    }

    private bool SettingsMatch(IDictionary<string, string?> oldSettings, IDictionary<string, string?> newSettings)
    {
        if (oldSettings.Count != newSettings.Count)
            return false;

        return oldSettings
            .OrderBy(s => s.Key)
            .SequenceEqual(newSettings.OrderBy(s => s.Key));
    }

    public void Dispose()
    {
        _refreshTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _refreshTimer?.Dispose();
    }
}
