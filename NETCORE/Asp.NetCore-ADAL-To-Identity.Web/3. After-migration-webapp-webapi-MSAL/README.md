# Migration steps to be followed to migrate to Identity.Web(MSAL) and from ADAL in .NET 6

## Changes needed in TodoListWebApp project

- Remove the below package reference  
   
   ```sh
   <ItemGroup> 
    <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
    <PackageReference Include="Microsoft.IdentityModel.Clients.ActiveDirectory" Version="5.3.0" /> 
  </ItemGroup> 
    ```
 - Install below NUGET packages 
   - Microsoft.Identity.Web.UI
   - Newtonsoft.Json
 - Comment all the code from the below files  
   - AzureAdAuthenticationBuilderExtensions.cs
   - AzureAdOptions.cs
   - NaiveSessionCache.cs
   - AccountController.cs
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
- Add below line of json in the appsettings.json 
  
  ```sh
   "TodoList": { 
    "TodoListScopes": "api://<API-ID>/user_impersonation", 
    "TodoListBaseAddress": "https://localhost:44351" 
  }, 
  ```
- Remove the below line of code to get rid of adal reference
   ```sh
   using Microsoft.IdentityModel.Clients.ActiveDirectory; 
  ```
- Declare the below variables in the TodoController class
  
  ```sh
   private readonly ITokenAcquisition _tokenAcquisition;
   IConfiguration _configuration;
   private readonly HttpClient _httpClient; 
  ```
- Declare the below constructors in the TodoController class
  
  ```sh
   public TodoController(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration)
     {
         _tokenAcquisition = tokenAcquisition;
         _configuration = configuration;
         _httpClient = httpClient;
     }
  ```
- Declare the below method in the TodoController class
  
  ```sh
    private async Task PrepareAuthenticatedClient() 
        { 
            //You would specify the scopes (delegated permissions) here for which you desire an Access token of this API from Azure AD. 
            //Note that these scopes can be different from what you provided in startup.cs. 
            //The scopes provided here can be different or more from the ones provided in Startup.cs. Note that if they are different, 
            //then the user might be prompted to consent again. 
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new List<string>()); 
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); 
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
        } 
  ```
- Comment the below line of code from get Index method
 
   ```sh
   AuthenticationResult result = null;
  ```
- Comment the below line of code in the get method of index

  ```sh 
  string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

                // Using ADAL.Net, get a bearer token to access the TodoListService
                AuthenticationContext authContext = new AuthenticationContext(AzureAdOptions.Settings.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
                ClientCredential credential = new ClientCredential(AzureAdOptions.Settings.ClientId, AzureAdOptions.Settings.ClientSecret);
                result = await authContext.AcquireTokenSilentAsync(AzureAdOptions.Settings.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                // Retrieve the user's To Do List.
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AzureAdOptions.Settings.TodoListBaseAddress + "/api/todolist");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);               
  ```
- In place of the above code, please paste the below snippet.
  
  ```sh
   await PrepareAuthenticatedClient();
   HttpResponseMessage response = await _httpClient.GetAsync(_configuration["TodoList:TodoListBaseAddress"] + "/api/todolist");
  ```
- Remove the reference of Authentication Context as shown in the below code snippet.
   ```sh
   if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return ProcessUnauthorized(itemList);
                    }
  ```

- Modify the **ProcessUnauthorized** method definition as below

   ```sh
   private ActionResult ProcessUnauthorized(List<TodoItem> itemList)
        {
            //var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == AzureAdOptions.Settings.TodoListResourceId);
            //foreach (TokenCacheItem tci in todoTokens)
            //    authContext.TokenCache.DeleteItem(tci);

            ViewBag.ErrorMessage = "UnexpectedError";
            TodoItem newItem = new TodoItem();
            newItem.Title = "(No items in list)";
            itemList.Add(newItem);
            return View(itemList);
        }
  ```

- Repeat the same for the post method of index

- Comment the below line of code from Post Index method
 
  ```sh
   AuthenticationResult result = null;
  ```
- Comment the below line of code in the get method of index

  ```sh 
   string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
                    AuthenticationContext authContext = new AuthenticationContext(AzureAdOptions.Settings.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
                    ClientCredential credential = new ClientCredential(AzureAdOptions.Settings.ClientId, AzureAdOptions.Settings.ClientSecret);
                    result = await authContext.AcquireTokenSilentAsync(AzureAdOptions.Settings.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                    // Forms encode todo item, to POST to the todo list web api.
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { Title = item }), System.Text.Encoding.UTF8, "application/json");

                    //
                    // Add the item to user's To Do List.
                    //
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, AzureAdOptions.Settings.TodoListBaseAddress + "/api/todolist");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    request.Content = content;
                    HttpResponseMessage response = await client.SendAsync(request);
        
  ```
- In place of the above code, please paste the below snippet.
  
  ```sh
   await PrepareAuthenticatedClient();
                    var jsonRequest = JsonConvert.SerializeObject(new { Title = item });
                    var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync(_configuration["TodoList:TodoListBaseAddress"] + "/api/todolist", jsoncontent);

  ```
- Remove the reference of Authentication Context as shown in the below code snippet.
   ```sh
   if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return ProcessUnauthorized(itemList);
                    }
  ```
 
- Specify the asp are path as MicrosoftIdentity under the views/shared folder in the file _LoginPartial,cshtml 
   
   Update asp-area value to **MicrosoftIdentity** instead of blank
  
- Build the project and run it .

## Steps to verify that app is using MSAL.

- Observe the URL during sign-in which should redirect to v2 endpoint 
  
  ```sh
   https://login.microsoftonline.com/<Tenant-Id>/oauth2/v2.0/authorize?client_id=<Client-Id>&redirect_uri=https%3a%2f%2flocalhost%3a44377%2fsignin-oidc&response_type=code&scope=openid+profile+offline_access+api%3 
   ```
 
- Go to the sign-in logs under non-interactive section and observe that, now we are reporting the MSAL version instead of ADAL, This confirms successful migration. Please do note that, interactive log may report blank or ASP.NET core module as authetication is not handled by MSAL/Identity.Web in asp.net core. It is the access token request which is implemented by the MSAL and Identity.Web.  

   ![image](https://user-images.githubusercontent.com/62542910/206981202-2b086f5f-e28e-4ac3-b7d6-ad745b25df82.png)
