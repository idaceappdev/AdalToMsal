# Brief about the sample project used. 
Quick note on the sample project used here to demostrate the ADAL to MSAL migration. 

### Scenario

This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.

1. The client ASP.NET Core Web App signs-in a user and obtain a JWT [ID Token](https://aka.ms/id-tokens) and an [Access Token](https://aka.ms/access-tokens) from **Azure AD**.
1. The **access token** is used as a *bearer* token to authorize the user to call the ASP.NET Core Web API protected by **Azure AD**.
