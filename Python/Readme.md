
# Brief about the sample project used. 
Quick note on the sample project used here to demostrate the ADAL to MSAL migration. 

## Scenario

This sample demonstrates an Python flask Web App calling Graph API that is secured using Azure AD.

1. The python ASP Web App signs-in a user and obtain a JWT [ID Token](https://aka.ms/id-tokens) and an [Access Token](https://aka.ms/access-tokens) from **Azure AD**.
1. The **access token** is used as a *bearer* token to authorize the user to call the Graph API protected by **Azure AD**.

## Migration guide for python based app
- We have our migration guide for the .NET based application at https://learn.microsoft.com/en-us/azure/active-directory/develop/migrate-python-adal-msal. S

## Project structure 
- The first folder [PythonWebApp-ADAL-To-MSAL](PythonWebApp-ADAL-To-MSAL) has the the project where Python flask Web App calling Graph API 
