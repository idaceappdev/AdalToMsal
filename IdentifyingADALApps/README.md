# Listing the ADAL apps in your tenant 
## Method 1: 
Please follow the workbook solution as described in the following article: https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-get-list-of-all-active-directory-auth-library-apps

## Method 2: 

For some reason, if you cannot run the workbook solution, you could also use the KQl queries added below. 

 ```sh
  let nonInteractive = AADNonInteractiveUserSignInLogs
      | mv-expand ParsedFields=parse_json(AuthenticationProcessingDetails)
      | extend Key = ParsedFields.key
      | extend Value = ParsedFields.value
      | where Key contains "AD App"
      | where Value contains "ADAL"
      | extend details = split(Value, " ")
      | extend Version = strcat(details[3], " ", details[4])
      | project
          TimeGenerated,
          ['App Name'] = AppDisplayName,
          ['App ID'] = AppId,
          ['ADAL Version'] = Version
      | summarize ['Sign-in Count'] = dcount(TimeGenerated, 4) by ['App Name'], ['App ID'], ['ADAL Version'];
  let interactive = SigninLogs
      | mv-expand ParsedFields=parse_json(AuthenticationProcessingDetails)
      | extend Key = ParsedFields.key
      | extend Value = ParsedFields.value
      | where Key contains "AD App"
      | where Value contains "ADAL"
      | extend details = split(Value, " ")
      | extend Version = strcat(details[3], " ", details[4])
      | project
          TimeGenerated,
          ['App Name'] = AppDisplayName,
          ['App ID'] = AppId,
          ['ADAL Version'] = Version
      | summarize ['Sign-in Count'] = dcount(TimeGenerated, 4) by ['App Name'], ['App ID'], ['ADAL Version'];
  let spLogIns = AADServicePrincipalSignInLogs
      | mv-expand ParsedFields=parse_json(AuthenticationProcessingDetails)
      | extend Key = ParsedFields.key
      | extend Value = ParsedFields.value
      | where Key contains "AD App"
      | where Value contains "ADAL"
      | extend details = split(Value, " ")
      | extend Version = strcat(details[3], " ", details[4])
      | project
          TimeGenerated,
          ['App Name'] = ServicePrincipalName,
          ['App ID'] = AppId,
          ['ADAL Version'] = Version
      | summarize ['Sign-in Count'] = dcount(TimeGenerated, 4) by ['App Name'], ['App ID'], ['ADAL Version'];
  union interactive, nonInteractive,spLogIns
```
## Method 3:

The below powershell script will extract the ADAL Apps ,based on the telemetry in the sign-in logs.You need to update the tenant ID in “$tid” variable & in the number of days in “$agoDays” variable. The recommendation is having the value to max to 7 days. The script will generate 3 CSV files named - Interactive_ADAL_$tId.csv , NonInteractive_ADAL_$tId.csv and WorkloadIdentities_ADAL_$tId.csv. By default the output files will be saved in profile folder (eg:- C:\Users\useralias)

```sh
$tId = "your-tenant-id"  # Add tenant ID from Azure Active Directory page on portal.
$agoDays = 4  # Will filter the log for $agoDays from the current date and time.

$startDate = (Get-Date).AddDays(-($agoDays)).ToString('yyyy-MM-dd')  # Get filter start date.
$pathForExport = "./"  # The path to the local filesystem for export of the CSV file.


Connect-MgGraph -Scopes "AuditLog.Read.All" -TenantId $tId  # Or use Directory.Read.All.
Select-MgProfile "beta" 

$clauses = (

    "createdDateTime ge $startDate",
    "signInEventTypes/any(t: t eq 'nonInteractiveUser')",
    "signInEventTypes/any(t: t eq 'servicePrincipal')"
    
)

# Get the interactive and non-interactive sign-ins based on filtering clauses.

$signInsInteractive = Get-MgAuditLogSignIn -Filter ($clauses[0] -Join " and ") -All
$signInsNonInteractive = Get-MgAuditLogSignIn -Filter ($clauses[0,1] -Join " and ") -All
$signInsWorkloadIdentities = Get-MgAuditLogSignIn -Filter ($clauses[0,2] -Join " and ") -All


$columnList = @{  # Enumerate the list of properties to be exported to the CSV files.
    Property = "CorrelationId","AppDisplayName", "AppId",@{Name="AuthenticationProcessingdetails"; expression={$_.AuthenticationProcessingdetails.value -like '*ADAL*'}}
}


$columnListWorkloadId = @{ #Enumerate the list of properties for workload identities to be exported to the CSV files.
    Property = "CorrelationId", "AppDisplayName","ServicePrincipalId", "ServicePrincipalName"
}

$signInsInteractive | ForEach-Object {
    foreach ($authDetail in $_.AuthenticationProcessingDetails)
    {
        if (($authDetail.Key -match "Azure AD App") -and ($authDetail.Value -match "ADAL"))
        {
            $_ | Select-Object @columnList
        }
    }
} | Export-Csv -Path ($pathForExport + "Interactive_ADAL_$tId.csv") -NoTypeInformation


$signInsNonInteractive | ForEach-Object {
    foreach ($authDetail in $_.AuthenticationProcessingDetails)
    {
        if (($authDetail.Key -match "Azure AD App") -and ($authDetail.Value -match "ADAL"))
        {
            $_ | Select-Object @columnList
        }
    }
} | Export-Csv -Path ($pathForExport + "NonInteractive_ADAL_$tId.csv") -NoTypeInformation

$signInsWorkloadIdentities | ForEach-Object {
    foreach ($authDetail in $_.AuthenticationProcessingDetails)
    {
        if (($authDetail.Key -match "Azure AD App") -and ($authDetail.Value -match "ADAL"))
        {
            $_ | Select-Object @columnListWorkloadId
        }
    }
} | Export-Csv -Path ($pathForExport + "WorkloadIdentities_ADAL_$tId.csv") -NoTypeInformation
```
