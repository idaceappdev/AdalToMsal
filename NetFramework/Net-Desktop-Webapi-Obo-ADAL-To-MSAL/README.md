# Project structure 
- The first folder [1. Before-desktop-webapi-obo-ADAL](https://github.com/idaceappdev/AdalToMsal/tree/main/NetFramework/Net-Desktop-Webapi-Obo-ADAL-To-MSAL/1.%20Before-desktop-webapi-obo-ADAL) has the .NET Framework Desktop app calling an ASP.NET Web API, which in turn calls the Microsoft Graph API using an access token obtained using the on-behalf-of flow. The project utilizes ADAL authentication library.
- The second folder [2. After-desktop-webapi-obo-MSAL](https://github.com/idaceappdev/AdalToMsal/tree/main/NetFramework/Net-Desktop-Webapi-Obo-ADAL-To-MSAL/2.%20After-desktop-webapi-obo-MSAL)) has the project which is built on top of the above project and here you will see the changes related to the code where actual migration happens from ADAL to MSAL.   
- The third folder [3. Nice-Todo-desktop-webapi-obo-MSAL](https://github.com/idaceappdev/AdalToMsal/tree/main/NetFramework/Net-Desktop-Webapi-Obo-ADAL-To-MSAL/3.%20Nice-Todo-desktop-webapi-obo-MSAL) has a readme with nice to do items leveraging the full capabilities of MSAL .NET.



