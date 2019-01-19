using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JAMTech.Extensions;
using System.Threading;
using JAMTech.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JAMTech.Models;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class SavedPasswordController : BaseController
    {
        /// <summary>
        /// Add saved passwords to user storage
        /// </summary>
        /// <param name="savedPasswords">Collection of saved passwords</param>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost()]
        [Produces(typeof(IEnumerable<UserSavedPassword>))]
        public async Task<IActionResult> SavePasswords([FromBody] List<SavedPassword> savedPasswords, string forUser=null)
        {
            if (savedPasswords == null || forUser == null || !savedPasswords.Any(m=> m != null))
                return new BadRequestResult();

            var obj = new UserSavedPassword()
            {
                uid=forUser,
                Data= savedPasswords
            };

             var result = await obj.ToMongoDB<UserSavedPassword>();
             return new OkObjectResult(result);
        }

        /// <summary>
        /// Delete saved password
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassword(string id, string forUser = null)
        {
            //check if sensor id correspond to the authenticated user (forUser)
            var userResults = await MongoDB.FromMongoDB<UserSavedPassword, SavedPassword>(forUser);
            if (userResults == null || !userResults.Any(t => t.Id == id))
                return new ForbidResult();
            var obj = new Models.UserSavedPassword()
            {
                uid = forUser,
                _id = new Models.UserSavedPassword.id { oid = id }
            };
            await obj.DeleteFromMongoDB();
            return new OkResult();
        }

        
        /// <summary>
        /// Get saved passwords of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpGet()]
        [Produces(typeof(IEnumerable<SavedPassword>))]
        public async Task<IActionResult> GetSavedPasswords(string forUser=null)
        {
            var result = await MongoDB.FromMongoDB<UserSavedPassword, SavedPassword> (forUser);
            return new OkObjectResult(result as IEnumerable<SavedPassword>);
        }
    }
}
