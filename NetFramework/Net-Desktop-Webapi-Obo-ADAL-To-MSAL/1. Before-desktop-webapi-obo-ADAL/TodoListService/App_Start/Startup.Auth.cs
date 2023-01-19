using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;

namespace TodoListService
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        SaveSigninToken = true,
                        ValidIssuer = ConfigurationManager.AppSettings["ida:Issuer"],
                        ValidAudiences = new List<string> { ConfigurationManager.AppSettings["ida:Audience"], ConfigurationManager.AppSettings["ida:ClientId"] },
                    }
                });
        }
    }
}
