# **Project structure**

   - The first folder [1.Before-daemon-webapi-ADAL](https://github.com/Mansv/AdalToMsal/tree/main/NETCore/1.Before-daemon-webAPI-ClientCred-ADAL) has the console .NET core project code which uses ADAL to sign-in and acquire the token to call an API.
   - The second folder [2.After-migration-daemon-webapi-MSAL](https://github.com/Mansv/AdalToMsal/tree/main/NETCore/2.After-daemon-webAPI-ClientCred-MSAL) has the project which is built on top of the above project and here you will see the changes related to the code where actual migration happens from ADAL to MSAL.
   - The third folder [3.Nice-Todo-daemon-webapi-MSAL](https://github.com/Mansv/AdalToMsal/tree/main/NETCore/3.Nice-Todo-daemon-webapi-MSAL) has a readme and code snippet which are nice to do thingsleveraging the full capabilities of Microsoft.Identity.Web & MSAL


The sample shown here explains how an ASP.NET Core 2.2 targeted projects are migrated to .NET 6.0. Please refer to the below doc for detailed information -

#### Migration guide for .NET based app
We have our migration guide for the .NET based application at https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-migration . Sinnce this is a web app, we could take the below decision point image
The current sample is in ASP.NET core 2.0
As far as support is considered, the minimum supported .NET version is .NET 6. Please refer the link https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
Taking the above into consideration, we are going to demostrate the two approaches that can be taken.
Migrate the .NET core version to .NEt 6 and then complete the ADAL to MSAL migration ( Reccomanded approach)
In the existing project use the MSAL.NET instead of ADAL.NET

