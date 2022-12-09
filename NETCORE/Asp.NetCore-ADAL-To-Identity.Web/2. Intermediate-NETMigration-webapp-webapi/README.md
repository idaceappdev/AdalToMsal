# Migration steps to be followed to migrate ASP.NET core version from 2.0 to .NET 6 which is the minimum supported version. 

This project had followed the insutruction from  https://learn.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-7.0&tabs=visual-studio to migrate the project to .NET 6 from asp.net core 2.2.  

## Changes needed on TodoListService project.  

- Right click on the project file and change the asp.net core target to .NET 6. 
- Remove the existing package references including asp.net core from the project except below NUGET 

   ```sh
  <ItemGroup> 
      <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
    </ItemGroup> 
  ```
- Now there are certain manual changes that need to be made to run the project in .NET 6.  
- Go to the startup class of the project and add below namespace  

   ```sh
  using Microsoft.Extensions.Hosting; 
  ```
  Replace **IHostingEnvironment** interface by **IWebHostEnvironment **
- Replace the below line of code in the Startup.cs in the project  
 
  ```sh
  app.UseMvc(); 
  ```
 
   By

    ```sh
     app.UseEndpoints(endpoints => 
              { 
                  endpoints.MapControllers(); 
              }); 
    ```
  Replace the below line of code  
  
  ```sh
  services.AddMvc();  
  ```
  By
  
   ```sh
     services.AddMvc(options => 
            { 
                options.Filters.Add<CustomExceptionFilter>(); 
            }); 
    services.AddControllers(); 
    ```
    You may see compiler complaining about **CustomExceptionFilter** class. Create a filters folder in the project and add below new class  
    
     ```sh
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
    ```
 - Use the below line of code just before adding authentication middleware. 
  
  ```sh
  app.UseRouting(); 
  ```
  After Authentication middleware, please add the authorization middleware too 
  
  ```sh
  app.UseAuthorization(); 
  ```
- Compile the project and it should build without any errors.  
