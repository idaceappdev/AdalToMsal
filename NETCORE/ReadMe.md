# Brief about the sample project used
Quick note on the sample project used here to demostrate the ADAL to MSAL migration. Walkthrough video is here: https://youtu.be/3LZ6QVbiUug

## Scenario

This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.

1. The client ASP.NET Core Web App signs-in a user and obtain a JWT [ID Token](https://aka.ms/id-tokens) and an [Access Token](https://aka.ms/access-tokens) from **Azure AD**.
1. The **access token** is used as a *bearer* token to authorize the user to call the ASP.NET Core Web API protected by **Azure AD**.

## Migration guide for .NET based app
- We have our migration guide for the .NET based application at https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-migration . Since this is a web app we can refer to the below guide to make a decision,

![image](https://user-images.githubusercontent.com/62542910/207603732-652eda25-5cde-4824-875e-5f9ce9f619d7.png)
-  The current sample is in ASP.NET core 2.0 
- As far as support is considered, the minimum supported .NET version is .NET 6. Please refer the link https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- Taking the above points into consideration, we are going to demonstrate the two approaches that can be taken. 
  - Migrate the .NET core version to .NET 6 and then complete the ADAL to MSAL migration (Recommended approach)
  - In the existing project, use MSAL.NET instead of ADAL.NET 

## Project structure 
- The first folder [Asp.NetCore-ADAL-To-Identity.Web](Asp.NetCore-ADAL-To-Identity.Web) has the the project where ASP.NET 2.2 core project is migrated to .NET 6 and then uses Microsoft.Identity.Web.
- The second folder [Asp.NetCore-ADAL-To-MSAL](Asp.NetCore-ADAL-To-MSAL) simply uses the MSAL.NET in place of ADAL.NET.
