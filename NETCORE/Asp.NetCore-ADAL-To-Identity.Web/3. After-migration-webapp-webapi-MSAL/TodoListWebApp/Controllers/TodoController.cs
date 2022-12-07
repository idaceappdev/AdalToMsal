using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using TodoListWebApp.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoListWebApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        // GET: /<controller>/

        private readonly ITokenAcquisition _tokenAcquisition;
        IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TodoController(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _httpClient = httpClient;
        }
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
        public async Task<IActionResult> Index()
        {
           // AuthenticationResult result = null;
            List<TodoItem> itemList = new List<TodoItem>();

            try
            {
                await PrepareAuthenticatedClient();

                // Because we signed-in already in the WebApp, the userObjectId is know
                //string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

                //// Using ADAL.Net, get a bearer token to access the TodoListService
                //AuthenticationContext authContext = new AuthenticationContext(AzureAdOptions.Settings.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
                //ClientCredential credential = new ClientCredential(AzureAdOptions.Settings.ClientId, AzureAdOptions.Settings.ClientSecret);
                //result = await authContext.AcquireTokenSilentAsync(AzureAdOptions.Settings.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //// Retrieve the user's To Do List.
                //HttpClient client = new HttpClient();
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AzureAdOptions.Settings.TodoListBaseAddress + "/api/todolist");
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await _httpClient.GetAsync(_configuration["TodoList:TodoListBaseAddress"] + "/api/todolist");

                // Return the To Do List in the view.
                if (response.IsSuccessStatusCode)
                {
                    List<Dictionary<String, String>> responseElements = new List<Dictionary<String, String>>();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    String responseString = await response.Content.ReadAsStringAsync();
                    responseElements = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(responseString, settings);
                    foreach (Dictionary<String, String> responseElement in responseElements)
                    {
                        TodoItem newItem = new TodoItem();
                        newItem.Title = responseElement["title"];
                        newItem.Owner = responseElement["owner"];
                        itemList.Add(newItem);
                    }

                    return View(itemList);
                }

                //
                // If the call failed with access denied, then drop the current access token from the cache, 
                //     and show the user an error indicating they might need to sign-in again.
                //
                //if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                //{
                //    return ProcessUnauthorized(itemList, authContext);
                //}
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (HttpContext.Request.Query["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                TodoItem newItem = new TodoItem();
                newItem.Title = "(Sign-in required to view to do list.)";
                itemList.Add(newItem);
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(itemList);
            }

            //
            // If the call failed for any other reason, show the user an error.
            //
            // return View("Error");
        }

        [HttpPost]
        public async Task<ActionResult> Index(string item)
        {
            if (ModelState.IsValid)
            {
                //
                // Retrieve the user's tenantID and access token since they are parameters used to call the To Do service.
                //
                //  AuthenticationResult result = null;
                List<TodoItem> itemList = new List<TodoItem>();

                try
                {
                    //string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
                    //AuthenticationContext authContext = new AuthenticationContext(AzureAdOptions.Settings.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
                    //ClientCredential credential = new ClientCredential(AzureAdOptions.Settings.ClientId, AzureAdOptions.Settings.ClientSecret);
                    //result = await authContext.AcquireTokenSilentAsync(AzureAdOptions.Settings.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                    //// Forms encode todo item, to POST to the todo list web api.
                    //HttpContent content = new StringContent(JsonConvert.SerializeObject(new { Title = item }), System.Text.Encoding.UTF8, "application/json");

                    ////
                    //// Add the item to user's To Do List.
                    ////
                    //HttpClient client = new HttpClient();
                    //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, AzureAdOptions.Settings.TodoListBaseAddress + "/api/todolist");
                    //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    //request.Content = content;
                    await PrepareAuthenticatedClient();

                    var jsonRequest = JsonConvert.SerializeObject(new { Title = item });
                    var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(_configuration["TodoList:TodoListBaseAddress"] + "/api/todolist", jsoncontent);

                    //
                    // Return the To Do List in the view.
                    //
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }

                    //
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    //     and show the user an error indicating they might need to sign-in again.
                    //
                    //if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    //{
                    //    return ProcessUnauthorized(itemList, authContext);
                    //}

                    return null;
                }
                catch (Exception)
                {
                    //
                    // The user needs to re-authorize.  Show them a message to that effect.
                    //
                    TodoItem newItem = new TodoItem();
                    newItem.Title = "(No items in list)";
                    itemList.Add(newItem);
                    ViewBag.ErrorMessage = "AuthorizationRequired";
                    return View(itemList);
                }
                //
                // If the call failed for any other reason, show the user an error.
                //
                // return View("Error");
            }
            return View("Error");
        }

        //private ActionResult ProcessUnauthorized(List<TodoItem> itemList, AuthenticationContext authContext)
        //{
        //    var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == AzureAdOptions.Settings.TodoListResourceId);
        //    foreach (TokenCacheItem tci in todoTokens)
        //        authContext.TokenCache.DeleteItem(tci);

        //    ViewBag.ErrorMessage = "UnexpectedError";
        //    TodoItem newItem = new TodoItem();
        //    newItem.Title = "(No items in list)";
        //    itemList.Add(newItem);
        //    return View(itemList);
        //}
    }
}
