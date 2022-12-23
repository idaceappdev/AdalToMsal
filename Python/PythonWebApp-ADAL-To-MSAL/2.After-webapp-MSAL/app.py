import msal
import flask
import uuid
import requests
import config

app = flask.Flask(__name__)
app.debug = True
app.secret_key = 'development'

PORT = 5000  # A flask app by default runs on PORT 5000
AUTHORITY_URL = config.AUTHORITY_HOST_URL + '/' + config.TENANT
REDIRECT_URI = 'http://localhost:{}/getAToken'.format(PORT)


@app.route("/")
def main():
    login_url = 'http://localhost:{}/login'.format(PORT)
    resp = flask.Response(status=307)
    resp.headers['location'] = login_url
    return resp


@app.route("/login")
def login():    
    resp = flask.Response(status=307)
    flask.session["flow"] = _build_auth_code_flow(scopes=config.SCOPE)
    resp.headers['location'] = flask.session["flow"]["auth_uri"]
    return resp


@app.route("/getAToken")
def main_logic():   
        result = _build_msal_app().acquire_token_by_auth_code_flow(flask.session.get("flow", {}), flask.request.args)
        
        # It is recommended to save this to a database when using a production app.
        flask.session['access_token'] = result['access_token']       
    
        return flask.redirect('/graphcall')

def _build_msal_app(cache=None, authority=None):
    return msal.ConfidentialClientApplication(
        config.CLIENT_ID, authority=authority or AUTHORITY_URL,
        client_credential=config.CLIENT_SECRET, token_cache=cache)

def _build_auth_code_flow(authority=None, scopes=None):
    return _build_msal_app(authority=authority).initiate_auth_code_flow(
        scopes or [],
        REDIRECT_URI)

@app.route('/graphcall')
def graphcall():
    if 'access_token' not in flask.session:
        return flask.redirect(flask.url_for('login'))
    endpoint = config.RESOURCE + '/' + config.API_VERSION + '/me/'
    http_headers = {'Authorization': 'Bearer ' + flask.session.get('access_token'),
                    'User-Agent': 'adal-python-sample',
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'client-request-id': str(uuid.uuid4())}
    graph_data = requests.get(endpoint, headers=http_headers, stream=False).json()
    return flask.render_template('display_graph_info.html', graph_data=graph_data)


if __name__ == "__main__":
    app.run()
