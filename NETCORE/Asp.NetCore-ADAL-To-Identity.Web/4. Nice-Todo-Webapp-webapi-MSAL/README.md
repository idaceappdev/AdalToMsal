

# Nice to do things on top of the project leveraging the Microsoft.Identity.Web capabilities. 

## 1. Usage of certificate over secrets 
Please refer the link https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates which explains hpw to use the certificate over secret with only few lines of code or configuration change. For example, on the same example, without any code change, Identity.Web can use the certificate which is present in the Keyvault and use it for authentication 

```sh
"ClientCertificates": [
      {
        "SourceType": "KeyVault",
        "KeyVaultUrl": "https://msidentitywebsamples.vault.azure.net",
        "KeyVaultCertificateName": "MicrosoftIdentitySamplesCert"
      }
     ]
  }
```

## 2. Usage of distributed cache 
For web apps that call web APIs and web APIs that call downstream APIs, the library provides several token cache serialization methods. Please refer the link https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization

## 3. Leverage Microsoft.Identity.Web in the API project to validate the token. 
 Microsoft.Identity.Web can be used to in API porject which does the token validation. 
 
 ### Changes needed in TodoListService project
- The TodoListService API does not use the ADAL library and doesn’t need any changes. But it is advisable to follow the below steps which takes care of validatation of the token.
- Remove the below package reference  

   ```sh
   <ItemGroup> 
      <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
    </ItemGroup> 
  ```
- Install the NUGET **Microsoft.Identity.Web** add the namespace **using Microsoft.Identity.Web;**in Startup.cs file
- Comment the below line of code 
   
   ```sh
    using Microsoft.AspNetCore.Authentication.AzureAD.UI; 
    using Microsoft.AspNetCore.Authentication.JwtBearer; 
  ```
 - Replace the below code  
 
  ```sh
     services.AddAuthentication(sharedOptions => 
            { 
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
            }) 
            .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options)); 
  ```
  By 

  ```sh
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false; 
    services.AddMicrosoftIdentityWebApiAuthentication(Configuration); 
  ```

- Comment all the code from the below files  
  
   - AzureAdAuthenticationBuilderExtensions.cs
   - AzureAdOptions.cs
- Now the service API can validate the specific scope in the token.  

## 4. Granular level of scoping the API
In the ADAL world we acquire the token for the resources. We can have more granular level of scoping. For example, in the current example we can have separate scoping for **Read** & && **Write** To implement this we need below changes 

### Changes required in Azure portal

- Go to the API app registration and expose 2 more scopes named as **ToDoList.Read** & **ToDoList.ReadWrite** 
- Add these API permissions in Web app project under the API Permission blade. 

### Changes needed in TodoListService project

- Add the below line of code in the TodoListController  

    ```sh
     private const string _todoListReadScope = "ToDoList.Read"; 
     private const string _todoListReadWriteScope = "ToDoList.ReadWrite"; 
    ```
 - Declare the below attribute the Get method of the controller 
 
   ```sh
   [RequiredScope( 
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope } 
            )] 
    ```
  Declare the below attribute the Post method of the controller 
  
   ```sh
  [RequiredScope( 
            AcceptedScope = new string[] { _todoListReadWriteScope } 
            )]
   ```
 
### Changes needed in TodoListWebApp project

- Declare the below 2 scopes in the appsettings.json 
 
 ```sh
  "TodoList": {
    "TodoListScopes": "api://<enter the client id of the TodoListService>/ToDoList.Read api://<enter the client id of the TodoListService>/ToDoList.ReadWrite",
    "TodoListBaseAddress": "https://localhost:44351"
  },
```

## 5. Incremental consent feature 
App could also leverage the incremental consent feature by following the instructions at https://github.com/AzureAD/microsoft-identity-web/wiki/Managing-incremental-consent-and-conditional-access which will avoid the upfront consent for all the permissions 

## 6. Proof Of Possession (PoP) tokens
Bearer tokens are the norm in modern identity flows, however they are vulnerable to being stolen and used to access a protected resource. Please refer the link for more details https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Proof-Of-Possession-%28PoP%29-tokens

## 7. Continuous Access Evaluation 
CAE is an Azure AD feature that allows access tokens to be revoked based on critical events and policy evaluation rather than relying on token expiry based on lifetime. Please refer the link https://learn.microsoft.com/en-us/azure/active-directory/develop/app-resilience-continuous-access-evaluation?tabs=dotnet

Current sample doesnt use graph API. If any web app calls the graph api, we can leverage the CAE feature below configuration 
 ```sh
{
  "AzureAd": {
    // ...
    // the following is required to handle Continuous Access Evaluation challenges
    "ClientCapabilities": [ "cp1" ],
    // ...
  },
  // ...
}
 ```
Code to handle the CAE exception 

 ```sh
 // Catch CAE exception from Graph SDK
  catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
  {
    try
    {
      Console.WriteLine($"{svcex}");
      string claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);
      _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
      return new EmptyResult();
    }
    catch (Exception ex2)
    {
      _consentHandler.HandleException(ex2);
    }
  }   
   ```
Here is the code sample - https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-1-Call-MSGraph
