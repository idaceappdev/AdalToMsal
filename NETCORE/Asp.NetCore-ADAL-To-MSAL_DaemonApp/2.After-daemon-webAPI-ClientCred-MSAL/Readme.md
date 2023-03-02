# **Migration steps to be followed to migrate to MSAL from ADAL in .NET Core Daemon application calling Web API**

## **Migration guide for .NET based app:**
We have our migration guide for the .NET based application at https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-migration .
The current sample is in ASP.NET core 2.2.

As far as support is considered, the minimum supported .NET version is .NET 6. 
Please refer the link https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
Taking the above into consideration, we are going to demonstrate the approach of migrating the .NET core version to .NET 6 and then complete the ADAL to MSAL migration.

If not done already, retarget the target .NET framework of both ToDoListClient project and ToDoListService project from .NET Core 2.2 to .NET 6.0.

#### **Changes needed in TodoListClient project:**

- Remove the below NuGet package

		Microsoft.IdentityModel.Clients.ActiveDirectory

- Install below NuGet packages

		Microsoft.Identity.Client
		Microsoft.Identity.Web

- In file appsettings.json file, add the following under 'CertName' section and rename this to 'Certificate' as shown -

		{
			"SourceType": "StoreWithDistinguishedName",
			"CertificateStorePath": "<CERTIFICATE_STORE_PATH>",
			"CertificateDistinguishedName": "<CERTIFICATE_DISTINGUISHED_NAME>"
		}
		
  Your new appsettings.json file should look like this -
	
		{
			  "AADInstance": "https://login.microsoftonline.com/{0}",
			  "Tenant": "<tenant_name>.onmicrosoft.com",
			  "ClientId": "<Enter_ToDoListClient_ClientID>",
			  "TodoListResourceId": "<Enter_ToDoListService_ClientID>",
			  "TodoListBaseAddress": "https://localhost:44351/",
			  "Certificate": {
			   "SourceType": "StoreWithDistinguishedName",
			   "CertificateStorePath": "<CERTIFICATE_STORE_PATH>",
			   "CertificateDistinguishedName": "<CERTIFICATE_DISTINGUISHED_NAME>"
		  }
		}
		
#### **NOTE:**


The above approach can be used when you wish to retrieve a certificate from your local store. This configuration, shown above, is added to the Certificate property in the appsettings.json file in the daemon-console directory replacing <CERTIFICATE_STORE_PATH> with the store path to your certificate and <CERTIFICATE_STORE_PATH> with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. 
Other than this approach, you can also get your certificate from a specified file path or even from Azure key vault. The configuration would then respectively change into-

1. From file path-

				{
					// ... 
					"Certificate":  {
						"SourceType":  "Path",
						"CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
						"CertificatePassword":  "<CERTIFICATE_PASSWORD>"
					}
				}


2. From Key Vault-

				{
					// ... 
					"Certificate":  {
						"SourceType":  "KeyVault",
						"KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
						"KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
					}
				}

You can read more about certificate stores [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).

- In the class file Program.cs, make the following changes -

  * Make Program a non-static class
  
  * Remove or comment off below lines-
    
			using Microsoft.IdentityModel.Clients.ActiveDirectory;
			using System.Security.Cryptography.X509Certificates;
			…
			private static AuthenticationContext authContext = null;
			private static ClientAssertionCertificate certCred = null;
			…
			private static int errorCode;
			
   * Add below lines-
		
			using Microsoft.Identity.Client;
			using Microsoft.Identity.Web;
			…
			IConfidentialClientApplication app;
			…
			const string ClientId = "<Enter_ToDoListClient_ClientID>";
			const string authority = "https://login.microsoftonline.com/<tenant_name>.onmicrosoft.com";
			const string resourceId = "<Enter_ToDoListService_ClientID>";
			
    * Comment out the code under the Main() function, and replace with the following code -
			
			{
			           config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
			
			            {
			                try
			                {
			                    RunAsync().GetAwaiter().GetResult();
			                }
			                catch (Exception ex)
			                {
			                    Console.ForegroundColor = ConsoleColor.Red;
			                    Console.WriteLine(ex.Message);
			                    Console.ResetColor();
			                }
			
			                Console.WriteLine("Press any key to exit");
			                Console.ReadKey();
			            }
			}
      
     
      Also convert the return type of the method into void from int.
      
   * Comment the entire function 'ReadCertificateFromStore()' as we no longer need to read the certificate in this way.
   * Also comment out the GetAccessToken() method, PostNewTodoItem(), DisplayToDoList() methods as instead we will create an async task named RunAsync() with similar logic.
   * The async task RunAsync() would be defined with the following code -
   
			  static async Task<AuthenticationResult> RunAsync()
			        {
			            AuthenticationResult authResult = null;
			            Program p = new Program();
			
			            ICertificateLoader certificateLoader = new DefaultCertificateLoader();
			            certificateLoader.LoadIfNeeded(config.Certificate);
			
			            p.app = ConfidentialClientApplicationBuilder.Create(ClientId)
			            .WithCertificate(config.Certificate.Certificate)
			            .WithAuthority(authority)
			            .Build();
			
			            try
			            {
			                authResult = await p.app.AcquireTokenForClient(new[] { $"{resourceId}/.default" })
			                .ExecuteAsync()
			                .ConfigureAwait(false);
			            }
			
			            catch (MsalException ex)
			            {
			                Console.WriteLine(
			                        String.Format("An error occurred while acquiring a token\n"));
			            }
			
			            var httpClient = new HttpClient();
			            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
			            string timeNow = DateTime.Now.ToString();
			            string todoText = "Task at time: " + timeNow;
			
			            int delay = 1000;
			            for (int i = 0; i < 10; i++)
			            {
			
			                //Section of code that will POST to the todo list web api.
			                Console.WriteLine("Posting to To Do list at {0}", timeNow);
			                HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", todoText) });
			                HttpResponseMessage response = await httpClient.PostAsync(config.TodoListBaseAddress + "api/todolist", content);
			                if (response.IsSuccessStatusCode == true)
			                {
			                    Console.WriteLine("Successfully posted new To Do item:  {0}\n", todoText);
			                }
			                else
			                {
			                    Console.WriteLine("Failed to post a new To Do item\nError:  {0}\n", response.ReasonPhrase);
			                }
			
			                Thread.Sleep(delay);
			                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
			
			                //Call the To Do list service to retrieve items in the list.
			                Console.WriteLine("Retrieving To Do list at {0}", DateTime.Now.ToString());
			                response = await httpClient.GetAsync(config.TodoListBaseAddress + "api/todolist");
			
			                if (response.IsSuccessStatusCode)
			                {
			                    // Read the response and output it to the console.
			                    string s = await response.Content.ReadAsStringAsync();
			                    List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);
			                    foreach (TodoItem item in toDoArray)
			                    {
			                        Console.WriteLine(item.Title);
			                    }
			
			                    Console.WriteLine("Total item count:  {0}\n", toDoArray.Count);
			                }
			                else
			                {
			                    Console.WriteLine("Failed to retrieve To Do list\nError:  {0}\n", response.ReasonPhrase);
			                }
			            }
			            return authResult;
			        }
			
   * In the Authenticationconfig.cs file, add the following lines -
			
              using Microsoft.Identity.Web;
              …
              public string TodoListScope { get; set; }
              …

              //The description of the certificate to be used to authenticate your application.
        
              //<remarks>
              //Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
              //(which is a kind of application password: the property above)
              //or a certificate previously shared with AzureAD during the application registration 
              //(and identified by this CertificateDescription)
              //</remarks> 
			
              public CertificateDescription Certificate { get; set; }
			
#### **Changes needed in TodoListService project:**

The following changes should have been already done if you followed the doc explaining migration from .NET Core 2.2 version to .NET 6Vfor ToDoListService prject. If not, the following changes need to be incorporated in the service project. If the previous doc was followed, the below steps could be skipped if repetitive.

   * Remove the below NuGet packages:
   
            Microsoft.AspNetCore.All
            Microsoft.NetCore.App
            System.IdentityModel.Tokens.Jwt
            
     You can also reomve the DotNetCliToolReference from the .csproj file.
		
   * In Startup.cs file, add the below namespace (if not already added)-
   
            using Microsoft.Extensions.Hosting;

     Update the line-
                    
                    services.AddMvc();

     With-
                    
                    services.AddMvc(options =>
                                {
                                    options.Filters.Add<CustomExceptionFilter>();
                                });
                    services.AddControllers();

     In the Configure() method definition, change the second parameter from _IHostingEnvironment_ to _IWebHostEnvironment_.

     Also add the below lines of code just before and adding authentication middleware respectively,
     
                    app.UseRouting();
                    //Authentication middleware
                    app.UseAuthorization();

     And replace,
                    
                    app.UseMvc();
     
     With,
                    
                    app.UseEndpoints(endpoints =>
                        {
                             endpoints.MapControllers();
                        });
		
   * Add a new folder named 'Filters' and create a class within named _'CustomExceptionFilter.cs'_
	   
     Add the following code in that class -
	
              using Microsoft.AspNetCore.Http;
              using Microsoft.AspNetCore.Mvc.Filters;
              using System.Net;
              using System;
              namespace TodoListService
              {
                  public class CustomExceptionFilter : IExceptionFilter
                  {
                      public void OnException(ExceptionContext context)
                      {
                          HttpStatusCode status = HttpStatusCode.InternalServerError;
                          String message = String.Empty;
                          var exceptionType = context.Exception.GetType();
                          if (exceptionType == typeof(UnauthorizedAccessException))
                          {
                              message = "Unauthorized Access";
                              status = HttpStatusCode.Unauthorized;
                          }
                          else if (exceptionType == typeof(NotImplementedException))
                          {
                              message = "A server error occurred.";
                              status = HttpStatusCode.NotImplemented;
                          }
                          else
                          {
                              message = context.Exception.Message;
                              status = HttpStatusCode.NotFound;
                          }
                          context.ExceptionHandled = true;
                          HttpResponse response = context.HttpContext.Response;
                          response.StatusCode = (int)status;
                          response.ContentType = "application/json";
                          var err = message + " " + context.Exception.StackTrace;
                          response.WriteAsync(err);
                      }
                  }
		        }

Make sure to update the service project's appsettings.json file with the appropriate values of Domain name, tenantID and clientID.

## **Run the sample**

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

## **Steps to verify that app is using MSAL (From admin perspective)**

1. Get a network trace (e.g. using Fiddler) to observe the URL during sign-in which should redirect to v2 endpoint such as:
https://login.microsoftonline.com/<tenant_name>.onmicrosoft.com/oauth2/v2.0/token

2. Go to the sign-in logs in Azure AD, under non-interactive section and observe that, now we are reporting the MSAL version instead of ADAL, This confirms successful migration.

3. You would also observe the migration to MSAL if you have the workbooks solution configured in your environment as described [here](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-get-list-of-all-active-directory-auth-library-apps).

## **Steps to verify that app is using MSAL (From developer perspective)**

1. Search the entire project/solution to identify whether ADAL NUGET package is installed - The name of the package is `Microsoft.IdentityModel.Clients.ActiveDirectory`

2. Search the entire project/solution for the namespace using `Microsoft.IdentityModel.Clients.ActiveDirectory;`

3. Search the entire project/solution for the class `AuthenticationContext`

#### Quick Tips: 
1. It is recommended that, you unistall the ADAL NUGET package from the solution so that, Visual studio reports package missing errors for the code wherever ADAL namespace and AuthenticationContext classes are used. That makes it easier to review the ADAL related code. 
2. If you do not find the ADAL references in the entire solution and still if your app is using ADAL calls, that means, you have a reference to a dependency which uses the ADAL. In that case, you will have to reach out to dependency owners to make the code changes. 

