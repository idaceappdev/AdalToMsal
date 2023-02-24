# Get more insights on the apps, starting from the appID
This script will enable you to get more insights on apps, starting from the application ID.
Output:
- ***DisplayName*** - The display name of the app
- ***AppOwnerOrganizationId*** - The organization where that application is registered. Depending on the way the application was created, this field can be blank.
- ***ApplicationObjectId*** - The ObjectID of the application object. In our portals, these objects are available under App Registrations section. For multitenant apps, this can be blank.
- ***ServicePrincipalObjectId*** - The ObjectID of the service principal.
- ***ApplicationOwnerObjectId*** - The ObjectIDs of the owner objects of the application. This can be blank.
- ***ApplicationOwner*** - The names of the owner objects of the application. This can be blank.
- ***ServicePrincipalOwnerObjectId*** - The ObjectIDs of the owner objects of the service principals. This can be blank.
- ***ServicePrincipalOwner*** - The names of the owner objects of the application. This can be blank.
- ***ServiceManagementReference*** - This property should be set to the application object to reference the team contact information from your enterprise Service or Asset Management Database. https://learn.microsoft.com/en-us/azure/active-directory/manage-apps/overview-assign-app-owners#faq
- ***ApplicationDescription*** - This contains the agregation of the Description and Notes fields available on an application object. (https://learn.microsoft.com/en-us/graph/api/resources/application?view=graph-rest-1.0#properties)
- ***ServicePrincipalDescription*** - This contains the agregation of the Description and Notes fields available on a service principal object. (https://learn.microsoft.com/en-us/graph/api/resources/serviceprincipal?view=graph-rest-1.0#properties)

# Prerequisits
This script is using the Microsoft Graph PowerShell module.
Installing it: https://learn.microsoft.com/en-us/powershell/microsoftgraph/installation?view=graph-powershell-1.0

# Utilizing the script
This script relays on receiving a list of application IDs in a csv file.

Sample format:

```
AppIDs
{AppID01}
{AppID02}
{AppID03}
...
```

Effective call:
```
.\GetOwners.ps1 -ApplicationIDcsv appIDs.csv
```

The output will be set in the same location as the original file, with an unique name.
The format will be: [original file name]-Export-[date with the format yyMMddHHmmss].csv

Sample names
```
Origin: appIDs.csv
Output: appIDs-Export-230224162009.csv
```

