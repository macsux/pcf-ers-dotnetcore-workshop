# Backend Buildout

Currently our UI application talks directly to the database. In this lab we'll build a out a REST API backend that will talk to database and implement a client side service in UI that will talk to it. *Instructions use .NET Core CLI to do many project manipulation tasks, but you are welcome to use your IDE of choice to do the same*

Create a new project based on `webapi` template and add it to solution

```shell
$ dotnet new webapi -o src/Articulate.Backend -n Articulate.Backend
$ dotnet sln add src/Articulate.Backend
$ cd src/Articulate.Backend
```

We're going to assume we're working with existing database using Entity Framework. Let's add the appropriate db provider into our projects.

```shell
$ dotnet add package Pomelo.EntityFrameworkCore.MySql -v 2.2.0
```

OR

```shell
$ dotnet add package Microsoft.EntityFrameworkCore.SqlServer -v 2.2.0
```

*Technically Entity Framework & SqlServer is included as part of `Microsoft.AspNetCore.App` on which `webapi` template is based when using .NET Core 2.x , but this will change with .NET Core 3.0, so it's best to be explicit going forward. *

Let's *scaffold* it into a Entity Framework Core DbContext. Change the provider name if using MySql.

```shell
$ dotnet ef dbcontext scaffold "Server=localhost;Database=Articulate;User id=sa;Password=P@ssword" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context AttendeeContext 
```

This has now reverse-engineered our DB schema into a DbContext and associated table models and placed it into Models subfolder in our project. 

Now let's import SteelToe connectors to simplify connection string management and ability to process CUPS service binding when on the platform. Add dependency to as following:

```shell
$ dotnet add package `Steeltoe.CloudFoundry.Connector.EFCore`
```

Let's use Steeltoe connectors to wire up our context with connection string information. When running locally this will come from appsettings.Development.json. When on PCF, connection string will come from CUPS binding. 

Edit `Startup.cs` and register our EF context into the service container:

```c#
services.AddDbContext<AttendeeContext>(cfg => cfg.UseSqlServer(Configuration));
// or
services.AddDbContext<AttendeeContext>(cfg => cfg.UseMySql(Configuration));
```

**Pro Tip**: When not using EF, such as with Dapper, you can register connection object injection (ex MySqlConnection) using syntax such as `services.AddMySqlConnection(Configuration)`. These extension methods are found in  `Steeltoe.Cloudfoundry.Connector` NuGet package.

Since we want to test out everything locally before putting it on the platform, let's configure our `appsettings.development.json` to include our connection string. But before we do, let's ensure that the config settings we're using are correct. Typos and configuration mismatch are top reasons things don't work. Luckily SteelToe has a JSON schema file you can import, which most modern IDEs will use to provide IntelliSense. Add it as the first line in your settings file as below, and the relevant configuration settings:

```json

  "$schema": "https://raw.githubusercontent.com/steeltoeoss-incubator/steeltoe-schema/master/schema.json",
  
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
  }
}
```

The last step is to wire up mapping of VCAP environmental variables into our project's configuration. This is done by registering it as a configuration provider. Edit your `Program.cs` and modify it as following

```c#
WebHost.CreateDefaultBuilder(args)
	.AddCloudFoundry()

...
```

We're now done with configuration - let's implement our business logic. Create a new controller called `AttendeesController` and implement the following methods:

```c#
Task<List<Attendees>> GetAll(); // GET on /api/attendees/
Task Add(Attendees); // PUT on /api/attendees
Task DeleteAll(); // DELETE on /api/attendees/all
```

### Add Swagger / OpenAPI

After you've finished implementing the API, let's make it easy to discover and consume. Back in the day of WCF and SOAP we relied on WSDL endpoints to describe what methods are available on our service endpoint and the data structures used. REST uses OpenAPI (aka Swagger) to describe the structure of our API in a JSON format. 

When using ASP.NET Core, Swagger middleware can automatically generate the description of our services based on the routes and controller methods. As a bonus, it comes with a helper UI page to help explore the API and test invoke endpoints. Let's add it now.

```shell
$ dotnet add package Swashbuckle.AspNetCore -v 5.0.0-rc2
```

in Startup.cs add the following 

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Articulate API", Version = "v1"}));
    ...
}
...
public void Configure(IApplicationBuilder app
{  
	...
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Articulate API V1"));
    ...
}
```

Edit Properties > launchSettings.json and set `launchUrl` to `swagger` so we get the nice Swagger GUI when running this locally. Go ahead and launch it. Play around with Swagger interface if this is the first time seeing it.

### Connect front end to backend

Let's now implement the client side of things and connect UI to the backend we just wrote. The Articulate.UI project has a stub class called ApiAttendeeClient. Let's implement this class to connect it to our REST endpoint. 

We can use the OpenAPI specification file produced by our backend to generate client side proxy class automatically using `NSwag` tool. 

Download the latest NSwag tooling chain from GitHub releases page: https://github.com/RicoSuter/NSwag/releases (grab the nswag.zip). 

Extract to some folder on your drive, like `c:\tools\nswag`

Ensure that your backend is running and is configured to port 5010 (adjust your launchsettings file if necessary).

Now use it to generate the client proxy class for our UI project. If you've worked with WCF, this is very similar to adding a service reference.

```shell
$ dotnet "C:\tools\nswag\netcore22\dotnet-nswag.dll" openapi2csclient /output:src/Articulate.UI/Services/ApiAttendeeClient.generated.cs /input:http://localhost:5010/swagger/v1/swagger.json /namespace:Articulate.Services /classname:ApiAttendeeClient /UseBaseUrl:false /GenerateDtoTypes:false
```

We now have a fully functional typed client to interact with our backend. If our backend API ever changes, we just need to rerun the above command to regenerate the client to match. *Note that because UI already had a class that represented `Attendee`  object we exclude generation of DTOs as part of above command - you normally would want those generated along with the client*

Note that we already had a class called called ApiAttendeeClient. We're using a C# `partial` keyword to split the definition of the class across two physical files, one being generated by our tool, and the other adding our custom code. 

Modify the `ApiAttendeeClient` user class to leverage the generated method so it conforms to `IAttendeeClient` interface.

Hints:

* Set IsMigrated to true for simplicity 
* You can get endpoint from httpClient.BaseUrl
* The generated method names based on the contract may not be very intuitive - consider this when designing your own APIs

#### Working with HttpClientFactory

As you might have noticed, our current implementation takes an HttpClient in the constructor. This allows us to inject an HttpClient that is already preconfigured to work with this proxy wrapper, including the BaseUrl, any authentication, default headers, etc. .NET Core includes a feature of mapping lifecycle of HttpClient and mapping it on to implementation clients such as the one we've just built. Let's see how this works.

Edit `AttendeeClientServiceExtensions.AddAtteendeeClient` method. Let's configure our new implementation for HttpClient injection such as the following:

```c#
services.AddHttpClient<ApiAttendeeClient>(http => http.BaseAddress = new Uri(configuration.GetValue<string>("backendUrl")));
```

Add `backendUrl` to our `appsettings.development.json` to point to our backend service (`http://localhost:5010`) 

Switch out `repositoryProvider` to `rest`.

Try to launch both backend and front end - both should now work!

### Publishing solution to PCF

Our project now consists of two deployable artifacts: frontend and backend. Now let's publish it to Cloud Foundry. 

Modify the `manifest.yml` to include both apps - right now it only has the UI. You can do this by copying the section that is already there (starting at `-name:`) , and adjusting the right parameters to include the second app.

- Use `attendees-service` for new app name

- Bind to our DB service as part of `push` by listing it under services:


   ``` yaml
   services: 
     - attendee-db
   ```

- repoint UI to point to backend by overriding `backendUrl` by setting environmental variable in the manifest

  ```yaml
  env:
    backendUrl: https://articulate-service-insightful-hyrax.apps.pcfone.io
  repositoryProvider: rest
  ```
  
  

You can also exclude bindings from manifest and set service bindings and environmental variables via `cf bind-service APPNAME SERVICE_NAME` and `cf set-env APPNAME VAR_NAME VALUE`

Let's publish both apps to the platform:

```shell
$ dotnet publish
$ cf push
```

**Pro tip**: even if you have more then one app defined in manifest, you can publish a specific one by specifying its name as defined in the manifest as an argument for push. ex `cf push articulate-service`.

### Improving DevOps with Telemetry

At this point you should have the basics down on how to publish and connect apps on PCF and consume services. Let's focus on getting some extended telemetry into our apps to help with DevOps.

#### Add actuators

Add reference to `Steeltoe.Management.CloudFoundryCore` and activate it in Startup.cs by adding `services.AddCloudFoundryActuators(Configuration);` and `app.UseCloudFoundryActuators();`

and activate Dynamic Log provider in your Program.cs WebHostBuilder by adding:

``` c#
.ConfigureLogging((builderContext, loggingBuilder) =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddDynamicConsole();
})
```
This will allow you to change log levels at runtime without restarting the app using Pivotal Apps Manager from the logging tab!
#### Add SQL server health check
Reference `Steeltoe.CloudFoundry.ConnectorCore` 

Add `services.AddSqlServerConnection(Configuration);` to your Startup file. 
This will configure SqlConnection for injection, but also register a health check against SQL Server, which is now part of our actuator /health endpoint. 
Try launching your backend after doing the above and access the actuator at /actuator/health. 

#### Add GitInfo versioning

This will allow app to be stamped with Git SHA code from which it was built, and exposed through /info actuator. This info will be exposed in Pivotal Apps Manager

Add `GitInfo` package

Edit your `csproj` file and add the following

```xml
  <ItemGroup>
    <None Include="git.properties">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
  <Target Name="_GitProperties" AfterTargets="CoreCompile">
    <WriteLinesToFile File="git.properties" Lines="git.remote.origin.url=$(GitRoot)" Overwrite="true" />
    <WriteLinesToFile File="git.properties" Lines="git.build.version=$(GitBaseVersion)" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.commit.id.abbrev=$(GitCommit)" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.commit.id=$(GitSha)" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.tags=$(GitTag)" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.branch=$(GitBranch)" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.build.time=$([System.DateTime]::Now.ToString('O'))" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.build.user.name=$([System.Environment]::GetEnvironmentVariable('USERNAME'))" Overwrite="false" />
    <WriteLinesToFile File="git.properties" Lines="git.build.host=$([System.Environment]::GetEnvironmentVariable('COMPUTERNAME'))" Overwrite="false" />
  </Target>
```

Try building the app and accessing `/actuator/info` endpoint.

#### Add distributed tracing

Often we need to diagnose a single request that stretches multiple apps. In our app, the user access UI application, which in turn calls backend app. Viewing logs stretching multiple apps in the context of a single logical request helps diagnosing many problems. PCF correlates request and logs by injecting a `x_b3_traceid` header into the first request on the platform. If the application propagates this header to downstream calls, PCF is able to establish request correlation. Steeltoe makes this easy by instrumenting HttpClient for automatic header propagation. 

Add `Steeltoe.Management.TracingCore` package

Activate distributed tracing in Startup class: `services.AddDistributedTracing(Configuration);`



### Putting it all together

Publish code and push to PCF

```
$ dotnet publish
$ cf push
```

Notice the affects of actuators, including health check

Try distributed tracing. 
Access the services page of the UI. 
From Pivotal Apps Manager, click PCF metrics link on the apps details page.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\metrics-link.png)

Search for call to `/services` and click distributed tracing link

![1569942600467](C:\Users\astakhov\AppData\Roaming\Typora\typora-user-images\1569942600467.png)

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\metrics-distributed-tracing.png)

