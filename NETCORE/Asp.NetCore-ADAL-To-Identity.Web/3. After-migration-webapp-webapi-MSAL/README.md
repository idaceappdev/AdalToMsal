# Migration steps to be followed to migrate to Identity.Web(MSAL) and from ADAL in .NET 6

This project is built on top of the previous project. 

## Changes needed in Azure portal

- Go to the API app registration and expose 2 more scopes named as **ToDoList.Read** & **ToDoList.ReadWrite** 
- Add these API permissions in Web app project under the API Permission blade. 

## Changes needed in TodoListService project

- The TodoListService API does not use the ADAL library and doesn’t need any changes. But it is advisable to follow the below steps which takes care of validatation of the token.
- Remove the below package reference  

   ```sh
   <ItemGroup> 
      <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
    </ItemGroup> 
  ```
- Install the NUGET **Microsoft.Identity.Web** in Startup.cs file
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
- Comment all the code from the below files  
  
   - AzureAdAuthenticationBuilderExtensions.cs
   - AzureAdOptions.cs
- Now the service API can validate the specific scope in the token.  

## Changes needed in TodoListWebApp project

- Remove the below package reference  
   
   ```sh
   <ItemGroup> 
    <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
    <PackageReference Include="Microsoft.IdentityModel.Clients.ActiveDirectory" Version="5.3.0" /> 
  </ItemGroup> 
    ```
 - Install the NUGET **Microsoft.Identity.Web.UI** & **Newtonsoft.Json**
 - Comment all the code from the below files  
  
  **AzureAdAuthenticationBuilderExtensions.cs**
  **AzureAdOptions.cs**
  **NaiveSessionCache.cs**
  **AccountController.cs**
- Comment all the line of code under configureservices in startup.cs  
 
   ```sh
   services.AddAuthentication(sharedOptions => 
            { 
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; 
               sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; 
            }) 
            .AddAzureAd(options => 
            { 
                Configuration.Bind("AzureAd", options); 
                AzureAdOptions.Settings = options; 
            }) 
            .AddCookie(); 
            services.AddMvc() 
                .AddSessionStateTempDataProvider(); 
            services.AddSession();   
    ```
 - Add below namespaces in the **Startup.cs**
  
  ```sh
  using Microsoft.AspNetCore.Http; 
  using Microsoft.Identity.Web; 
  using Microsoft.Identity.Web.UI;    
  ```
  Add below line of code in the configureServices method.
  
   ```sh
     services.AddDistributedMemoryCache(); 
     services.Configure<CookiePolicyOptions>(options => 
        { 
                // This lambda determines whether user consent for non-essential cookies is needed for a given request. 
                options.CheckConsentNeeded = context => true; 
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified; 
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1 
                options.HandleSameSiteCookieCompatibility(); 
            }); 
            services.AddOptions(); 
    // The following lines of code adds the ability to authenticate users of this web app. 
    // Refer to https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps to learn more 
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration) 
                    .EnableTokenAcquisitionToCallDownstreamApi( 
                        Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries) 
                     ) 
                    .AddInMemoryTokenCaches(); 
            services.AddControllersWithViews(options => 
            { 
                var policy = new AuthorizationPolicyBuilder() 
                    .RequireAuthenticatedUser() 
                    .Build(); 
                options.Filters.Add(new AuthorizeFilter(policy)); 
            }).AddMicrosoftIdentityUI(); 
            services.AddRazorPages(); 
   ```
- Comment the below line of code 

  ```sh
  app.UseSession(); 
  ```
  Add below line of json in the appsettings.json 
  
  ```sh
   "TodoList": { 
    "TodoListScopes": "api://<API-ID>/ToDoList.Read api://<API-ID>/ToDoList.ReadWrite", 
    "TodoListBaseAddress": "https://localhost:44351" 
  }, 
  ```
