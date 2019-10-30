### Connecting to existing services

While marketplace offers the best self-service experience for developers, many times you'll need to connect to an existing service such as database that is running outside the platform. While you are free to use the traditional .NET by putting connection string into appsettings.json, the service binding semantics offer a powerful abstraction for declaring external dependencies in your apps and having actual endpoint be tied to a space your app lives in. 

One of the features of the platform is the ability to define a **C**ustom **U**ser **P**rovided **Se**rvice (CUPS) from PCF marketplace and bind it to your app. CUPS can be though of as an arbitrary piece of JSON configuration data that you treat as a service instance inside your space and can use service binding semantics to attach it to your apps. This also lets you leverage SteelToe connectors to automatically configure service factories with connection information. 

Lets use CUPS to attach our UI directly to the database. A good rule of thumb is to get everything working locally before you try it on the platform. Lets do that now.

Edit appsettings.development.json and edit connection string for the database you're targeting. Change `repositoryProvider` setting to the database type you're working with: either `mysql` or `sqlserver`. 

```json
{
...
  "MySql": {
    "Client": {
      "SslMode": "none",
      "ConnectionString": "Server=localhost;Database=ers;Uid=root;Pwd=;sslmode=none;"
    }
  },
  "SqlServer": {
    "Credentials": {
      "ConnectionString": "Server=localhost;Database=Articulate;User id=sa;Password=P@ssword"
    }
  },
  "repositoryProvider": "sqlserver",
...
}
```

Run the app locally, and access services page. Confirm that the repository provider is your choice of database

![1569427466166](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\services-provider.png)

Notice that configuration is using SteelToe connector syntax for configuring services in local environment. These will be override by service binding when running on the platform.

Now let's configure this on PCF using CUPS. 

Create a CUPS service:

```bash
$ cf cups articulate-db -p url
url> sqlserver://username:password@sqlServerHostOrIp/databasename
```

Notice that connection string information is assigned to a key `url` and is delivered using Uri syntax instead of the usual SQL connection string. This is done on purpose to standardize endpoint formats. The prefix of the uri (scheme) gives the hint to SteelToe connector as to the type of service you're attaching. Depending on the database you're using the scheme will be different (`sqlserver` or `mysql`) *Note that if you have special characters in username or password they need to be URL encoded. You can use a site like https://meyerweb.com/eric/tools/dencoder/ to do it*

Bind the service to your app and restart it to make it pick up the new environmental variables

```shell
$ cf bind-service articulate-ui articulate-db
$ cf restart articulate-ui
```

*The internal logic of the app automatically switches from memory to database provider based on the service binding if `repositoryProvider` is not set (see `AttendeeClientServiceExtensions.GetProvider` if interested in implementation details). We can force it to use a different one even if there's a binding by setting environmental variable as following:* `$ cf set-env attendees-ui repositoryProvider memory`