(function () {

    // The HTML for this View
    var viewHTML;
    // Instantiate the ADAL AuthenticationContext
    //var authContext = new AuthenticationContext(config);

    /*Instantiate MSAL PublicClientApplication Object*/
    var msalInstance = new msal.PublicClientApplication(config);

    function refreshViewData() {

        // Empty Old View Contents
        var $dataContainer = $(".data-container");
        $dataContainer.empty();
        var $loading = $(".view-loading");

        //var user = authContext.getCachedUser();
        var user = msalInstance.getAllAccounts()[0];
        //console.log(user);

        var $html = $(viewHTML);
        var $template = $html.find(".data-container");
        var output = '';

        // ADAL Code
        //for (var property in user.profile) {
        //    if (user.profile.hasOwnProperty(property)) {
        //        var $entry = $template;
        //        $entry.find(".view-data-claim").html(property);
        //        $entry.find(".view-data-value").html(user.profile[property]);
        //        output += $entry.html();
        //    }
        //}

        // MSAL Code
        for (var property in user.idTokenClaims) {
            if (user.idTokenClaims.hasOwnProperty(property)) {
                var $entry = $template;
                $entry.find(".view-data-claim").html(property);
                $entry.find(".view-data-value").html(user.idTokenClaims[property]);
                output += $entry.html();
            }
        }

        // Update the UI
        $loading.hide();
        $dataContainer.html(output);
    };

    // Module
    window.userDataCtrl = {
        requireADLogin: true,
        preProcess: function (html) {

        },
        postProcess: function (html) {
            viewHTML = html;
            refreshViewData();
        },
    };
}());

