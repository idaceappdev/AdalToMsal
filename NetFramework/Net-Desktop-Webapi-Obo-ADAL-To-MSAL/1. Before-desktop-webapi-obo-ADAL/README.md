---
services: active-directory
platforms: dotnet
client: .NET Framework Desktop App
service: ASP.NET Web API
endpoint: AAD V1
---
# Calling a downstream web API from a web API using Azure AD

## About this sample

### Overview

In this sample, the flow is as following:

1. Sign-in to the client application.
1. Acquire a token for the Asp.net Web API (`TodoListService`) and call it.
1. The Asp.Net Web API authorizes the caller, obtains another [access token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) using the [on-behalf-of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) and then calls another downstream Web API ([Microsoft Graph](https://graph.microsoft.com)).


   ![Topology](./ReadmeFiles/Topology.png)

### Scenario. How the sample uses ADAL.NET

- `TodoListClient` uses  Active Directory Authentication Library for .NET (ADAL.NET) to acquire a token for the user in order to call the first web API. For more information about how to acquire tokens interactively, see [Acquiring tokens interactively Public client application flows](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-interactively---Public-client-application-flows).
- Then `TodoListService` also uses ADAL.NET  to get a token to act on behalf of the user to call the Microsoft Graph. For details, see [Service to service calls on behalf of the user](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Service-to-service-calls-on-behalf-of-the-user). It then decorates the todolist item entered by the user, with the First name and the Last name of the user. Below is a screen copy of what happens when the user named *automation service account* entered "item1" in the textbox.

  ![Todo list client](./ReadmeFiles/TodoListClient.png)

Both flows use the OAuth 2.0 protocol to obtain the tokens. For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).


## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet pacakges, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can follow the steps in the paragraphs below ([Step 2](#step-2--register-the-sample-with-your-azure-active-directory-tenant) and [Step 3](#step-3--configure-the-sample-to-use-your-azure-ad-tenant))


#### First step: choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. On the top bar, click on your account, and then on **Switch Directory**. 
1. Once the *Directory + subscription* pane opens, choose the Active Directory tenant where you wish to register your application, from the *Favorites* or *All Directories* list.
1. Click on **All services** in the left-hand nav, and choose **Azure Active Directory**.

> In the next steps, you might need the tenant name (or directory name) or the tenant ID (or directory ID). These are presented in the **Properties**
of the Azure Active Directory window respectively as *Name* and *Directory ID*

#### Register the service app (TodoListService-OBO)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-OBO`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. Select **Save** to save your changes.
1. In the app's registration screen, click on the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, click on **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security posture.
   - The generated key value will be displayed when you click the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button at the bottom.
   - Note User.Read permission is normally included automatically, if you don't see this permission, you could follow the steps to add it.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can declare the parameters to expose this app as an Api for which client applications can obtain [access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for.
The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this Api. To declare an resource URI, follow the following steps:
   - Click `Set` next to the **Application ID URI** to generate a URI that is unique for this app.
   - For this sample, accept the proposed Application ID URI (api://{clientId}) by selecting **Save**.
1. All Apis have to publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code) for the client's to obtain an access token successfully. To publish a scope, follow the following steps:
   - Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
        - For **Scope name**, use `user_impersonation`.
        - Select **Admins and users** options for **Who can consent?**
        - For **Admin consent display name** type `Access TodoListService-OBO`
        - For **Admin consent description** type `Allows the app to access TodoListService-OBO as the signed-in user.`
        - For **User consent display name** type `Access TodoListService-OBO`
        - For **User consent description** type `Allow the application to access TodoListService-OBO on your behalf.`
        - Keep **State** as **Enabled**
        - Click on the **Add scope** button on the bottom to save this scope.

#### Register the client app (TodoListClient-OBO)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-OBO`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Public client (mobile & desktop)** option.
   - In the **Redirect URIs** | **Suggested Redirect URIs for public clients (mobile, desktop)** section, select **https://login.microsoftonline.com/common/oauth2/nativeclient**
1. Select **Save** to save your changes.
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected.
   - In the list of APIs, select the API `TodoListService-OBO`.
   - In the **Delegated permissions** section, select the **Access 'TodoListService-OBO'** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button at the bottom.


#### Configure known client applications for service (TodoListService-OBO)

For a middle tier Web API (`TodoListService-OBO`) to be able to call a downstream Web API, the middle tier app needs to be granted the required permissions as well.
However, since the middle tier cannot interact with the signed-in user, it needs to be explicitly bound to the client app in its Azure AD registration.
This binding merges the permissions required by both the client and the middle tier Web Api and presents it to the end user in a single consent dialog. The user then consent to this combined set of permissions.

To achieve this, you need to add the **Application Id** of the client app, in the Manifest of the Web API in the `knownClientApplications` property. Here's how:

1. In the [Azure portal](https://portal.azure.com), navigate to your `TodoListService` app registration, and select **Manifest** section.
1. In the manifest editor, change the `"knownClientApplications": []` line so that the array contains 
   the Client ID of the client application (`TodoListClient-OBO`) as an element of the array.

    For instance:

    ```json
    "knownClientApplications": ["ca8dca8d-f828-4f08-82f5-325e1a1c6428"],
    ```

1. **Save** the changes to the manifest.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, ClientID is the same as Application ID or AppId.

Open the solution in Visual Studio to configure the projects

#### Configure the service project

1. Open the `TodoListService\Web.Config` file
1. Find the app key `ida:Tenant` and replace the existing value with your AAD tenant name, e.g. contoso.onmicrosoft.com.
1. Find the app key `ida:Audience` and replace the existing value with the App ID URI you registered earlier, when exposing an API. For instance use `api://<application_id>`.
1. Find the app key `ida:AppKey` and replace the existing value with the client secret you saved during the creation of the `TodoListService-OBO` app, in the Azure portal.
1. Find the app key `ida:ClientID` and replace the existing value with the application ID (clientId) of the `TodoListService-OBO` application copied from the Azure portal.
1. Find the app key `ida:Issuer` and replace the existing value with `https://login.microsoftonline.com/{TENANT_ID}/v2.0`.

#### Configure the client project

1. Open the `TodoListClient\App.Config` file
1. Find the app key `ida:Tenant` and replace the existing value with your AAD tenant name, e.g. contoso.onmicrosoft.com.
1. Find the app key `ida:ClientId` and replace the existing value with the application ID (clientId) of the `TodoListClient-OBO` application copied from the Azure portal.
1. Find the app key `ida:RedirectUri` and replace the existing value with "https://login.microsoftonline.com/common/oauth2/nativeclient".
1. Find the app key `todo:TodoListResourceId` and replace the existing value with the Client ID of the TodoListService-OBO app. 
1. Find the app key `todo:TodoListBaseAddress` and replace the existing value with the base address of the TodoListService-OBO project (by default `https://localhost:44321/`).


### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects, or the three projects, as startup projects, with the service project starting first.

Explore the sample by signing in, adding items to the To Do list, Clearing the cache (which removes the user account), and starting again.  The To Do list service will take the user's access token, received from the client, and use it to get another access token so it can act On Behalf Of the user in the Microsoft Graph API.  This sample caches the user's access token at the To Do list service, so it does not request a new access token on every request. This cache is a database cache.

[Optionally], when you have added a few items with the TodoList Client, login to the todoListSPA with the same credentials as the todoListClient, and observe the id-Token, and the content of the Todo List as stored on the service, but as Json. This will help you understand the information circulating on the network.

## About the code

The code using ADAL.NET is in the [TodoListClient/MainWindow.xaml.cs](TodoListClient/MainWindow.xaml.cs) file in the `SignIn()` method. See [More information][#More-information] below for details on how this work. The call to the TodoListService is done in the `AddTodoItem()` method.

The code for the Token cache serialization on the client side (in a file) is in [TodoListClient/FileCache.cs](TodoListClient/FileCache.cs)

The code acquiring a token on behalf of the user from the service side is in [TodoListService/Controllers/TodoListController.cs](TodoListService/Controllers/TodoListController.cs)

The code for the Service side serialization (in a database) is in [TodoListService/DAL/DbTokenCache.cs](TodoListService/DAL/DbTokenCache.cs). you can see how it's referenced by the Controller in the [CallGraphAPIOnBehalfOfUser()](https://github.com/Azure-Samples/active-directory-dotnet-webapi-onbehalfof/blob/49ddb0a47018db1d1cc2c397341bdc2331bcb502/TodoListService/Controllers/TodoListController.cs#L154) method.

## Verifying this app is using ADAL

### Verifying this app is using ADAL (From Admin perspective)

1. Get network trace, notice the URL during the sign-in process which will be using the V1 endpoint.
    
    https://login.microsoftonline.com/<Tenant_ID>/oauth2/authorize?client_id=<client_id>&redirect_uri=...
  
2. Go to Azure portal and observe the sign -in logs and click on the additional details tab and observe the ADAL information(under non-interactive logs)
    

3. If you have setup the workbook solution to track the ADAL apps as explained [here](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-get-list-of-all-active-directory-auth-library-apps) 
    

### Verifying this app is using ADAL (From developer perspective)

1. Search the entire project/solution to identify whether ADAL NUGET package is installed - The name of the package is `Microsoft.IdentityModel.Clients.ActiveDirectory`   
2. Search the entire project/solution for the namespace `using Microsoft.IdentityModel.Clients.ActiveDirectory;`
3. Search the entire project/solution for the class `AuthenticationContext`


## More information

For more information, see ADAL.NET's conceptual documentation:

- [Recommended pattern to acquire a token](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token#recommended-pattern-to-acquire-a-token)
- [Acquiring tokens interactively in public client applications](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-interactively---Public-client-application-flows)
- [Service to service calls on behalf of the user](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Service-to-service-calls-on-behalf-of-the-user).
- [Customizing Token cache serialization](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Token-cache-serialization)

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).
