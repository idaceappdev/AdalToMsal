

# Nice to do things
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
