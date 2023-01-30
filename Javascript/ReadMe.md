# Brief about the sample project used.

Quick note on the sample project used here to demonstrate the ADAL to MSAL migration.

## Scenario

This sample demonstrates a Javascript (JQuery) ADAL single page app (SPA) calling an ASP .NET Web API that is secured using Azure AD.

The client SPA signs-in a user and obtain a JWT ID-Token and an Access-Token from Azure AD.
The Access-Token is then used as a _bearer_ token to authorize the user to call the ASP .NET Web API protected by Azure AD.

[Note]: The sample uses a single app-registration for that defines both the frontend SPA and the backend TodoList API.

## Migration guide for Javascript Based App

We have our migration guide for the Javascript based single page application (SPA) at [https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-compare-msal-js-and-adal-js](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-compare-msal-js-and-adal-js).

## Project Structure

- The folder "Before (With ADAL)" has the project where we have the Javascript SPA with ADAL.js code implementing using JQuery.
- The folder "After (With MSAL)" has the project where we have the same Javascript SPA with ADAL.js code migrated to MSAL.js using JQuery.

## Steps to test this sample

1. Follow the instructions available in the "Before (With ADAL) directory to setup the sample SPA and the API".
2. Next, follow the instructions available in the "After (With MSAL)" directory to migrate your ADAL code to MSAL.
