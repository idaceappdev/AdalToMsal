---
services: azure-active-directory
platforms: javascript
author: soumi
---

# Azure AD Javascript Getting Started

> There's a newer version of this sample! Check it out: https://github.com/Azure-Samples/active-directory-javascript-singlepageapp-dotnet-webapi-v2
>
> This newer sample takes advantage of the Microsoft identity platform (formerly Azure AD v2.0).

This sample demonstrates the steps to migrate an the exisiting ADAL Javascript SPA sample to use MSAL.

## How To Run This Sample

Getting started is simple! To run this sample you will need:

- Visual Studio 2017
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1: Run the sample

Make sure you follow the steps mentioned in the `Before (With ADAL)` directory for the app to run successfully.

### Step 2: Add the dependency to include MSAL-Browser in this project.

Locate the `index.html` page and then add the following script tag above the following line `<script src="App/Scripts/app.js"></script>` to include the minified version of MSAL-Browser to your existing project.

    ```
    <!--Adding MSAL-Browser CDN-->
        <script type="text/javascript" src="https://alcdn.msauth.net/browser/2.32.2/js/msal-browser.min.js"></script>
    ```

### Step 3: Update the app.js file to include MSAL.

Locate the app.js file present under `App\Scripts\` and update the follow the steps below to migrate from ADAL to MSAL.

1.  Locate the following section and comment it out.

    ```
    // Enter Global Config Values & Instantiate ADAL AuthenticationContext
        //window.config = {
        //    instance: 'https://login.microsoftonline.com/',
        //    tenant: '<tenant>',
        //    clientId: '<Enter client-id or application-id>',
        //    postLogoutRedirectUri: window.location.origin,
        //    cacheLocation: 'localStorage' // enable this for IE, as sessionStorage does not work for localhost.
        //};
        //var authContext = new AuthenticationContext(config);
    ```

2.  Add the following code, to specify the config for MSAL object. Makesure to update the `<clientId>` section (with the app-id of your API's App registration) available under **apiConfig** object.

    ```
    // MSAL Code
        // Enter MSAL Config values
        window.config = {
            auth: {
                clientId: 'clientId',
                authority: "https://login.microsoftonline.com/<tenant>",
                redirectUri: window.location.origin,
                postLogoutRedirectUri: window.location.origin,
            },
            cache: {
                cacheLocation: 'localStorage'
            },
            system: {
                loggerOptions: {
                    loggerCallback: (level, message, containsPii) => {
                        if (containsPii) {
                            return;
                        }
                        switch (level) {
                            case msal.LogLevel.Error:
                                console.error(message);
                                return;
                            case msal.LogLevel.Info:
                                console.info(message);
                                return;
                            case msal.LogLevel.Verbose:
                                console.debug(message);
                                return;
                            case msal.LogLevel.Warning:
                                console.warn(message);
                                return;
                        }
                    },
                },
            },
        };

        /**
        * Scopes you add here will be prompted for user consent during sign-in.
        * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
        * For more information about OIDC scopes, visit:
        * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
        */
        window.loginRequest = {
            scopes: []
        };

        var apiConfig = {
            scopes: ["<clientId>/.default"]
        }

        window.tokenRequest = {
            scopes: [...apiConfig.scopes],
        };
    ```

3.  Create the MSAL PublicClientApplication Object and Instantiate it and add the userAccount variable.

    ```
    /*Instantiate MSAL PublicClientApplication Object*/
    var msalInstance = new msal.PublicClientApplication(config);

    // Declare the variables
    var userAccount = '';
    ```

4.  Check for the following code snippet and comment it out.

    ```
    // Check For & Handle Redirect From AAD After Login
        //ADAL Code
        var isCallback = authContext.isCallback(window.location.hash);
        authContext.handleWindowCallback();
        $errorMessage.html(authContext.getLoginError());

        if (isCallback && !authContext.getLoginError()) {
            window.location = authContext._getItem(authContext.CONSTANTS.STORAGE.LOGIN_REQUEST);
        }
    ```

5.  Add the following code snippet to set the default UI.

    ```
    // When using MSAL

        // Set the default UI
        $userDisplay.empty();
        $userDisplay.hide();
        $signInButton.show();
        $signOutButton.hide();
    ```

6.  Add the promise handler for handling the response received from the redirect flow.

    ```
    /**
    * A promise handler needs to be registered for handling the
    * response returned from redirect flow. For more information, visit:
    *
    */
    msalInstance.handleRedirectPromise()
        .then(handleResponse)
        .catch(error => {
            console.error(error);
        });
    ```

7.  Located and comment out the following section:

    ```
    // Check Login Status, Update UI
    var user = authContext.getCachedUser();
    if (user) {
        $userDisplay.html(user.userName);
        $userDisplay.show();
        $signInButton.hide();
        $signOutButton.show();
    } else {
        $userDisplay.empty();
        $userDisplay.hide();
        $signInButton.show();
        $signOutButton.hide();
    }
    ```

    then add the following functions your code.

    ```
    // Set the UI post user logs in using MSAL
        function handleUI(user) {
            $userDisplay.html(user.username);
            $userDisplay.show();
            $signInButton.hide();
            $signOutButton.show();
        }

        function selectAccount() {
            const currentAccounts = msalInstance.getAllAccounts();
            if (!currentAccounts) {
                return;
            } else if (currentAccounts.length > 1) {
                console.warn("Multiple accounts detected");
            } else if (currentAccounts.length === 1) {
                userAccount = currentAccounts[0];
                handleUI(userAccount);
            }
        }

        /*Handle Response post login*/
        function handleResponse(response) {
            if (!response) {
                selectAccount()
            } else {
                userAccount = response.account;
                console.log(userAccount)
                handleUI(userAccount);
            }
        }
    ```

8.  Update the signInButton and signOutButton code with the respective code snippets.
    **SignIn:**

    ```
    $signInButton.click(function () {
            //ADAL Code
            //authContext.login();

            //MSAL Code
            msalInstance.loginRedirect(loginRequest);
        });
    ```

    **SignOut:**

    ```
    $signOutButton.click(function () {
        //ADAL Code
        //authContext.logOut();

        //MSAL Code
        var logoutRequest = {
            account: msalInstance.getAccountByUsername(userAccount.username),
            postLogoutRedirectUri: config.auth.postLogoutRedirectUri
        }
        msalInstance.logoutRedirect(logoutRequest);
    });
    ```

9.  Locate the LoadView(view) method and comment the following section of the code.

    ```
    // Check if View Requires Authentication
    //ADAL Code
    if (ctrl.requireADLogin && !authContext.getCachedUser()) {
        authContext.config.redirectUri = window.location.href;
        authContext.login();
        return;
    }
    ```

    then add the following just after the comment ADAL code section.

    ```
    //MSAL Code
    if (ctrl.requireADLogin && !userAccount) {
        msalInstance.config.redirectUri = window.location.href;
        msalInstance.loginRedirect(loginRequest);
        return;
    }
    ```

### Step 4: Update the `userDataCrtl.js` file present under `App\Scripts\Ctrls\` to support MSAL by following the steps mentioned below:

1. Comment the Adal Context code snippet.

```
/ Instantiate the ADAL AuthenticationContext
var authContext = new AuthenticationContext(config);
```

2. Instantiate MSAL PublicClientApplication Object.

```
/*Instantiate MSAL PublicClientApplication Object*/
var msalInstance = new msal.PublicClientApplication(config);
```

3. Locate the following ADAL code inside `refreshDataView()` method and comment it.

```
// ADAL Code
for (var property in user.profile) {
   if (user.profile.hasOwnProperty(property)) {
      var $entry = $template;
       $entry.find(".view-data-claim").html(property);
       $entry.find(".view-data-value").html(user.profile[property]);
       output += $entry.html();
   }
}
```

4. Since MSAL doesnt not support the `user.profile` property instead, it profiles `user.idTokenClaims` that can be used to fetch the user properties. Add the following code snippet to list the user properties.

```
// MSAL Code
for (var property in user.idTokenClaims) {
   if (user.idTokenClaims.hasOwnProperty(property)) {
      var $entry = $template;
      $entry.find(".view-data-claim").html(property);
      $entry.find(".view-data-value").html(user.idTokenClaims[property]);
      output += $entry.html();
   }
}
```

### Step 5: Update the `todoListCrtl.js` file present under `App\Scripts\Ctrls\` to support MSAL by following the steps mentioned below:

1. Comment the Adal Context code snippet,

   ```
   / Instantiate the ADAL AuthenticationContext
   var authContext = new AuthenticationContext(config);
   ```

   and add instantiate MSAL PublicClientApplication Object.

   ```
   /*Instantiate MSAL PublicClientApplication Object*/
   var msalInstance = new msal.PublicClientApplication(config);
   ```

2. Locate the following ADAL code inside `refreshDataView()` method and comment it.

   ```
   // Acquire Token for Backend
   //ADAL Code
   authContext.acquireToken(authContext.config.clientId, function (error, token) {
        // Handle ADAL Error
        if (error || !token) {
            printErrorMessage('ADAL Error Occurred: ' + error);
            return;
        }

        // Get TodoList Data
        $.ajax({
            type: "GET",
            url: "/api/TodoList",
            headers: {
                'Authorization': 'Bearer ' + token,
            },
        }).done(function (data) {

            var $html = $(viewHTML);
            var $template = $html.find(".data-container");

            // For Each Todo Item Returned, Append a Table Row
            var output = data.reduce(function (rows, todoItem, index, todos) {
                var $entry = $template;
                var $description = $entry.find(".view-data-description").html(todoItem.Description);
                $entry.find(".data-template").attr('data-todo-id', todoItem.ID);
                return rows + $entry.html();
            }, '');

            // Update the UI
            $loading.hide();
            $dataContainer.html(output);

        }).fail(function () {
            printErrorMessage('Error getting todo list data')
        }).always(function () {

            // Register Handlers for Buttons in Data Table
            registerDataClickHandlers();
        });
    });
   ```

   then add the following MSAL code.

   ```
   // MSAL Code
   tokenRequest.account = msalInstance.getAllAccounts()[0];
   msalInstance.acquireTokenSilent(tokenRequest)
       .then(response => {
           console.log(response);
           if (!response.accessToken || response.accessToken === "") {
               throw new msalInstance.InteractionRequiredAuthError;
           } else {
               console.log("access_token acquired at: " + new Date().toString());
               token = response.accessToken;

               // Get TodoList Data
               $.ajax({
                   type: "GET",
                   url: "/api/TodoList",
                   headers: {
                       'Authorization': 'Bearer ' + token,
                   },
               }).done(function (data) {
                   var $html = $(viewHTML);
                   var $template = $html.find(".data-container");

                   // For Each Todo Item Returned, Append a Table Row
                   var output = data.reduce(function (rows, todoItem, index, todos) {
                       var $entry = $template;
                       var $description = $entry.find(".view-data-description").html(todoItem.Description);
                       $entry.find(".data-template").attr('data-todo-id', todoItem.ID);
                       return rows + $entry.html();
                   }, '');

                   // Update the UI
                   $loading.hide();
                   $dataContainer.html(output);
               }).fail(function () {
                   printErrorMessage('Error getting todo list data');
               }).always(function () {
                   //Register Handlers for Buttons in Data Table
                   registerDataClickHandlers();
               })
           }
       }).catch(error => {
           console.log("Silent token acquisition fails. Acquiring token using Redirect. \n", error);
           if (error instanceof msalInstance.InteractionRequiredAuthError) {
               // Need to check this part
               return msalInstance.acquireTokenRedirect(loginRequest);
           } else {
               console.log(error);
           }
       });
   };
   ```

3. Locate the following ADAL code for the **DELETE** endpoint inside `registerDataClickHandlers()` method and comment it.

   ```
   // Acquire Token for Backend
   //ADAL Code
   authContext.acquireToken(authContext.config.clientId, function (error, token) {

       // Handle ADAL Errors
       if (error || !token) {
           printErrorMessage('ADAL Error Occurred: ' + error);
           return;
       }

       // Delete the Todo
       $.ajax({
           type: "DELETE",
           url: "/api/TodoList/" + todoId,
           headers: {
               'Authorization': 'Bearer ' + token,
           },
       }).done(function () {
           console.log('DELETE success.');
       }).fail(function () {
           console.log('Fail on new Todo DELETE');
           printErrorMessage('Error deleting todo item.')
       }).always(function () {
           refreshViewData();
       });
   });
   ```

   then add the following MSAL code snippet to make the **DELETE** endpoint to work..

   ```
   //MSAL Code
   tokenRequest.account = msalInstance.getAllAccounts()[0];
   msalInstance.acquireTokenSilent(tokenRequest)
       .then(response => {
           console.log(response);
           if (!response.accessToken || response.accessToken === "") {
               throw new msalInstance.InteractionRequiredAuthError;
           } else {
               console.log("access_token acquired at: " + new Date().toString());
               token = response.accessToken;

               // Delete the Todo
               $.ajax({
                   type: "DELETE",
                   url: "/api/TodoList/" + todoId,
                   headers: {
                       'Authorization': 'Bearer ' + token,
                   },
               }).done(function () {
                   console.log('DELETE success.');
               }).fail(function () {
                   console.log('Fail on new Todo DELETE');
                   printErrorMessage('Error deleting todo item.');
               }).always(function () {
                   // Refresh TodoList
                   refreshViewData();
               })
           }
       }).catch(error => {
           console.log("Silent token acquisition fails. Acquiring token using Redirect. \n", error);
           if (error instanceof msalInstance.InteractionRequiredAuthError) {
               // Need to check this part
               return msalInstance.acquireTokenRedirect(loginRequest);
           } else {
               console.log(error);
           }
       });
   ```

4. Locate the following ADAL code for the **PUT** endpoint (To edit TodoList items) inside `registerDataClickHandlers()` method and comment it.

   ```
   // Acquire Token for Backend
   //ADAL Code
   authContext.acquireToken(authContext.config.clientId, function (error, token) {

       // Handle ADAL Errors
       if (error || !token) {
           printErrorMessage('ADAL Error Occurred: ' + error);
           return;
       }

       // Update Todo Item
       $.ajax({
           type: "PUT",
           url: "/api/TodoList",
           headers: {
               'Authorization': 'Bearer ' + token,
           },
           data: {
               Description: $description.val(),
               ID: todoId,
           },
       }).done(function () {
           console.log('PUT success.');
       }).fail(function () {
           console.log('Fail on todo PUT');
           printErrorMessage('Error saving todo item.')
       }).always(function () {
           refreshViewData();
           $description.val('');
       });
   });
   ```

   and add the following MSAL code snippet to make the **PUT** endpoint to work.

   ```
   // MSAL Code
   tokenRequest.account = msalInstance.getAllAccounts()[0];
   msalInstance.acquireTokenSilent(tokenRequest)
       .then(response => {
           console.log(response);
           if (!response.accessToken || response.accessToken === "") {
               throw new msalInstance.InteractionRequiredAuthError;
           } else {
               console.log("access_token acquired at: " + new Date().toString());
               token = response.accessToken;

               // Edit TodoList Data
               $.ajax({
                   type: "PUT",
                   url: "/api/TodoList",
                   headers: {
                       'Authorization': 'Bearer ' + token,
                   },
                   data: {
                       Description: $description.val(),
                       ID: todoId,
                   },
               }).done(function () {
                   console.log('PUT success.');
               }).fail(function () {
                   console.log('Fail on todo PUT');
                   printErrorMessage('Error saving todo item.');
               }).always(function () {
                   // Refresh TodoList
                   $description.val('');
                   refreshViewData();
               })
           }
       }).catch(error => {
           console.log("Silent token acquisition fails. Acquiring token using Redirect. \n", error);
           if (error instanceof msalInstance.InteractionRequiredAuthError) {
               // Need to check this part
               return msalInstance.acquireTokenRedirect(loginRequest);
           } else {
               console.log(error);
           }
       });
   ```

5. Locate the following ADAL code inside `registerViewClickHandlers()` method and comment it,

   ```
   // Acquire Token for Backend
   //ADAL Code
   authContext.acquireToken(authContext.config.clientId, function (error, token) {

       // Handle ADAL Errors
       if (error || !token) {
           printErrorMessage('ADAL Error Occurred: ' + error);
           return;
       }

       // POST a New Todo
       $.ajax({
           type: "POST",
           url: "/api/TodoList",
           headers: {
               'Authorization': 'Bearer ' + token,
           },
           data: {
               Description: $description.val(),
           },
       }).done(function () {
           console.log('POST success.');
       }).fail(function () {
           console.log('Fail on new Todo POST');
           printErrorMessage('Error adding new todo item.');
       }).always(function () {

           // Refresh TodoList
           $description.val('');
           refreshViewData();
       });
   });
   ```

   and then add the following MSAL code snippet for the POST endpoint to work.

   ```
   // MSAL Code
   tokenRequest.account = msalInstance.getAllAccounts()[0];
   msalInstance.acquireTokenSilent(tokenRequest)
       .then(response => {
           console.log(response);
           if (!response.accessToken || response.accessToken === "") {
               throw new msalInstance.InteractionRequiredAuthError;
           } else {
               console.log("access_token acquired at: " + new Date().toString());
               token = response.accessToken;

               // Add TodoList Data
               $.ajax({
                   type: "POST",
                   url: "/api/TodoList",
                   headers: {
                       'Authorization': 'Bearer ' + token,
                   },
                   data: {
                       Description: $description.val(),
                   },
               }).done(function () {
                   console.log('POST success.');
               }).fail(function () {
                   console.log('Fail on new Todo POST');
                   printErrorMessage('Error adding new todo item.');
               }).always(function () {
                   // Refresh TodoList
                   $description.val('');
                   refreshViewData();
               })
           }
       }).catch(error => {
           console.log("Silent token acquisition fails. Acquiring token using Redirect. \n", error);
           if (error instanceof msalInstance.InteractionRequiredAuthError) {
               // Need to check this part
               return msalInstance.acquireTokenRedirect(loginRequest);
           } else {
               console.log(error);
           }
       });
   ```

## About the Code

The key files containing authentication logic are the following:

**App.js** - Provides the app configuration values used by ADAL for driving protocol interactions with AAD, indicates which routes should not be accessed without previous authentication, issues login and logout requests to Azure AD, handles both successful and failed authentication callbacks from Azure AD, and displays information about the user received in the id_token.

**index.html** - contains a reference to adal.js.

**todoListCtrl.js**- shows how to take advantage of the acquireToken() method in ADAL to get a token for accessing a resource.

**userDataCtrl.js** - shows how to extract user information from the cached id_token.
