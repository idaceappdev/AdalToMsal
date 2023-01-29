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
