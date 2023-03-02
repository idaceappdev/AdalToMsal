# **Nice to do things on top of the project leveraging the MSAL.NET capabilities.**

## **Token cache serialization**
After Microsoft Authentication Library (MSAL) acquires a token, it caches that token. MSAL.NET provides default and custom serialization of the token cache. (https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet)

## **Proof Of Possession(PoP) tokens**
Bearer tokens are the norm in modern identity flows, however they are vulnerable to being stolen and used to access a protected resource. Please refer the link for more details (https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Proof-Of-Possession-(PoP)-tokens)

## **Logging**
The Microsoft Authentication Library (MSAL) apps generate log messages that can help diagnose issues. An app can configure logging with a few lines of code, and have custom control over the level of detail and whether or not personal and organizational data is logged. (https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging)
