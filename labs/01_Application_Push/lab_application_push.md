# Push the demo application

The prime directive of Pivotal Cloud Foundry is to host applications. We are going to exercise that directive by pushing a very simple .NET Core application.

By the end of this lab you should have your Articulate application up and running in your account.
--

Set your target environment

If you haven't already, download the latest release of the Cloud Foundry CLI from https://github.com/cloudfoundry/cli/releases for your operating system and install it.

Set the API target for the CLI:
```
$ cf api --skip-ssl-validation https://api.<DOMAIN_PROVIDED_BY_INSTRUCTOR>
```

Login to Pivotal Cloud Foundry:

```
$ cf login
```

Follow the prompts, entering in the student credentials provided by the instructor and choosing the development space.

Build and Push!

Change to the articulate application directory:
```
$ cd $COURSE_HOME
```

### Build

Use the .NET Core CLI to build and package the application:

```
$ dotnet publish
```

Publishing includes restoring the required .NET dependencies need to deploy the app on to the target environment. The default publish command will include third party libraries, but exclude the .NET runtime and Base Class Libraries (BCL) that are part of .NET Core SDK.

### Push

Now use the `cf push` command to push the application to PCF!

```
$ cf push
```

You should see output similar to the following listing. Take a look at the listing callouts for a play-by-play of what's happening:


```
Pushing from manifest to org Canada / space astakhov as astakhov@pivotal.io...
Using manifest file C:\Projects\pcf-ers-dotnetcore-workshop\manifest.yml <1>
Getting app info...
Creating app with these attributes... <2>
+ name:        articulate-ui
  path:        C:\Projects\pcf-ers-dotnetcore-workshop\src\Articulate.UI\bin\Debug\netcoreapp2.2\publish
+ instances:   1
+ memory:      512M
  routes:
+   articulate-ui-relaxed-panther.cfapps.io <3>

Creating app articulate-ui...
Mapping routes... <4>
Comparing local files to remote cache...
Packaging files to upload...
Uploading files...
 410.64 KiB / 410.64 KiB [================================] 100.00% 1s <5>

Waiting for API to complete processing files...

Staging app and tracing logs... <6>
   Downloading dotnet_core_buildpack_beta...
   Downloading dotnet_core_buildpack...
   Downloading nodejs_buildpack...
   Downloading php_buildpack...
   Downloading binary_buildpack...
   Downloaded php_buildpack
   Downloading staticfile_buildpack...
   Downloaded dotnet_core_buildpack
   Downloading java_buildpack...
   Downloaded java_buildpack
   Downloading ruby_buildpack...
   Downloaded binary_buildpack (9.1M)
   Downloading go_buildpack...
   Downloaded staticfile_buildpack (9.5M)
   Downloading python_buildpack...
   Downloaded dotnet_core_buildpack_beta (99.4M)
   Downloaded nodejs_buildpack (123.5M)
   Downloaded ruby_buildpack (345.8M)
   Downloaded go_buildpack (479M)
   Downloaded python_buildpack (563.7M)
   Cell 331ea532-5d72-4dcb-8332-a6d844ccd2cd creating container for instance 65b6cece-2770-4f94-80c8-25df4c9e3672
   Cell 331ea532-5d72-4dcb-8332-a6d844ccd2cd successfully created container for instance 65b6cece-2770-4f94-80c8-25df4c9e3672
   Downloading app package...
   Downloaded app package (1.8M)
   -----> Dotnet-Core Buildpack version 2.2.14
   -----> Supplying Dotnet Core
   -----> Installing libunwind 1.3.1
          Copy [/tmp/buildpacks/34ff34548d4f134f888d9721aa5848d9/dependencies/7cd276dbdcda7e2158dde0f8b48d611f/libunwind-1.3.1-cflinuxfs3-96d2f3d0.tar.gz]
          using the default SDK
   -----> Installing dotnet-sdk 2.2.401
          Copy [/tmp/buildpacks/34ff34548d4f134f888d9721aa5848d9/dependencies/4807e2ac46ef64f350aad26c8d94f30b/dotnet-sdk.2.2.401.linux-amd64-cflinuxfs3-f75ce2d9.tar.xz]
   -----> Installing dotnet-runtime 2.2.6 <7>
          Copy [/tmp/buildpacks/34ff34548d4f134f888d9721aa5848d9/dependencies/c260d0039b2ae9cdb10401a8a0d502bf/dotnet-runtime.2.2.6.linux-amd64-cflinuxfs3-2825ca3e.tar.xz]
   -----> Finalizing Dotnet Core
   -----> Installing dotnet-aspnetcore 2.2.6
          Copy [/tmp/buildpacks/34ff34548d4f134f888d9721aa5848d9/dependencies/5c594b103f816ed6cd7c91b648bd0095/dotnet-aspnetcore.2.2.6.linux-amd64-cflinuxfs3-2ad97587.tar.xz]
   -----> Installing dotnet-runtime 2.2.6
          Copy [/tmp/buildpacks/34ff34548d4f134f888d9721aa5848d9/dependencies/c260d0039b2ae9cdb10401a8a0d502bf/dotnet-runtime.2.2.6.linux-amd64-cflinuxfs3-2825ca3e.tar.xz]
   -----> Cleaning staging area
   Exit status 0
   Uploading droplet, build artifacts cache... <8>
   Uploading droplet...
   Uploading build artifacts cache...
   Uploaded build artifacts cache (221B)
   Uploaded droplet (177.2M)
   Uploading complete
   Cell 331ea532-5d72-4dcb-8332-a6d844ccd2cd stopping instance 65b6cece-2770-4f94-80c8-25df4c9e3672
   Cell 331ea532-5d72-4dcb-8332-a6d844ccd2cd destroying container for instance 65b6cece-2770-4f94-80c8-25df4c9e3672

Waiting for app to start...

name:              articulate-ui
requested state:   started
routes:            articulate-ui-relaxed-panther.cfapps.io
last uploaded:     Mon 16 Sep 15:01:48 EDT 2019
stack:             cflinuxfs3
buildpacks:        dotnet-core

type:            web
instances:       1/1
memory usage:    512M
start command:   cd ${HOME} && exec dotnet ./Articulate.dll --server.urls http://0.0.0.0:${PORT} <9>
     state     since                  cpu    memory      disk      details                                                          
#0   running   2019-09-16T19:02:12Z   0.0%   0 of 512M   0 of 1G <10>

```



1. The CLI is using a manifest to provide necessary configuration details such as application name, memory to be allocated, and path to the application artifact.
   Take a look at `manifest.yml` to see how.
2. In most cases, the CLI indicates each Cloud Foundry API call as it happens.
   In this case, the CLI has created an application record for _articulate-ui_ in your assigned space.
3. All HTTP/HTTPS requests to applications will flow through Cloud Foundry's front-end router called [(Go)Router](http://docs.cloudfoundry.org/concepts/architecture/router.html).
   Here the CLI is creating a route with random word tokens inserted (again, see `manifest.yml` for a hint!) to prevent route collisions across the default PCF domain.
4. Now the CLI is _binding_ the created route to the application.
   Routes can actually be bound to multiple applications to support techniques such as [blue-green deployments](http://www.mattstine.com/2013/07/10/blue-green-deployments-on-cloudfoundry).
5. The CLI finally uploads the application bits to PCF. Notice that it's uploading _90 files_! This is because Cloud Foundry actually explodes a ZIP artifact before uploading it for caching purposes.
6. Now we begin the staging process. The [.NET Core Buildpack](https://github.com/cloudfoundry/dotnet-core-buildpack) is responsible for assembling the runtime components necessary to run the application.
7. Here we see the version of the .NET Core Runtime that has been chosen and installed.
8. The complete package of your application and all of its necessary runtime components is called a _droplet_.
   Here the droplet is being uploaded to PCF's internal blobstore so that it can be easily copied to one or more [Diego Cells](http://docs.cloudfoundry.org/concepts/diego/diego-components.html#cell-components) for execution.
9. The CLI tells you exactly what command and argument set was used to start your application.
10. Finally the CLI reports the current status of your application's health.

You can get the same output at any time by typing `cf app articulate-ui`.
====

Visit the application in your browser by hitting the route that was generated by the CLI.  You can find the route by typing `cf apps`, and it will look something like `https://articulate-ui-naturopathic-souple.<DOMAIN-PROVIDED-BY-INSTRUCTOR>`

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\screenshot_main.png)

Take a look at the `Application Environment Information` section on the top right-hand corner of the UI.
This gives you important information about the state of the currently running articulate-ui_ instance, including what application instance index and what Cloud Foundry services are bound.
It will become important in the next lab!