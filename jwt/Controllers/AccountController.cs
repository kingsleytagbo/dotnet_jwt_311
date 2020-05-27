using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

//JWT Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Options;
using jwt.Models;

namespace jwt.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<List<Tenant>> _tenants;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration,
            IOptions<List<Tenant>> tenants)
        {
            _logger = logger;
            _configuration = configuration;
            _tenants = tenants;
        }

        [HttpGet]
        public IEnumerable<dynamic> Get()
        {

        string[] Values = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new 
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Value = Values[rng.Next(Values.Length)]
            })
            .ToArray();
        }


        [HttpPost("getusers")]
        [Authorize]
        public ActionResult<IEnumerable<string>> GetUsers()
        {
            return Ok(new string[] { "value1", "value2", "value3", "value4", "value5" });
        }


        /// <summary>
        /// Authenticates a User / Account
        /// </summary>
        /// <returns>Return a valid user account or null if authentication is unsuccessful</returns>
        private ITCC_User Authenticate(Login value)
        {
            ITCC_User user = null;

            // Validate that this user is authentic and is authorized to access your system
            // TODO: Implement your own authetication logic
            if (value.UserName == "Kingsley")
            {
                user = new ITCC_User { UserName = "Kingsley Tagbo", EmailAddress = "test.test@gmail.com" };
            }

            return user;
        }


        [HttpPost("login")]
        public IActionResult Login([FromHeader] String username, [FromHeader] string password, [FromHeader] bool rememberme )
        {
            IActionResult response = Unauthorized();

            try
            {
                var headers = Request.Headers;
                var authSite = headers["auth_site"];
                Login login = new Login() { UserName = username, Password = password, RememberMe = rememberme };

                Tenant tenant = null;
                ITCC_User user = null;
                string tenantId = null;
                string token = null;

                if (authSite.Any() != false)
                {
                    user = Authenticate(login);
                    if ((user != null) && (this._tenants != null))
                    {
                        tenantId = authSite.ToString();
                        tenant = this._tenants.Value.Where(s => s.Key == tenantId).FirstOrDefault();
                        token = CreateJWT(user, tenant, tenantId);
                        response = Ok(new { token = token });

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return response;
        }


        private string CreateJWT(ITCC_User userInfo, Tenant tenant, string tenantId)
        {
            var privateKey = ((tenant != null) && !string.IsNullOrEmpty(tenant.PrivateKey)) ? tenant.PrivateKey : tenantId;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            DateTime jwtExpires = DateTime.Now.AddMinutes(30);
            int jwtDuration = 30;

            int.TryParse(_configuration["Jwt:Expires"], out jwtDuration);
            jwtExpires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(jwtDuration));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                tenantId,
                new[]
                    {
                        new Claim(ClaimTypes.Name, userInfo.UserName)
                    },
              expires: jwtExpires,
              signingCredentials: credentials);

            token.Header.Add("kid", tenantId);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
