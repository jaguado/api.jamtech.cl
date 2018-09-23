using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Collections;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace JAMTech.Filters
{
    /// <summary>
    /// Attribute used to check whether the request contains a token in the Header "X-Authenticated-Userid"
    /// If it does contains the header it then will check if the body and query string of the request to find
    /// a rut, finally, it will try to find the rut in the payload of the 
    /// </summary>
    public class SocialAuth : IAsyncActionFilter
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        const string googlePublicKey = "";
        const string uidFieldName = "forUser";
        const string authHeader = "Authorization";
        const string tokenName = "access_token";
        private static readonly bool checkAuth = Environment.GetEnvironmentVariable("disableAuth") == null || bool.Parse(Environment.GetEnvironmentVariable("disableAuth")) == false;

        private async Task CheckGoogleAsync(ActionExecutingContext context, string accessToken, string uid)
        {
            if (GetFromRequest(context, "provider") == "facebook") return;
            try
            {
                await ValidateAccessTokenWithGoogleAsync(context, accessToken, uid);
            }
            catch (Exception ex)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "GoogleAuth failed. " + ex.Message
                };
            }
        }

        private static async Task ValidateAccessTokenWithGoogleAsync(ActionExecutingContext context, string token, string uid)
        {
            //check if token is valid
            const string baseUrl = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=";
            using (var response = await Helpers.Net.GetResponse(baseUrl + token))
            {
                if (!response.IsSuccessStatusCode)
                    context.Result = new ContentResult()
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Content = "Invalid access token"
                    };
                else
                {
                    var googleResult = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(googleResult, Startup.jsonSettings);
                    // validate parameter uid against google uid
                    if (!string.IsNullOrEmpty(uid))
                    {
                        if (uid != result.id.ToString())
                            context.Result = new ContentResult()
                            {
                                StatusCode = StatusCodes.Status401Unauthorized,
                                Content = "Invalid user id"
                            };
                    }
                    else
                    {
                        //add user info to request
                        context.ActionArguments[uidFieldName] = result.id.ToString();
                        context.ActionArguments["userInfo"] = googleResult;
                    }
                }
            }
        }

        private async Task CheckFacebookAsync(ActionExecutingContext context, string accessToken, string uid)
        {
            if (GetFromRequest(context, "provider") == "google") return;
            try
            {
                await ValidateAccessTokenWithFacebookAsync(context, accessToken, uid);
            }
            catch (Exception ex)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "FacebookAuth failed. " + ex.Message
                };
            }
        }

        private static async Task ValidateAccessTokenWithFacebookAsync(ActionExecutingContext context, string token, string uid)
        {
            //check if token is valid
            using (var response = await Helpers.Net.GetResponse("https://graph.facebook.com/me?access_token=" + token))
            {
                if (!response.IsSuccessStatusCode)
                    context.Result = new ContentResult()
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Content = "Invalid access token"
                    };
                else
                {
                    var facebookResult = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(facebookResult, Startup.jsonSettings);

                    // validate parameter uid against google uid
                    if (!string.IsNullOrEmpty(uid))
                    {
                        if (uid != result.id.ToString())
                            context.Result = new ContentResult()
                            {
                                StatusCode = StatusCodes.Status401Unauthorized,
                                Content = "Invalid user id"
                            };
                    }
                    else
                    {
                        //add user info to request
                        context.ActionArguments[uidFieldName] = result.id.ToString();
                        context.ActionArguments["userInfo"] = facebookResult;
                    }
                }
            }
        }

        private T GetFromBody<T>(ActionExecutingContext context, string key)
        {
            return context.ActionArguments.ContainsKey(key) ? (T)context.ActionArguments[key] : default(T);
        }

        private string GetFromRequest(ActionExecutingContext context, string key)
        {
            return context.ActionArguments.ContainsKey(key) ? context.ActionArguments[key].ToString() : context.HttpContext.Request.Query[key].ToString();
        }

        private string GetFromHeader(ActionExecutingContext context, string headerName = "X-Authenticated-Userid")
        {
            return context.HttpContext.Request.Headers[headerName];
        }

        private bool JwtContains(JwtSecurityToken token, string key, string value)
        {
            try
            {
                var tokenValue = token.Payload[key].ToString();
                var auth = tokenValue == value;
                if (!auth) Console.Error.WriteLineAsync($"Authorization problem on field '{key}'. Input: {value.ToString()}, Auth: {tokenValue.ToString()}");
                return auth;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLineAsync(ex.Message);
                return false;
            }
        }

        public static JwtSecurityToken ValidateAndDecode(string jwt, X509Certificate2 cert)
        {
            if (cert == null) return new JwtSecurityToken(jwt);

            var rsaSecurityKey = cert !=null ? new RsaSecurityKey(cert.GetRSAPublicKey()) : null;
            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = rsaSecurityKey!=null,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey= rsaSecurityKey != null
            };

            if (rsaSecurityKey != null)
                validationParameters.IssuerSigningKeys = new SecurityKey[] { rsaSecurityKey };
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
                // Or, you can return the ClaimsPrincipal
                // (which has the JWT properties automatically mapped to .NET claims)
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                throw new Exception($"Token was invalid: {argex.Message}");
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //remove auth when method is OPTIONS
            if (checkAuth && context.HttpContext.Request.Method != "OPTIONS")
            {
                //Get access token and check state
                var accessToken = GetFromHeader(context, authHeader) ?? string.Empty;
                if (accessToken == string.Empty)
                    accessToken = GetFromRequest(context, tokenName);
                else
                    accessToken = accessToken.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                {
                    //throw new SecurityTokenException("Access token missing");
                    // Check for authorization
                    if (!context.Filters.Any(a=> a is AllowAnonymousFilter))
                    {
                        context.Result = new ContentResult()
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Content = "Access token missing"
                        };
                    }
                }
                else
                {
                    var uid = GetFromRequest(context, uidFieldName);
                    await CheckGoogleAsync(context, accessToken, uid);
                    await CheckFacebookAsync(context, accessToken, uid);
                }
            }

            if (context.Result == null)
                await next();
        }
    }
}
