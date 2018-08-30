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

namespace JAMTech.Filters
{
    /// <summary>
    /// Attribute used to check whether the request contains a token in the Header "X-Authenticated-Userid"
    /// If it does contains the header it then will check if the body and query string of the request to find
    /// a rut, finally, it will try to find the rut in the payload of the 
    /// </summary>
    public class GoogleAuth : ActionFilterAttribute
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        const string googlePublicKey = "";
        const string uidFieldName = "forUser";
        public override void OnActionExecuted(ActionExecutedContext context) { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {         
            try
            {
                //JWT
                var jwt = JwtHeader(context, "Authorization");
                if (jwt != null)
                {
                    var token = ValidateAndDecode(jwt.Replace("Bearer ", ""), null);
                    if (!JwtContains(token, "sub", "105560751972558300957")) //TODO get uid from parameters
                        throw new SecurityTokenException();
                }
                else
                {
                    //Access token
                    var token = GetFromRequest(context, "access_token");
                    var uid = GetFromRequest(context, uidFieldName);
                    if (token != string.Empty)
                    {
                        //check if token is valid
                        var response = Helpers.Net.GetResponse("https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + token).Result;
                        if (!response.IsSuccessStatusCode)
                            throw new SecurityTokenException("Invalid access token");
                        else
                        {
                            // validate parameter uid against google uid
                            var googleResult = response.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<dynamic>(googleResult);
                            if (!string.IsNullOrEmpty(uid))
                            {
                                if (uid != result.id.ToString())
                                    throw new SecurityTokenException("Invalid user id");
                            }
                            else
                            {
                                //add uid as parameter
                                context.ActionArguments.Add(uidFieldName, result.id.ToString());
                            }
                        }
                    }
                    else
                        throw new SecurityTokenException("Access token missing");
                }

            }
            catch(Exception ex)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "GoogleAuth failed. " + ex.Message
                };
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

        private string JwtHeader(ActionExecutingContext context, string jwtHeaderName = "X-Authenticated-Userid")
        {
            return context.HttpContext.Request.Headers[jwtHeaderName];
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
    }
}
