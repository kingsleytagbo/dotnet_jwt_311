using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using jwt.Models;

namespace jwt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
            });

            services.AddControllers();

            // Add the whole configuration object here.
            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<List<Setting>>(Configuration.GetSection("Settings:Setting"));

            /*
            ServiceProvider sp = services.BuildServiceProvider();
            IOptions<List<Setting>> tenants = sp.GetService<IOptions<List<Setting>>>();
            */
            IOptions<List<Setting>> tenants = this.BuildTenantsFromServiceProvider(services);

            // Get access to the tenants defined in appsettings.json
            if (tenants != null)
            {
                // Console.WriteLine(tenants.ToString());
            }

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
                //options.RequireHttpsMetadata = false;
                //options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
               {
                    //Use this for a Single Site / Non-tenant Site
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    //ValidAudience = Configuration["Jwt:Issuer"],

                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,

                    // Specify the valid issue from appsettings.json
                    ValidIssuer = Configuration["Jwt:Issuer"],

                    // Specify the tenant API keys as the valid audiences
                    ValidAudiences = tenants.Value.Select(t => t.Key).ToList(),

                   IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                   {
                       List<SecurityKey> keys = new List<SecurityKey>();
                       Setting setting = tenants.Value.Where(t => t.Key == kid).FirstOrDefault();
                       var privateKey = ((setting != null) && !string.IsNullOrEmpty(setting.PrivateKey)) ? setting.PrivateKey : kid;

                       if (!string.IsNullOrEmpty(privateKey))
                       {
                           var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
                           keys.Add(signingKey);
                       }
                       return keys;
                   }
               };

               options.Events = new JwtBearerEvents
               {
                   OnAuthenticationFailed = ctx =>
                   {
                       // Console.WriteLine(ctx);
                       if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                       {
                           ctx.Response.Headers.Add("Token-Expired", "true");
                       }
                       return Task.CompletedTask;
                   },

                   OnMessageReceived = ctx =>
                   {
                       // Console.WriteLine(ctx);
                        //ctx.Request.EnableBuffering();
                        return Task.CompletedTask;
                   },

                   OnTokenValidated = context =>
                   {
                       // Console.WriteLine(context.ToString());
                       return Task.CompletedTask;
                   }
               };

           });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //JWT Token
            app.UseAuthentication();

            app.UseAuthorization();

            // Shows UseCors with named policy.
            app.UseCors("AllowAllHeaders");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IOptions<List<Setting>> BuildTenantsFromServiceProvider(IServiceCollection services)
        {
            ServiceProvider sp = services.BuildServiceProvider();
            IOptions<List<Setting>> tenants = sp.GetService<IOptions<List<Setting>>>();
            return tenants;
        }
    }
}
