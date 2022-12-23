# Migration steps to be followed to migrate to MSAL python and from ADAL in flsk web app

## Changes needed in config.py file

- Add the new scope in the config file as below. 
   
   ```sh
    SCOPE = ['User.Read']
   ```
## Changes needed in app.py file

- Remove the below line of code to get rid of adal refernce
   
   ```sh
    import adal
   ```
  Replace it with MSAL package refernce 
   
   ```sh
    import msal
   ```
- Remove the below line of code which is no longer needed MSAL provides this abstration.
   
   ```sh
    TEMPLATE_AUTHZ_URL = ('https://login.microsoftonline.com/{}/oauth2/v2.0/authorize?' +
                      'response_type=code&client_id={}&redirect_uri={}&' +
                      'state={}&scope={}/{}')
   ```
 - Define the below two methods which initilizes the msal object.
   
   ```sh
    def _build_msal_app(cache=None, authority=None):
      return msal.ConfidentialClientApplication(
        config.CLIENT_ID, authority=authority or AUTHORITY_URL,
        client_credential=config.CLIENT_SECRET, token_cache=cache)

    def _build_auth_code_flow(authority=None, scopes=None):
      return _build_msal_app(authority=authority).initiate_auth_code_flow(
        scopes or [],
        REDIRECT_URI)
   ```
 - Remove the below code in the login method.
   
   ```sh
    auth_state = str(uuid.uuid4())
    flask.session['state'] = auth_state
    authorization_url = TEMPLATE_AUTHZ_URL.format(
        config.TENANT,
        config.CLIENT_ID,
        REDIRECT_URI,
        auth_state,
        config.RESOURCE,
        config.SCOPE)
   ```
   Replace it with the below line of code
   
   ```sh
   flask.session["flow"] = _build_auth_code_flow(scopes=config.SCOPE)
    resp.headers['location'] = flask.session["flow"]["auth_uri"]
   ```
- Remove the below code in the getAToken method.
   
   ```sh
   code = flask.request.args['code']
        state = flask.request.args['state']
        if state != flask.session['state']:
            raise ValueError("State does not match")
        auth_context = adal.AuthenticationContext(AUTHORITY_URL)
        token_response = auth_context.acquire_token_with_authorization_code(code, REDIRECT_URI, config.RESOURCE,
           config.CLIENT_ID, config.CLIENT_SECRET)
   ```
   Replace it with the below line of code
   
   ```sh
   result = _build_msal_app().acquire_token_by_auth_code_flow(flask.session.get("flow", {}), flask.request.args)
    
    # It is recommended to save this to a database when using a production app.
        flask.session['access_token'] = result['access_token']       

   ```
## Steps to verify that app is using MSAL.

- Observe the URL during sign-in which should redirect to v2 endpoint 
  
  ```sh
   https://login.microsoftonline.com/<Tenant-Id>/oauth2/v2.0/authorize?client_id=<Client-Id>&redirect_uri=https%3a%2f%2flocalhost%3a44377%2fsignin-oidc&response_type=code&scope=openid+profile+offline_access+api%3 
   ```
 
- Go to the sign-in logs under non-interactive section and observe that, now we are reporting the MSAL version instead of ADAL, This confirms successful migration. Please do note that, interactive log may report blank It is the access token request which is implemented by the MSAL python

  ![image](https://user-images.githubusercontent.com/62542910/209336559-5fdfc971-9445-4671-a8ec-4fb81b3d2f0d.png)
