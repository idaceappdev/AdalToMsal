# Migration steps to be followed to migrate to MSAL from ADAL in .NET Framework 4.8

## Changes needed in TodoListClient project

- Remove the below NuGet packages
   - Microsoft.IdentityModel.Clients.ActiveDirectory

 - Install below NuGet packages 
   - Microsoft.Identity.Client

 - In file App.config
   - Remove or comment off below lines
   ```sh
   <add key="ida:RedirectUri" value=""/>
   <add key="todo:TodoListResourceId" value=""/> 
   ```

   - Add below lines, note {TodoListService_Client_ID} is Client ID of TodoListService-OBO
   ```sh
   <add key="todo:TodoListScope" value="api://{TodoListService_Client_ID}/.default"/>
   ```

 - In file MainWindow.xaml.cs - initial part
   - Remove or comment off below lines
   ```sh
   using System.IdentityModel;
   ...
   Uri redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);
   ...
   private static string todoListResourceId = ConfigurationManager.AppSettings["todo:TodoListResourceId"];
   ...
   private AuthenticationContext authContext = null;

   ```

   - Add below lines
   ```sh
   using Microsoft.Identity.Client;
   ...
   private static string TodoListScope = ConfigurationManager.AppSettings["todo:TodoListScope"];
   private static readonly string[] Scopes = { TodoListScope };
   ...
   private readonly IPublicClientApplication _app;
   ```

 - In file MainWindow.xaml.cs - MainWindow()
   - Remove or comment off below lines
   ```sh
   authContext = new AuthenticationContext(authority, new FileCache());

   ```

   - Add below lines (at the position where above lines removed)
   ```sh
   _app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(authority)
                .WithDefaultRedirectUri()
                .Build();
   ```

 - In file MainWindow.xaml.cs - GetTodoListAsync(bool isAppStarting)
   - Remove or comment off below lines
   ```sh
            try
            {
                result = await authContext.AcquireTokenSilentAsync(todoListResourceId, clientId);
                SignInButton.Content = clearCacheString;
                this.SetUserName(result.UserInfo);
            }
            catch (AdalException ex)
            {
                // There is no access token in the cache, so prompt the user to sign-in.
                if (ex.ErrorCode == AdalError.UserInteractionRequired || ex.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    if (!isAppStarting)
                    {
                        MessageBox.Show("Please sign in to view your To-Do list");
                        SignInButton.Content = signInString;
                    }
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                    }
                    MessageBox.Show(message);
                }

                UserName.Content = Properties.Resources.UserNotSignedIn;

                return;
            }

   ```

   - Add below lines (at the position where above lines removed)
   ```sh
            try
            {
                var accounts = (await _app.GetAccountsAsync()).ToList();
                if (!accounts.Any())
                {
                    SignInButton.Content = signInString;
                    return;
                }

                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(
                    () =>
                    {
                        SignInButton.Content = clearCacheString;
                        SetUserName(result.Account);
                    });
            }
            catch (MsalUiRequiredException)
            {
                if (!isAppStarting)
                {
                    MessageBox.Show("Please sign in to view your To-Do list");
                    SignInButton.Content = signInString;
                }
                return;
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                MessageBox.Show(message);

                UserName.Content = Properties.Resources.UserNotSignedIn;
                return;
            }
   ```

 - In file MainWindow.xaml.cs - AddTodoItemAsync()
   - Remove or comment off below lines
   ```sh
            AuthenticationResult result = null;
            try
            {
                result = await authContext.AcquireTokenSilentAsync(todoListResourceId, clientId);
                this.SetUserName(result.UserInfo);
            }
            catch (AdalException ex)
            {
                // There is no access token in the cache, so prompt the user to sign-in.
                if (ex.ErrorCode == AdalError.UserInteractionRequired || ex.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    MessageBox.Show("Please sign in first");
                    SignInButton.Content = signInString;
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                UserName.Content = Properties.Resources.UserNotSignedIn;

                return;
            }

   ```

   - Add below lines (at the position where above lines removed)
   ```sh
            var accounts = (await _app.GetAccountsAsync()).ToList();
            if (!accounts.Any())
            {
                MessageBox.Show("Please sign in first");
                return;
            }

            //
            // Get an access token to call the To Do service.
            //
            AuthenticationResult result = null;
            try
            {
                //Calling MSAL to acquire an access token with the scope 'access_as_user' for the logged user IAccount
                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                SetUserName(result.Account);
            }
            catch (MsalUiRequiredException)
            {
                MessageBox.Show("Please re-sign");
                SignInButton.Content = signInString;
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }

                Dispatcher.Invoke(() =>
                {
                    UserName.Content = Properties.Resources.UserNotSignedIn;
                    MessageBox.Show("Unexpected error: " + message);
                });

                return;
            }
   ```

 - In file MainWindow.xaml.cs - SignIn()
   - Remove or comment off below lines
   ```sh
            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == clearCacheString)
            {
                TodoList.ItemsSource = string.Empty;
                authContext.TokenCache.Clear();
                // Also clear cookies from the browser control.
                SignInButton.Content = signInString;
                UserName.Content = Properties.Resources.UserNotSignedIn;
                return;
            }

            //
            // Get an access token to call the To Do list service.
            //
            AuthenticationResult result = null;
            try
            {
                // Force a sign-in (PromptBehavior.Always), as the ADAL web browser might contain cookies for the current user, and using .Auto
                // would re-sign-in the same user
                result = await authContext.AcquireTokenAsync(todoListResourceId, clientId, redirectUri, new PlatformParameters(PromptBehavior.Always));
                SignInButton.Content = clearCacheString;
                SetUserName(result.UserInfo);
                GetTodoList();
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "access_denied")
                {
                    // The user canceled sign in, take no action.
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                UserName.Content = Properties.Resources.UserNotSignedIn;

                return;
            }

   ```

   - Add below lines
   ```sh
            var accounts = (await _app.GetAccountsAsync()).ToList();

            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == clearCacheString)
            {
                TodoList.ItemsSource = string.Empty;

                // clear the cache
                while (accounts.Any())
                {
                    await _app.RemoveAsync(accounts.First());
                    accounts = (await _app.GetAccountsAsync()).ToList();
                }

                // Also clear cookies from the browser control.
                SignInButton.Content = signInString;
                UserName.Content = Properties.Resources.UserNotSignedIn;
                return;
            }

            AuthenticationResult result = null;
            try
            {
                // Force a sign-in (PromptBehavior.Always), as the MSAL web browser might contain cookies for the current user
                result = await _app.AcquireTokenInteractive(Scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    SignInButton.Content = clearCacheString;
                    SetUserName(result.Account);
                    GetTodoList();
                });
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "access_denied")
                {
                    // The user canceled sign in, take no action.
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                Dispatcher.Invoke(() =>
                {
                    UserName.Content = Properties.Resources.UserNotSignedIn;
                });
            }
   ```

 - In file MainWindow.xaml.cs - SetUserName()
   - Change this function to below
   ```sh
        private void SetUserName(IAccount userInfo)
        {
            string userName = null;

            if (userInfo != null)
            {
                userName = userInfo.Username ?? userInfo.HomeAccountId.ObjectId;

                if (userName == null)
                    userName = Properties.Resources.UserNotIdentified;

                UserName.Content = userName;
            }
        }
   ```

 - In file FileCache.cs
   - Remove the file. (Token cache serialization is a separate topic)


## Changes needed in TodoListService project

- Remove the below NuGet packages
   - Microsoft.IdentityModel.Clients.ActiveDirectory

 - Install below NuGet packages 
   - Microsoft.Identity.Client

 - In file web.config
   - Remove or comment off below lines
   ```sh
   <add key="ida:GraphResourceId" value="https://graph.microsoft.com"/> 
   ```

   - Add below lines
   ```sh
   <add key="ida:RedirectUri" value="https://localhost:44321/" />
   ```

 - In file TodoListController.cs - initial part
   - Remove or comment off below lines
   ```sh
   using Microsoft.IdentityModel.Clients.ActiveDirectory;
   ...
   private static string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];

   ```

   - Add below lines
   ```sh
   using Microsoft.Identity.Client;
   ...
   private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

   ```

 - In file TodoListController.cs - CallGraphAPIOnBehalfOfUser()
   - Remove or comment off below lines
   ```sh
            //
            // Use ADAL to get a token On Behalf Of the current user.  To do this we will need:
            //      The Resource ID of the service we want to call.
            //      The current user's access token, from the current request's authorization header.
            //      The credentials of this application.
            //      The username (UPN or email) of the user calling the API
            //
            ClientCredential clientCred = new ClientCredential(clientId, appKey);
            string userAccessToken = (string)ClaimsPrincipal.Current.Identities.First().BootstrapContext;
            UserAssertion userAssertion = new UserAssertion(userAccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer", null);

            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            AuthenticationContext authContext = new AuthenticationContext(authority, new DbTokenCache(userId));

            // In the case of a transient error, retry once after 1 second, then abandon.
            // Retrying is optional.  It may be better, for your application, to return an error immediately to the user and have the user initiate the retry.
            bool retry = false;
            int retryCount = 0;

            do
            {
                retry = false;
                try
                {
                    result = await authContext.AcquireTokenAsync(graphResourceId, clientCred, userAssertion);
                    accessToken = result.AccessToken;
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == SERVICE_UNAVAILABLE)
                    {
                        // Transient error, OK to retry.
                        retry = true;
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                }
            } while ((retry == true) && (retryCount < 2));

   ```

   - Add below lines (at the position where above lines removed)
   ```sh
            string[] scopes = { "user.read" };

            try
            {
                string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

                // Creating a ConfidentialClientApplication using the Build pattern (https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications)
                var app = ConfidentialClientApplicationBuilder.Create(clientId)
                   .WithAuthority(authority)
                   .WithClientSecret(appKey)
                   .WithRedirectUri(redirectUri)
                   .Build();

                // Hooking MSALPerUserSqlTokenCacheProvider class on ConfidentialClientApplication's UserTokenCache.
                //MSALPerUserSqlTokenCacheProvider sqlCache = new MSALPerUserSqlTokenCacheProvider(app.UserTokenCache, dbContext, ClaimsPrincipal.Current);

                //Grab the Bearer token from the HTTP Header using the identity bootstrap context. This requires SaveSigninToken to be true at Startup.Auth.cs
                var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext.ToString();

                // Creating a UserAssertion based on the Bearer token sent by TodoListClient request.
                //urn:ietf:params:oauth:grant-type:jwt-bearer is the grant_type required when using On Behalf Of flow: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow
                UserAssertion userAssertion = new UserAssertion(bootstrapContext, "urn:ietf:params:oauth:grant-type:jwt-bearer");

                // Acquiring an AuthenticationResult for the scope user.read, impersonating the user represented by userAssertion, using the OBO flow
                result = await app.AcquireTokenOnBehalfOf(scopes, userAssertion)
                    .ExecuteAsync();

                accessToken = result.AccessToken;
                
            }
            catch (MsalUiRequiredException msalServiceException)
            {
                /*
                * If you used the scope `.default` on the client application, the user would have been prompted to consent for Graph API back there
                * and no incremental consents are required (this exception is not expected). However, if you are using the scope `access_as_user`,
                * this exception will be thrown at the first time the API tries to access Graph on behalf of the user for an incremental consent.
                * You must then, add the logic to delegate the consent screen to your client application here.
                * This sample doesn't use the incremental consent strategy.
                */
                throw msalServiceException;
            }
            catch (Exception ex)
            {
                throw ex;
            }

   ```

 - In file DAL/DbTokenCache.cs
   - Remove this file (Token cache serialization is a separate topic)

 - In file DAL/TodoListServiceContext.cs
   - Remove or comment off below lines
   ```sh
   public DbSet<PerWebUserCache> PerUserCacheList { get; set; }
   ```

 - In Visual studio, View -> Server Explorer, 
   - Connect to connection TodoLIstServiceContext(TodoListService), expand Tables, if there is a table _MigrationsHistory, right click on the table, choose 'delete', then click on 'Update database'. Because DB context changed, this is to avoid the error "The model backing the 'TodoLIstServiceContext' context has changed since the database was created."

- Build the project and run it .

## Steps to verify that app is using MSAL.

- Get network trace (e.g. using Fiddler) to observe the URL during sign-in which should redirect to v2 endpoint 
  
  ```sh
   https://login.microsoftonline.com/<Tenant-Id>/oauth2/v2.0/authorize?client_id=<Client-Id>&redirect_uri=<Redirect-Uri>&response_type=code&scope=openid+profile+offline_access+api
   ```
 
- Go to the sign-in logs under non-interactive section and observe that, now we are reporting the MSAL version instead of ADAL, This confirms successful migration.

   