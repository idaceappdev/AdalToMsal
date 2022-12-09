# Migration steps to be followed to migrate ASP.NET core version from 2.0 to .NET 6 which is the minimum supported version. 

This project had followed the insutruction from  https://learn.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-7.0&tabs=visual-studio to migrate the project to .NET 6 from asp.net core 2.2.  

## Changes needed on TodoListService project.  

- Right click on the project file and change the asp.net core target to .NET 6. 
- Remove the existing package references including asp.net core from the project except below NUGET 
  <ItemGroup> 
    <PackageReference Include="Microsoft.AspNetCore.Authentication.AzureAD.UI" Version="2.2.0" /> 
  </ItemGroup> 
