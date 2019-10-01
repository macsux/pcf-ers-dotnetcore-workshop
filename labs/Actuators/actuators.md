# Actuators

As companies move towards DevOps model, getting rich, accurate, and real time telemetry is critical for diagnosing issues for apps in production. An emerging pattern is a concept of actuators, where each app exposes a set of endpoints exposing internal telemetry about the app. Some examples of actuators are 

- env - report environment information, including configuration loaded by the app, environmental variables, etc
- health - report on app's internal health status and any required external dependencies
- http trace - capture recent calls as received by the application, including headers sent, response codes, latency, etc
- mappings - URLs the application is configured to listen on (controller mappings)
- loggers - view current logger settings, and ability to change log levels at runtime

[SteelToe library](https://steeltoe.io/) offers a framework for installing many common actuators including the ones listed above, and patterns for implementing your own. The Articulate app has been instrumented with SteelToe actuators. 

Try accessing them at /actuators endpoint and explore different information available.

### Integration with Apps Manager

Pivotal apps manager is able to automatically detect the presence of many actuators in the app, and automatically expose information found in them in the web UI. It does this by probing the app on the `/cloudfoundryapplication` endpoint, which acts as HATEOS endpoint for all actuators exposed to the platform. Access to the actuators is automatically secured by platform's internal OAuth2 server. Pivotal apps manager is able to obtain the right token automatically, so it's seamless. However, hitting one of the cloud foundry actuator endpoints on without token will give 404 Unauthorized. 

*Note that the sample app exposes two actuator endpoints - /actuator which is unsecured, and /cloudfoundryapplication for integration with Apps Manager. In real applications you would only want to use later one outside of your local environment*

Log into apps manager and go to the `articulate-ui` app

![1569366390650](..\images\actuators-health-check.png)

1. Notice the the Steeltoe icon next to app name indicating that Apps Manager has found actuators on the app
2. Expand dropdown arrow on instance list. 
3. See detail information for **each individual** container. Compare this to raw information as exposed by the app by accessing `/actuators/health` endpoint
4. Notice the Git SHA code from which the app was built. This acts as your version stamp.

Access the Logs tab. Press the `Configure Logging Levels` button (which only appears when logging actuator is enabled). In Filter, try typing a logger name (ex. Steeltoe, or Microsoft). Notice how you can set log level for each logger individual using the slider.

![1569365798152](..\images\actuators-dynamic-logging.png)

Try increasing `Microsoft` log level to `Trace`. Tail the logs by pressing the PLAY button in the logs tab, and try accessing the application. Notice the increase in extra logging information coming from the app

Access the `Trace` tab, and access the site again to generate some traffic. Go back to apps manager, and click Refresh. Notice the internal request telemetry exposed for each request as captured by the app.

![1569366102904](..\images\actuators-trace.png)

Access the `Settings` tab.

Notice the presence of additional info, which tells you the Git commit SHA code it was built from, who made the built. The `View Mappings` button lets you view every endpoint exposed by the app, and the corresponding Controller and Action servicing requests at that route.

![1569366344201](..\images\actuator-settings)

![1569366322550](..\images\actuator-mappings.png)