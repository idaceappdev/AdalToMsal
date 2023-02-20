
# Migration steps to be followed to migrate to Identity.Client (MSAL)
This project is built on top of the previous one.

## About this scenario
We are aiming to update the ASP.NET Core 2.0 application to use Identity.Web (MSAL) without updating the .NET Core version.
This is aimed for scenarios where the developers are unable to update the .NET Core version at this moment, but the imediate goal is to migrate away from ADAL.
All changes will happen on the web application, as this one does the access token acquisition for the API.

## Changes needed in ***TodoListWebApp*** project

### Install the following NUGET packages

```
Microsoft.Identity.Client 
Microsoft.Identity.Web.TokenCache
```

### Create a class called ***MsalAppBuilder*** with below content 

```
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Identity.Client; 
using Microsoft.Identity.Web; 
using Microsoft.Identity.Web.TokenCacheProviders.Distributed; 
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Security.Claims; 
using System.Threading.Tasks; 

namespace TodoListWebApp 
{ 
    public static class MsalAppBuilder 
    { 
        public static string GetAccountId(string oid,string tid)//ClaimsPrincipal claimsPrincipal) 
        {         
            return $"{oid}.{tid}"; 
        } 

        private static IConfidentialClientApplication clientapp; 

        public static IConfidentialClientApplication BuildConfidentialClientApplication(string clientId, 
                string secret, string authority, string redirectUri) 
        { 
            if (clientapp == null) 
            { 
                clientapp = ConfidentialClientApplicationBuilder.Create(clientId) 
                      .WithClientSecret(secret) 
                      .WithRedirectUri(redirectUri) 
                      .WithAuthority(new Uri(authority)) 
                      .Build(); 

                clientapp.AddDistributedTokenCache(services => 
                { 
                    // Do not use DistributedMemoryCache in production! 
                    // This is a memory cache which is not distributed and is not persisted. 
                    // It's useful for testing and samples, but in production use a durable distributed cache, 
                    // such as Redis. 
                    services.AddDistributedMemoryCache(); 

                    // The setting below shows encryption which works on a single machine.  
                    // In a distributed system, the encryption keys must be shared between all machines 
                    // For details see https://github.com/AzureAD/microsoft-identity-web/wiki/L1-Cache-in-Distributed-(L2)-Token-Cache#distributed-systems 
                    services.Configure<MsalDistributedTokenCacheAdapterOptions>(o => 
                    { 
                        o.Encrypt = true; 
                    }); 
                }); 
                /* 
                                // Could also use other forms of cache, like Redis 
                                // See https://aka.ms/ms-id-web/token-cache-serialization 
                                clientapp.AddDistributedTokenCache(services => 
                                { 
                                    services.AddStackExchangeRedisCache(options => 
                                    { 
                                        options.Configuration = "localhost"; 
                                        options.InstanceName = "SampleInstance"; 
                                    }); 
                                }); 
                */ 
            } 
            return clientapp; 
        } 

        //public static async Task RemoveAccount() 
        //{ 
        //    // BuildConfidentialClientApplication(); 

        //    string.Format("{0}.{1}", HttpContext.User.GetObjectId(), HttpContext.User.GetTenantId()) 
        //    var userAccount = await clientapp.GetAccountAsync(GetAccountId(HttpContext.User.GetObjectId(), HttpContext.User.GetTenantId())); 
        //    if (userAccount != null) 
        //    { 
        //        await clientapp.RemoveAsync(userAccount); 
        //    } 
        //} 
    } 
} 
```
 

### In the file ***AzureAdAuthenticationBuilderExtensions.cs*** , look for the method OnAuthorizationCodeReceived  

Comment the below code  

```
var authContext = new AuthenticationContext(context.Options.Authority, new NaiveSessionCache(userObjectId, context.HttpContext.Session));
var credential = new ClientCredential(context.Options.ClientId, context.Options.ClientSecret);
var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(context.TokenEndpointRequest.Code,
   new Uri(context.TokenEndpointRequest.RedirectUri, UriKind.RelativeOrAbsolute), credential, context.Options.Resource);
```

Add the below code 
```
IConfidentialClientApplication app = MsalAppBuilder.BuildConfidentialClientApplication(context.Options.ClientId, 
                                    context.Options.ClientSecret, context.Options.Authority, 
                                    context.TokenEndpointRequest.RedirectUri); 
var authResult = await app.AcquireTokenByAuthorizationCode( 
               new[] { $"{AzureAdOptions.Settings.TodoListResourceId}/.default" }, 
               context.TokenEndpointRequest.Code) 
               .ExecuteAsync() 
               .ConfigureAwait(false); 
```

### Inside the ***TodoController.cs***, we need to update the references used by:

Including
```
using Microsoft.Identity.Client; //MSAL.Net
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Http;
```

And removing
```
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory; //ADAL.Net
```

### Inside the ***TodoController.cs*** file add a constructor  

```
IHttpContextAccessor _context = null; 
public TodoController(IHttpContextAccessor context) 
{ 
   _context = context; 
} 
```

### Add the below method in the ***TodoController.cs*** 

```
public async Task<AuthenticationResult> GetAuthenticationResult(string resourceId) 
{ 
   IConfidentialClientApplication app = MsalAppBuilder.BuildConfidentialClientApplication(AzureAdOptions.Settings.ClientId, 
            AzureAdOptions.Settings.ClientSecret, AzureAdOptions.Settings.Authority,
            string.Format("{0}://{1}{2}", _context.HttpContext.Request.Scheme, _context.HttpContext.Request.Host, AzureAdOptions.Settings.CallbackPath)); 

   AuthenticationResult authResult; 

   var scopes = new[] { $"{resourceId}/.default" }; 
   var account = await app.GetAccountAsync(string.Format("{0}.{1}", HttpContext.User.GetObjectId(), HttpContext.User.GetTenantId())); 

   try 
   { 
         // try to get an already cached token 
         authResult = await app.AcquireTokenSilent( 
                     scopes, 
                     account) 
                     // .WithTenantId(specificTenantId)  
                     // See https://aka.ms/msal.net/withTenantId 
                     .ExecuteAsync().ConfigureAwait(false); 
   } 
   catch (MsalUiRequiredException) 
   { 
         // The controller will need to challenge the user 
         // including asking for claims={ex.Claims} 
         throw; 
   } 
   return authResult; 
} 
```

### In ***TodoController.cs***, for Index controller  

Replace
```
// Because we signed-in already in the WebApp, the userObjectId is know
string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

// Using ADAL.Net, get a bearer token to access the TodoListService 
AuthenticationContext authContext = new AuthenticationContext(AzureAdOptions.Settings.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session)); 
ClientCredential credential = new ClientCredential(AzureAdOptions.Settings.ClientId, AzureAdOptions.Settings.ClientSecret); 
result = await authContext.AcquireTokenSilentAsync(AzureAdOptions.Settings.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId)); 
```

Add the below code 
```
result = await GetAuthenticationResult(AzureAdOptions.Settings.TodoListResourceId); 
```

### In ***TodoController.cs***, update the call to ProcessUnauthorized from Index controller

From:
```
if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
   return ProcessUnauthorized(itemList, authContext);
}
```

To: 
```
if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    return ProcessUnauthorized(itemList);
}
```

### In ***TodoController.cs***, update ProcessUnauthorized from Index controller by removing

```
var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == AzureAdOptions.Settings.TodoListResourceId);
foreach (TokenCacheItem tci in todoTokens)
authContext.TokenCache.DeleteItem(tci);
```
