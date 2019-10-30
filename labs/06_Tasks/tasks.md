# Tasks

Most applications are intended to be long running, as their usage model is to service user's requests or respond to events from other systems. Some types of workloads are more representative of tasks - one off activities, or those run on a schedule. You can execute a task in PCF by telling it to execute a command against an image of an existing app on the platform. The platform will then spin up a dedicated container image, execute the command, and shut down.  The logs for the task will be part of the application log stream, but will be prefixed with task name. Given this it becomes obvious that bundling tasks with your app makes a lot of sense.

SteelToe library has the ability to embed tasks into your applications that you can invoke from the command line. Let's look at what's involved in creating a task.

1. Import NuGet package `Steeltoe.Management.TaskCore`. 

2. Create task implementation by implementing `Steeltoe.Common.Tasks.IApplicationTask` interface:

    ```c#
      public interface IApplicationTask
      {
        string Name { get; }
        void Run();
      }
    ```

    Each implementation must have a unique name, which is how they will be invoked from command line. 

3. Register into service container using `services.AddTask<MyTaskImplementation>`. 

4. Finally, change your `Program.cs` to use a new extension method for the `IWebHost` as following: `BuildWebHost(args).RunWithTasks();`

You can now invoke task from CLI as following:

```shell
dotnet MyApplication.dll --runtask TaskName
```

Lets take a look at a common example of a task: applying schema migrations to your database. Steeltoe includes a task to migrate schema for you called `MigrateDbContextTask` - you can see example of this in Articulate-UI app startup file. This allows you to apply Entity Framework migrations bundled with your app by executing the app as a task.

### Migrations background

Database migrations involve a strategy of doing controlled, safe, and reproduceable way to manage database schemas. At it's core it is a series of SQL instructions grouped into batches. Each batch of such SQL instructions is called a migration, and is usually a result of a change in schema as part of a release. Migrations are applied in consistent order, with each migration building on the results of previous. Ideally your first migration should start with the assumption that the database is empty. A special tracking table is created in the database to keep track of which migrations have been applied, essentially creating a way to version the database. 

There are many migration systems, including FlywayDb, LiquidBase, DbUp, RedGate, EntityFramework migrations and others. Some use raw SQL, such as FlywayDB. Others allow defining migrations using DSL such as liquidbase. One of the easiest to work with is EntityFramework migrations, and it's key advantages include:

- generating migrations automatically as structure of DbContext changes
- migrations represented as C# code
- ability to apply migration both from CLI or application 



### Trying things locally

Change `appsettings.development.json` to use SQLite memory provider:

```json
{
...
	"repositoryProvider": "memory",
...
}
```

Now lets try to run migrations

```bash
$ cd /src/Articulate.UI
$ dotnet run --runtask migrate
```

Bonus:

Edit `appsettings.json` and change `Logging:LogLevel:Default` to `Information`. Rerun above command to view the underlying SQL commands that are executed as part of the migration.

### Running tasks on PCF

If you haven't already, push the app to PCF. From Git root folder:

```shell
$ dotnet publish
$ cf push
```

Wait until the app is deployed. Open a second terminal window and tail the logs

```shell
$ cf logs articulate-ui
```

Now let's execute migration task. 

```shell
$ cf run-task articulate-ui "exec dotnet ./Articulate.dll --runtask migrate" --name MyMigrationTask
```

Observe the log output in the second window. 

*Note that unless you've configured your app to use real database, it will default to SQLite memory mode. Migrations applied by the task will not affect the main app as it runs a separate container, so results of changes will be local to memory of the task container and will be lost when it completes.*