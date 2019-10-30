# Blue Green Deployment

You'll learn how to manage application upgrades with a blue-green deployment

To simulate a blue-green deployment, first scale `articulate-ui` to multiple instances.

```
$ cf scale articulate-ui -i 2
```

## Perform a Blue-Green Deployment
Read about using Blue-Green Deployments to reduce downtime and risk.
Browse to the articulate Blue-Green page.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\blue_green1.png)

Lets assume that the deployed application is version 1. Let's generate some traffic. Press the `Start` button.

_Leave this open as a dedicated tab in your browser. We will come back to this later._

Observe our existing application handling all the web requests.
Start Load
Record the hostname for the articulate application.
This is our production route. _You will use this in the next step._
For example:

```
$ cf routes

Getting routes for org Canada / space astakhov as astakhov@pivotal.io ...

space      host                               domain      port   path   type   apps                            service
astakhov   articulate-ui-relaxed-panther       cfapps.io                        articulate-ui
```

Now let's `push` the next version of `articulate-ui`.

However, this time we will specify the hostname by appending `-v2` to our production route.

For example (your hostname will be different):
```
$ cd ~/pcf-ers-workshop/src/Articulate.UI/bin/Debug/netcoreapp2.2/publish
$ cf push articulate-ui-v2 --no-start
```

Bind articulate-ui-v2 to the articulate-service user provided service.
```
$ cf bind-service articulate-ui-v2 articulate-service
```
_You can ignore the "TIP: Use 'cf restage articulate-ui-v2' to ensure your env variable changes take effect" message at this time._

Start the application.
```
$ cf start articulate-ui-v2
```

Now we have two versions of our app deployed.

Open a new tab and view version 2 of `articulate` in your browser. Take note of the application name.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\blue_green2.png)
At this point in the deployment process, you could do further testing of the version you are about to release before exposing customers to it.
Let's assume we are ready to start directing production traffic to version 2. We need to map our production route to `articulate-ui-v2`.
For example (your domain and hostname may be different):

```
$ cf map-route articulate-ui-v2 cfapps.io -n  articulate-ui-relaxed-panther
```

Return to browser tab where you started the load. You should see that it is starting to send requests to version 2.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\blue_green3.png)

Press the `Reset` button, so we can see how the load get distributed across app instances.

If you are running with a similar configuration to this:
```
$ cf apps

Getting apps in org mborges-org / space development as admin...
OK

name                     requested state   instances   memory   disk   urls
articulate-ui                started           2/2         768M     1G     ...
articulate-ui-v2             started           1/1         768M     1G     ...
```
You should see about a third of the requests going to version 2.

Move more traffic to version 2.
```
$ cf scale articulate-ui -i 1
$ cf scale articulate-ui-v2 -i 2
```

If you `Reset` the load generator, you will see 2/3 of the traffic go to `articulate-ui-v2`.
Move all traffic to version 2.

Remove the production route from the `articulate-ui` application.

For example (your domain and hostname may be different):

```
$ cf unmap-route articulate-ui cfapps.io -n  articulate-ui-relaxed-panther
```
If you `Reset` the load generator, you will see all the traffic goes to `articulate-v2`.

![](C:\Projects\pcf-ers-dotnetcore-workshop\labs\images\blue_green5.png)
*_NOTE:_* Refreshing the entire page will update the application name.
Remove the temp route from the articulate-v2 application.
For example (your domain and hostname may be different):
```
$ cf unmap-route articulate-v2 cfapps-01.haas-66.pez.pivotal.io -n articulate-heartsickening-elegance-temp
```
*Congratulations!* You performed a blue-green deployment.

### Questions
* How would a rollback situation be handled using a blue-green deployment?
* What other design implications does running at least two versions at the same time have on your applications?
* Do you do blue-green deployments today? How is this different?

## Cleanup
Let's reset our environment.

Delete the articulate application.
```
$ cf delete articulate-ui
```

Rename articulate-v2 to articulate.
```
$ cf rename articulate-ui-v2 articulate-ui
```
Restart articulate.
```
$ cf restart articulate-ui
```

Scale down.
```
$ cf scale articulate-ui -i 1
```
