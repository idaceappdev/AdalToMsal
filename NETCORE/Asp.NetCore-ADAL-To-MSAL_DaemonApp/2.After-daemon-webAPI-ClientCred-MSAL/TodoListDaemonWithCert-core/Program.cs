/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

// The following using statements were added for this sample.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace TodoListDaemonWithCert
{
    class Program
    {
        private static AuthenticationConfig config;
        private static HttpClient httpClient = new HttpClient();

        IConfidentialClientApplication app;

        const string ClientId = "[Enter_client_ID_Of_ToDoListClient_from_Azure_Portal,_e.g._82692da5-a86f-44c9-9d53-2f88d52b478b]";
        const string authority
        = "https://login.microsoftonline.com/[Enter_tenant_name,_e.g._contoso.onmicrosoft.com].onmicrosoft.com";
        const string resourceId = "[Enter_client_ID_Of_ToDoListService_from_Azure_Portal,_e.g._82692da5-a86f-44c9-9d53-2f88d52b478b]";

        static void Main(string[] args)
        {
            // Create the authentication context to be used to acquire tokens.
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
                // .WithTenantId(specificTenant)
                // See https://aka.ms/msal.net/withTenantId
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

                // Call the To Do list service.
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
    }
}
