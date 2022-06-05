# ErikNoren.Extensions.Configuration.SqlServer
This library is a Microsoft.Extensions.Configuration provider that reads settings from a database and makes those values
available via IConfiguration or in a strongly typed class created using Microsoft.Extensions.Options. The provider uses
the same Microsoft.Data.SqlClient package used by Microsoft.EntityFrameworkCore to reduce dependency bloat in the case
a project already has a dependency on EF Core.

This provider takes advantage of the changes to the way configuration values are constructed starting in 6.0.0. The new
startup allows for configuration values to be read out before all sources have been configured. This is very useful for
a provider that needs to retrieve configuration settings (like a connection string) in order to do its work.


## Usage Examples

### Minimum Required Parameters
```csharp
...
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSqlServer(config =>
{
    //If the connection string was defined in an appsettings file, environment variable, etc. it can be retrieved here:
    config.ConnectionString = builder.Configuration.GetConnectionString("DemoDatabase");
    config.CreateQueryDelegate = sqlConn => new ("SELECT SettingKey, SettingValue FROM dbo.Settings WHERE IsActive = 1", sqlConn);
});
...
```

### Refresh Values
```csharp
...
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSqlServer(config =>
{
    //If the connection string was defined in an appsettings file, environment variable, etc. it can be retrieved here:
    config.ConnectionString = builder.Configuration.GetConnectionString("DemoDatabase");
    config.CreateQueryDelegate = sqlConn => new ("SELECT SettingKey, SettingValue FROM dbo.Settings WHERE IsActive = 1", sqlConn);

    //Define an interval for the SqlServerConfigurationProvider to reconnect to the database and look for updated settings
    config.RefreshInterval = TimeSpan.FromMinutes(1);
});
...
```


## Database Setup
Since IConfiguration uses string keys and string values a Settings table is very easy to construct. The minimum the table needs
is only 2 columns: one for a setting Key and one for a setting Value. You can of course add additional columns for things like
an IsActive flag to enable the ability to turn settings on and off as needed.

The code samples use a table created with the following SQL:
```sql
CREATE TABLE [dbo].[Settings] (
    [SettingKey]   VARCHAR (255)  NOT NULL,
    [SettingValue] NVARCHAR (MAX) NULL,
    [IsActive]     BIT            NOT NULL
);

```
The IsActive column is not required if you do not plan to toggle settings between active and inactive states. I found this to be
useful to be able to disable a setting without deleting it in case I wanted to re-activate it without having to recreate the key
name which can be quite lengthy as explained below.

The column used for setting Keys should not allow nulls. All settings must have a non-null, non-empty string. Values can be null.
The type and length of the columns can be whatever length is long enough to accommodate the keys and values. Be sure to create the
key column with enough length to accommodate the flattened structure of setting keys. Meaning nested settings are flattened into a
colon-delimited string so depending on the length of your key names and how deeply they are nested you might end up with quite long
strings in your key column.

For example given the following JSON represenation of nested objects:
```json
{
    "AppSettings": {
        "IsEnabled": true,
        "EmailInfo": {
            "To": "person1@example.com",
            "From": "person2@example.com"
        }
    }
}
```

The flattened key-value pair representation would be:
```
"AppSettings:IsEnabled", "true"
"AppSettings:EmailInfo:To", "person1@example.com"
"AppSettings:EmailInfo:From", "person2@example.com"
```

The keys can get even more complicated when you have arrays of values.
```json
{
    "AppSettings": {
        "IsEnabled": true,
        "EmailInfo": [{
            "To": ["person1@example.com", "person3@example.com", "person4@example.com"],
            "From": "person2@example.com"
        }, {
            "To": ["person5@example.com"],
            "From": "person2@example.com"
        }]
    }
}
```
```
"AppSettings:IsEnabled", "true"
"AppSettings:EmailInfo:0:To:0", "person1@example.com"
"AppSettings:EmailInfo:0:To:1", "person3@example.com"
"AppSettings:EmailInfo:0:To:2", "person4@example.com"
"AppSettings:EmailInfo:0:From", "person2@example.com"
"AppSettings:EmailInfo:1:To:0", "person5@example.com"
"AppSettings:EmailInfo:1:From", "person2@example.com"
```

I was very surprised and happy to see how easy it was to create a new provider and integrate it into my projects.
Read more about [Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) and [Options](https://docs.microsoft.com/en-us/dotnet/core/extensions/options) on the Microsoft Docs site.


Erik Noren
[@ErikNoren](https://twitter.com/ErikNoren)

