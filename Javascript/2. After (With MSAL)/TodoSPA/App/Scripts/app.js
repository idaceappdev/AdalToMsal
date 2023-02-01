
(function () {

    // Enter Global Config Values & Instantiate ADAL AuthenticationContext
    //window.config = {
    //    instance: 'https://login.microsoftonline.com/',
    //    tenant: '<tenant>',
    //    clientId: '<Enter client-id or application-id>',
    //    postLogoutRedirectUri: window.location.origin,
    //    cacheLocation: 'localStorage' // enable this for IE, as sessionStorage does not work for localhost.
    //};
    //var authContext = new AuthenticationContext(config);

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

    /*Instantiate MSAL PublicClientApplication Object*/
    var msalInstance = new msal.PublicClientApplication(config);

    // Declare the variables
    var userAccount = '';

    // Get UI jQuery Objects
    var $panel = $(".panel-body");
    var $userDisplay = $(".app-user");
    var $signInButton = $(".app-login");
    var $signOutButton = $(".app-logout");
    var $errorMessage = $(".app-error");

    // Check For & Handle Redirect From AAD After Login
    //ADAL Code
    //var isCallback = authContext.isCallback(window.location.hash);
    //authContext.handleWindowCallback();
    //$errorMessage.html(authContext.getLoginError());

    //if (isCallback && !authContext.getLoginError()) {
    //    window.location = authContext._getItem(authContext.CONSTANTS.STORAGE.LOGIN_REQUEST);
    //}

    // When using MSAL

    // Set the default UI
    $userDisplay.empty();
    $userDisplay.hide();
    $signInButton.show();
    $signOutButton.hide();


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


    // Check Login Status, Update UI
    //var user = authContext.getCachedUser();
    //if (user) {
    //    $userDisplay.html(user.userName);
    //    $userDisplay.show();
    //    $signInButton.hide();
    //    $signOutButton.show();
    //} else {
    //    $userDisplay.empty();
    //    $userDisplay.hide();
    //    $signInButton.show();
    //    $signOutButton.hide();
    //}


    // Handle Navigation Directly to View
    window.onhashchange = function () {
        loadView(stripHash(window.location.hash));
    };
    window.onload = function () {
        $(window).trigger("hashchange");
    };

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



    // Register NavBar Click Handlers
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
    $signInButton.click(function () {
        //ADAL Code
        //authContext.login();

        //MSAL Code
        msalInstance.loginRedirect(loginRequest);
    });

    // Route View Requests To Appropriate Controller
    function loadCtrl(view) {
        switch (view.toLowerCase()) {
            case 'home':
                return homeCtrl;
            case 'todolist':
                return todoListCtrl;
            case 'userdata':
                return userDataCtrl;
        }
    }

    // Show a View
    function loadView(view) {

        $errorMessage.empty();
        var ctrl = loadCtrl(view);
        console.log(ctrl)
        if (!ctrl)
            return;

        // Check if View Requires Authentication
        //ADAL Code
        //if (ctrl.requireADLogin && !authContext.getCachedUser()) {
        //    authContext.config.redirectUri = window.location.href;
        //    authContext.login();
        //    return;
        //}

        //MSAL Code
        if (ctrl.requireADLogin && !userAccount) {
            msalInstance.config.redirectUri = window.location.href;
            msalInstance.loginRedirect(loginRequest);
            return;
        }

        // Load View HTML
        $.ajax({
            type: "GET",
            url: "App/Views/" + view + '.html',
            dataType: "html"
        }).done(function (html) {

            // Show HTML Skeleton (Without Data)
            var $html = $(html);
            $html.find(".data-container").empty();
            $panel.html($html.html());
            ctrl.postProcess(html);

        }).fail(function () {
            $errorMessage.html('Error loading page.');
        }).always(function () {

        });
    }

    function stripHash(view) {
        return view.substr(view.indexOf('#') + 1);
    }

}());




