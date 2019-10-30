# Logging, Scale and HA

How PCF facilitates application management, including: access application 
logs, scale an application, access events and handle failed application instances

### Access Application Logs

Review the documentation on [application logs](http://docs.pivotal.io/pivotalcf/1-11/devguide/deploy-apps/streaming-logs.html).

You can tail the logs:
```
$ cf logs articulate-ui
```

Then open a browser and view the _articulate-ui_ application. 

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\screenshot_main.png)

Observe the log output when the _articulate-ui_ web page is refreshed. 
More logs are added!

To stop tailing logs, go to the terminal tailing the logs and send an
interrupt (Control + c).

You can also access the recent logs using 
```
$ cf logs articulate-ui --recent
```

### Questions
* Where should your application write logs?
* What are some of the different origin codes seen in the log?
* How does this change how you access logs today? At scale?

### Access _articulate_ events
Events for the application can also be used to compliment the logs in determining what has occurred with an application.
```
$ cf events articulate-ui
```

### Scale _articulate_
#### Scale Up
Start tailing the logs again.
[mac, linux]
```
$ cf logs articulate-ui | grep "API\|CELL"
```
[windows]

```
$ cf logs articulate-ui | findstr "API CELL"
```

The above statement filters only matching log lines from the Cloud Controller 
and Cell components.

In another terminal window scale articulate-ui. 
```
$ cf scale articulate-ui -m 1G
```
Observe log output.
Stop tailing the logs.
Scale articulate-ui back to our original settings.

```
$ cf scale articulate-ui -m 768M
```

### Scale Out
Browse to the _Scale and HA_ page on the _articulate_ application.
![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\screenshot_scaleAndHA.png)
Review the _Application Environment Information_. 
Press the `Start Load Test` button. Notice that all requests are landing on the first instance of the application.
In another terminal windows, scale _articulate-ui_ application.

```
$ cf scale articulate-ui -i 2
```

Return to `articulate-ui` in a web browser. Press the `Refresh` button several times. Observe the `Addresses` 
and `Instance Index` changing.

_Notice how quickly the new application instances are provisioned and subsequently load balanced!_

#### Questions

* How long does it take to scale up or out applications now?


### High Availability
Pivotal Cloud Foundry has [4 levels of HA](https://content.pivotal.io/blog/the-four-levels-of-ha-in-pivotal-cf) (High Availability) that keep your applications and the underlying platform running. In this section, we will demonstrate one of them. Failed application instances will be recovered.

PCF automatically monitors container health and restarts failed containers. There are 3 types of built-in health checks

- process - process spawned by container startup command is running
- port - application is listening on the required port as set by the container (PORT env var)
- http - app is responding with HTTP 200 at a preset url (default /). This endpoint is periodically checked by the platform


At this time you should be running multiple instances of `articulate-ui`. Confirm this with the following command:

```
$ cf app articulate-ui
```
Return to `articulate-ui` in a web browser and navigate to the Scale and HA page. Press the Refresh button. Confirm the application is running.
Kill the app. Press the Kill button!
Check the state of the app through the cf CLI.

```
$ cf app articulate-ui
```
Sample output below (notice the requested state vs actual state). In this case, Pivotal Cloud Foundry had already detected the failure and is starting a new instance.

```
Showing health and status for app articulate-ui in org Canada / space astakhov as astakhov@pivotal.io...

name:             articulates-ui
requested state:   started
routes:          articulatees-ui-relaxed-panther.cfapps.io
last uploaded:     Mon 16 Sep 15:01:48 EDT 2019
stack:             cflinuxfs3
buildpacks:        dotnet-core

type:           web
instances:      1/2
memory usage:   512M
     state      since                  cpu    memory          disk           details
#0   starting   2019-09-16T19:18:32Z   1.7%   8.1M of 512M    395.7M of 1G
#1   running    2019-09-16T19:16:16Z   1.0%   37.2M of 512M   395.7M of 1G
```

Repeat this command as necessary until `state = running`.
In your browser, Refresh the articulate-ui application.
The app is back up!

A new, healthy app instance has been automatically provisioned to replace the failing one.

View which instance was killed.
```
$ cf events articulate-ui
```
Scale articulate-ui back to our original settings.
```
$ cf scale articulate-ui -i 1
```
#### Questions
* How do you recover failing application instances today?
* What effect does this have on your application design?
* How could you determine if your application has been crashing?

## Beyond the class
Try the same exercises, but using Apps Manager instead

