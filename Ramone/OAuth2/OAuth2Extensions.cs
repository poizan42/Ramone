﻿using System;
using System.Collections.Specialized;
using System.Web;
using CuttingEdge.Conditions;
using Ramone.Utility;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections;


namespace Ramone.OAuth2
{
  public static class OAuth2Extensions
  {
    private const string OAuth2SettingsSessionKey = "OAuth2Settings";
    private const string OAuth2StateSessionKey = "OAuth2State";


    /// <summary>
    /// Configure OAuth2 and store configuration in session for later use. 
    /// Must always be called before using any of the other OAuth2 methods.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="settings"></param>
    /// <returns>The session passed in as argument.</returns>
    public static ISession OAuth2_Configure(this ISession session, OAuth2Settings settings)
    {
      Condition.Requires(settings, "settings").IsNotNull();

      session.Items[OAuth2SettingsSessionKey] = settings;

      return session;
    }


    /// <summary>
    /// Get URL for user authorization via browser (user agent). This will initiate the "Authorization Code Grant" flow.
    /// </summary>
    /// <remarks>See http://tools.ietf.org/html/rfc6749#section-4.1.1</remarks>
    /// <param name="session"></param>
    /// <param name="scope"></param>
    /// <returns>Authorization request URL.</returns>
    public static Uri OAuth2_GetAuthorizationRequestUrl(this ISession session, string scope = null)
    {
      OAuth2Settings settings = GetSettings(session);

      string authorizationRequestState = RandomStrings.GetRandomStringWithLettersAndDigitsOnly(20);
      OAuth2SessionState state = session.OAuth2_GetOrCreateState();
      state.AuthorizationState = authorizationRequestState;

      var codeRequestArgs = new
      {
        response_type = "code",
        client_id = settings.ClientID,
        redirect_uri = settings.RedirectUri.ToString(),
        scope = scope,
        state = authorizationRequestState
      };

      return settings.AuthorizationEndpoint.AddQueryParameters(codeRequestArgs);
    }


    /// <summary>
    /// Extract authorization code from authorization response encoded in a redirect URL from the authorization endpoint.
    /// </summary>
    /// <remarks>After completion of the authorization process the browser will be redirected to a URL specified
    /// by the client (and configured using Ramone's OAuth2Settings). This URL will contain the acquired 
    /// authorization code. Call OAuth2_GetAuthorizationCodeFromRedirectUrl to extract the code.</remarks>
    /// <param name="session"></param>
    /// <param name="redirectUrl"></param>
    /// <returns>Authorization code</returns>
    public static string OAuth2_GetAuthorizationCodeFromRedirectUrl(this ISession session, string redirectUrl)
    {
      Condition.Requires(redirectUrl, "redirectUrl").IsNotNull();

      OAuth2SessionState sessionState = session.OAuth2_GetState();

      NameValueCollection parameters = HttpUtility.ParseQueryString(new Uri(redirectUrl).Query);

      string state = parameters["state"];
      if (sessionState.AuthorizationState == null || state != sessionState.AuthorizationState)
        return null;

      return parameters["code"];
    }


    /// <summary>
    /// Request an access token from authorization code acquired in earlier requests.
    /// </summary>
    /// <remarks>See http://tools.ietf.org/html/rfc6749#section-4.1.3</remarks>
    /// <param name="session"></param>
    /// <param name="authorizationCode"></param>
    /// <param name="useAccessToken">Request automatic use of the returned access token in following requests.</param>
    /// <returns></returns>
    public static OAuth2AccessTokenResponse OAuth2_GetAccessTokenFromAuthorizationCode(
      this ISession session, 
      string authorizationCode,
      bool useAccessToken = true)
    {
      OAuth2Settings settings = GetSettings(session);

      NameValueCollection tokenRequestArgs = new NameValueCollection();
      tokenRequestArgs["grant_type"] = "authorization_code";
      tokenRequestArgs["code"] = authorizationCode;
      tokenRequestArgs["redirect_uri"] = settings.RedirectUri.ToString();
      tokenRequestArgs["client_id"] = settings.ClientID;

      if (!settings.UseBasicAuthenticationForClient)
        tokenRequestArgs["client_secret"] = settings.ClientSecret;

      return GetAndStoreAccessToken(session, tokenRequestArgs, useAccessToken);
    }


    /// <summary>
    /// Request an access token using the flow "Resource Owner Password Credentials Grant".
    /// </summary>
    /// <remarks>See http://tools.ietf.org/html/rfc6749#section-4.3</remarks>
    /// <param name="session"></param>
    /// <param name="ownerUserName"></param>
    /// <param name="ownerPassword"></param>
    /// <param name="useAccessToken">Request automatic use of the returned access token in following requests.</param>
    /// <returns></returns>
    public static OAuth2AccessTokenResponse OAuth2_GetAccessTokenFromResourceUsingOwnerUsernamePassword(
      this ISession session,
      string ownerUserName, 
      string ownerPassword,
      bool useAccessToken = true)
    {
      OAuth2Settings settings = GetSettings(session);

      NameValueCollection tokenRequestArgs = new NameValueCollection();
      tokenRequestArgs["grant_type"] = "password";
      tokenRequestArgs["username"] = ownerUserName;
      tokenRequestArgs["password"] = ownerPassword;

      if (!settings.UseBasicAuthenticationForClient)
      {
        tokenRequestArgs["client_id"] = settings.ClientID;
        tokenRequestArgs["client_secret"] = settings.ClientSecret;
      }

      return GetAndStoreAccessToken(session, tokenRequestArgs, useAccessToken);
    }


    /// <summary>
    /// Does this session have an active access token associated with it?
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public static bool OAuth2_HasActiveAccessToken(this ISession session)
    {
      return session.RequestInterceptors.Find("Bearer") != null;
    }


    /// <summary>
    /// Get a copy of the OAuth2 settings (use OAuth2_Configure to change them)
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public static OAuth2Settings OAuth2_GetSettings(this ISession session)
    {
      object settings;
      session.Items.TryGetValue(OAuth2SettingsSessionKey, out settings);
      return settings as OAuth2Settings;
    }


    /// <summary>
    /// Get current authorization state.
    /// </summary>
    /// <remarks>The authorization state contains information about active authorization codes,
    /// authorization request state, access token and so on. The state can later on be restored with a
    /// called to OAuth2_RestoreState.</remarks>
    /// <param name="session"></param>
    /// <returns></returns>
    public static OAuth2SessionState OAuth2_GetState(this ISession session)
    {
      object state;
      session.Items.TryGetValue(OAuth2StateSessionKey, out state);
      return state as OAuth2SessionState;
    }


    /// <summary>
    /// Restore authorization state previously obtained from OAuth2_GetState.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static ISession OAuth2_RestoreState(this ISession session, OAuth2SessionState state)
    {
      session.Items[OAuth2StateSessionKey] = state;
      if (state.AccessToken != null && state.TokenType != null)
        ActivateAuthorization(session, state.AccessToken, state.TokenType);
      return session;
    }


    internal static OAuth2SessionState OAuth2_GetOrCreateState(this ISession session)
    {
      object obj;
      session.Items.TryGetValue(OAuth2StateSessionKey, out obj);
      OAuth2SessionState state = obj as OAuth2SessionState;
      if (state == null)
      {
        state = new OAuth2SessionState();
        session.Items[OAuth2StateSessionKey] = state;
      }
      return state;
    }


    private static OAuth2AccessTokenResponse GetAndStoreAccessToken(ISession session, object args, bool useAccessToken)
    {
      OAuth2Settings settings = GetSettings(session);

      Request request = session.Bind(settings.TokenEndpoint)
                               .AsFormUrlEncoded()
                               .AcceptJson();

      if (settings.UseBasicAuthenticationForClient)
        request = request.BasicAuthentication(settings.ClientID, settings.ClientSecret);

      using (var response = request.AcceptJson().Post<Hashtable>(args))
      {
        OAuth2AccessTokenResponse accessToken = new OAuth2AccessTokenResponse
        {
          access_token = TryGet<string>(response.Body["access_token"]),
          token_type = TryGet<string>(response.Body["token_type"]),
          expires_in = TryGet<int?>(response.Body["expires_in"]),
          refresh_token = TryGet<string>(response.Body["refresh_token"]),
          AllParameters = response.Body
        };

        if (useAccessToken)
        {
          OAuth2SessionState state = session.OAuth2_GetOrCreateState();
          state.AccessToken = accessToken.access_token;
          state.TokenType = accessToken.token_type;

          ActivateAuthorization(session, accessToken.access_token, accessToken.token_type);
        }
        return accessToken;
      }
    }


    private static T TryGet<T>(object v)
    {
      if (v == null)
        return default(T);
      if (v is T)
        return (T)v;
      return default(T);
    }


    private static void ActivateAuthorization(ISession session, string accessToken, string tokenType)
    {
      if (string.Equals(tokenType, "bearer", StringComparison.InvariantCultureIgnoreCase))
      {
        session.RequestInterceptors.Add("Bearer", new BearerTokenRequestInterceptor(accessToken));
      }
      else
        throw new InvalidOperationException(string.Format("Unknown access token type '{0}' (expected 'bearer')", tokenType));
    }


    private static OAuth2Settings GetSettings(ISession session)
    {
      object settings;
      if (!session.Items.TryGetValue(OAuth2SettingsSessionKey, out settings))
        throw new InvalidOperationException("No OAuth2 settings has been registered with the session");

      if (settings is OAuth2Settings)
        return (OAuth2Settings)settings;
      
      throw new InvalidOperationException(string.Format("Unknown type '{0}' has been registered for OAuth2 settings with the session", settings.GetType()));
    }
  }
}
