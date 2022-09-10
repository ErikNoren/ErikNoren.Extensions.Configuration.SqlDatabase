using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace ErikNoren.Extensions.Configuration.SqlServer;

public class SqlServerConfigurationProvider<TDb> : ConfigurationProvider, IDisposable where TDb : DbConnection
{
    public SqlServerConfigurationSource<TDb> Source { get; }

    private readonly Timer? _refreshTimer = null;
    
    public SqlServerConfigurationProvider(SqlServerConfigurationSource<TDb> source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));

        if (Source.RefreshInterval.HasValue)
            _refreshTimer = new Timer(_ => ReadDatabaseSettings(true), null, Timeout.Infinite, Timeout.Infinite);
    }

    public override void Load()
    {

        ReadDatabaseSettings(false);

        if (_refreshTimer != null && Source.RefreshInterval.HasValue)
            _refreshTimer.Change(Source.RefreshInterval.Value, Source.RefreshInterval.Value);
    }

    private void ReadDatabaseSettings(bool isReload)
    {
        if ( Source.CreateQueryDelegate == null)
            return;

        try
        {
          

            var queryCommand = Source.CreateQueryDelegate(Source.DbConnection);

            if (queryCommand == null)
                return;

            using (queryCommand)
            {
                Source.DbConnection.Open();

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