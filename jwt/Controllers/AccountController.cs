﻿using System;
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
        private readonly IOptions<List<Setting>> _settings;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration,
            IOptions<List<Setting>> settings)
        {
            _logger = logger;
            _configuration = configuration;
            _settings = settings;
        }

        [HttpGet]
        public IEnumerable<dynamic> Get()
        {

        string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new 
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("login")]
        public IActionResult Login([FromHeader] string login)
        {
            IActionResult response = Unauthorized();

            var headers = Request.Headers;
            var authSite = headers["auth_site"];

            if (authSite.Any() != false)
            {
                var tenantId = authSite.ToString();
                var user = Authenticate(new ITCC_User() { UserName = login });
                Setting setting = null;

                if (user != null)
                {
                    if (this._settings != null)
                    {
                        setting = this._settings.Value.Where(s => s.Key == tenantId).FirstOrDefault();
                    }

                    var token = CreateJWT(user, setting, tenantId);
                    response = Ok(new { token = token });
                }
            }

            return response;
        }

        /// <summary>
        /// Authenticates a User / Account
        /// </summary>
        /// <returns>Return a valid user account or null if authentication is unsuccessful</returns>
        private ITCC_User Authenticate(ITCC_User value)
        {
            ITCC_User user = null;

            // Validate that this user is authentic and is authorized to access your system
            // TODO: Implement your own authetication logic
            if (value.UserName == "kingsley")
            {
                user = new ITCC_User { UserName = "Kingsley Tagbo", EmailAddress = "test.test@gmail.com" };
            }

            return user;
        }

        [HttpPost("getusers")]
        [Authorize]
        public ActionResult<IEnumerable<string>> GetUsers()
        {
            return Ok(new string[] { "value1", "value2", "value3", "value4", "value5" });
        }


        private string CreateJWT(ITCC_User userInfo, Setting setting, string tenantId)
        {
            var privateKey = ((setting != null) && !string.IsNullOrEmpty(setting.PrivateKey)) ? setting.PrivateKey : tenantId;
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
