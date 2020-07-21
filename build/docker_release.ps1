docker run --name "DevOps_Timer" `
    -e DEVOPSCLIENTOPTIONS_ACTIVEITEMQUERYID: `
    -e DEVOPSCLIENTOPTIONS_CLOSEDTASKSQUERYID: `
    -e DEVOPSCLIENTOPTIONS_PERSONALACCESSTOKEN: `
    -e DEVOPSCLIENTOPTIONS_PROJECTNAME: `
    -e DEVOPSCLIENTOPTIONS_TEAMNAME: `
    -e DEVOPSUSEROPTIONS_EMAIL: `
    -e DEVOPSUSEROPTIONS_TARGETHOURS:8 `
    -e DEVOPSUSEROPTIONS_DAYSTARTHOUR:8 `
    -e TIMEMANAGEMENTOPTIONS_UPDATESPERHOUR:0.5 `
    --restart unless-stopped repo-tag

## WARNING
## Will not work if http repo is not added to the 'insecure-registries' 
## config under Dashboard -> Docker Settings -> Docker Engine.
## It was too much effort to setup SSL cerfs.

#'-e DEVOPSCLIENTOPTIONS_ACTIVEITEMQUERYID:'   # Query To Get Your Active Items (Bugs, User Stories etc).
#'-e DEVOPSCLIENTOPTIONS_CLOSEDTASKSQUERYID:'  # Query To Get Closed Tasks (Used to calculate time completion today)
#'-e DEVOPSCLIENTOPTIONS_PERSONALACCESSTOKEN:' # Access Token (Go into your DevOps settings to create one)
#'-e DEVOPSCLIENTOPTIONS_PROJECTNAME:'         # Name of your project on DevOps
#'-e DEVOPSCLIENTOPTIONS_TEAMNAME:'            # Name of your team on DevOps, such as Bobs Engineering Team
#'-e DEVOPSUSEROPTIONS_EMAIL:'                 # Your email adress on DevOps, such as jonathan.jackson@sgsco.com


