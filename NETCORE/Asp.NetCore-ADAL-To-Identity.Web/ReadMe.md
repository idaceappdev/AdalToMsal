# Project structure 
- The first folder [1.Before-webapp-webapi-ADAL](1.Before-webapp-webapi-ADAL) has the ASP.NET 2.2 core project code which uses ADAL to sign-in and acquire the token to call an API.
- The second folder [2. Intermediate-NETMigration-webapp-webapi](2.%20Intermediate-NETMigration-webapp-webapi) has the project which is built on top of the above project and walks you through the steps involved to migrate the ASP.NET core 2.2 to .NET 6 which is a minimum .NET supported version. The project still uses ADAL. 
- The third folder [3. After-migration-webapp-webapi-MSAL](3.%20After-migration-webapp-webapi-MSAL) has the project which is built on top of the above project and here you will see the changes related to the code where actual migration happens from ADAL to Identity.Web.   
- The fourth folder [4. Nice-Todo-Webapp-webapi-MSAL](4.%20Nice-Todo-Webapp-webapi-MSAL) has a readme and code snippets which are nice to do things leveraging the full capabilities of Microsoft.Identity.Web & MSAL.  

