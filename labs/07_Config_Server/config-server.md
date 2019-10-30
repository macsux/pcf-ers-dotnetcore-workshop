# Config Server

This is where we examine how configuration management works in .NET Core. Configuration values can come from multiple sources (aka *Configuration Providers*). They are all registered inside a single configuration object, which provides a single consolidated view of all keys and values in the underlying providers.

When the same configuration setting appears in multiple providers, the provider that is registered last will be the one providing the final value. Built in providers include JSON, Environmental Variables, and console arguments.

With many applications and environments, configuration management adds significant operational overhead. In these cases, you can use a Spring Cloud Config Server to consolidate all configuration settings in one place. Config Server acts as a configuration service API that plugs as just another Configuration Provider into standard .NET configuration. Config Server itself reads it config values either out of one of the support backing stores, such as Git or Hashicorp Vault. This allows configuration across multiple apps and environments to be consolidated into a single location, and provides version control and auditing. Config server also support encryption for serving sensitive values such as database connection strings.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\config-server.png)



Config server is a Java Spring Boot application, which you can start locally. You can also provision it from PCF marketplace, assuming PCF is configured with Spring Cloud Services tile. As part of config server configuration, you need to point it to where your configuration data is stored. When running locally we'll use a local folder with config files - when on PCF, we'll use a Git repo url. Config server exposes underlying configuration via a simple REST interface and returns values as JSON. The appropriate client side libraries exist for both .NET via Steeltoe package and for Java Spring to automatically connect and map configuration data into native configuration management subsystems. 

### Starting config server locally

Browse to `config-server` folder

Start config server from command line

   ```
   .\mvnw spring-boot:run
   ```

This starts config server on http://localhost:8888. The config server provided is already configured to use a local `/config` folder as the source for configuration data. If you want to change the underlying config store, you can do so from configuration file in `\config-server\src\main\resources\application.yml`. Go ahead and take a look at the file now

### Understanding config structure
The name of the configuration files determine the scope of values included in query responses. Configuration data is stored either in `.yml` or `.properties` formats (JSON is not supported out of the box). Config server allows you to layer your configuration data into a layered cake, from least specific to most specific. More specific values override configuration found in generic files. 


application.yml - includes values applicable to all applications, and will be included in every response
<appname>.yml - values for specific app name 
<appname>-<profile>.yml - values for specific app name in specific profile (aka environment)

You can also create folders to further subdivide config into "labels". When using Git, tags, branches and SHA codes can be used in labels.

### Accessing values

To query config server, use the following URL pattern:



http://localhost:8888/appname/profile/<label>
App name and profile are mandatory, where's label is an optional segment. 

_We recommend you use PostMan or Chrome with JSONView extension to get proper formatting of results_

1. Access http://localhost:8888/articulate-ui/default to get default values from application.yml and articulate-ui.yml
2. Access http://localhost:8888/articulate-ui/production to get production specific overrides 
3. Access http://localhost:8888/articulate-ui/production/rest to include additional label config values

### Security
The default config server is not secured when running locally, which is why we can hit it with curl. When running on PCF, it is automatically secured by the UAA (PCF integrated OpenID server), and requires a valid token to retrieve values. 

### Encryption

You can set a key on the config server which will be used to encrypt and decrypt sensitive values. This allows you to encrypt sensitive information by hitting an endpoint, copy the encrypted value and put it into your config file. As long as the config server is configured with the key, it will be able to decode values automatically when queried.

The encryption key is set in `config-server\src\main\resources\bootstrap.yml` file.

Use Postman for next exercise

1. Send a POST request to  http://localhost:8888/encrypt with body set to value you want to encrypt (use raw content type)

   ![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\config-server-encrypt.png)

2. Inside `config` folder, create a new file called "myapp.yaml`. Put an encrypted value configuration value as following:

```
SecretValue: "{cipher}ca8c7cab17ba35ebbbf63059afd4bd0e451d2e1d477b83e928491c88a1b6c2e5"
```
3. Retrieve the values for `myapp`: http://localhost:8888/myapp/default. Notice that the value is already decrypted

```json
{
    "name": "myapp",
    "profiles": [
        "default"
    ],
    "label": null,
    "version": null,
    "state": null,
    "propertySources": [
        {
            "name": "file:../config/myapp.yaml",
            "source": {
                "SecretValue": "secret string"
            }
        },
        {
            "name": "file:../config/application.yaml",
            "source": {
                "ErrorMessage": "These are not the errors you're looking for",
                "Logging.IncludeScopes": false,
                "Logging.LogLevel.Default": "Warning",
                "Logging.LogLevel.Articulate": "Information",
                "Logging.LogLevel.Steeltoe.CloudFoundry.Connector.EFCore": "Information"
            }
        }
    ]
}
```



### How to add to code

1. Add `Steeltoe.Extensions.Configuration.ConfigServerCore` package

2. Modify WebHostBuilder in `Program.cs` with `.AddConfigServer()`

   ```c#
   public static IWebHost BuildWebHost(string[] args) =>
       WebHost.CreateDefaultBuilder(args)
       .AddConfigServer()
       .UseStartup<Startup>()
       .Build();
   ```

### Config server on PCF

When working on PCF, you can provision config server as a service from the marketplace. As part of the creation command, you need to give it a config file pointing to your config repo.

1. Access the app you've deployed to PCF at the /config endpoint
	
   Examine the configuration data driving the colors of circles
   
2. Create a file called `gitconfig.json` with the following content

  ```json
   {
       "git" : { 
           "uri": "https://github.com/macsux/pcf-ers-dotnetcore-workshop.git",
           "searchPaths": "config"
       },
       "encrypt": {
           "key": "theforce"
       }
   }
  ```
  You can find full list of configuration values in the docs here: https://docs.pivotal.io/spring-cloud-services/1-5/common/config-server/configuring-with-git.html

3. Create config server in your space:

   ```
   cf create-service p-config-server standard myconfig -c gitconfig.json
   ```

4. Bind it to your app

   ```
   cf bind-service articulate-ui myconfig
   ```

5. Restart the app

   ```
   cf restart articulate-ui
   ```
6. Refresh the config page of the app. Notice that the extra circle color is now coming from config server
   