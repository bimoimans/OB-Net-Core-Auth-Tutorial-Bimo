using IdentityServer4;
using IdentityServer4.EntityFramework.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RumahMakanPadangAuth.bll;
using RumahMakanPadangAuth.dal;
using RumahMakanPadangAuth.dal.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.api
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
            //install certificate to avoid missing keyset
            X509Certificate2 cert = new X509Certificate2("example.pfx", Configuration.GetValue<string>("Certificate:Password"));
            string migrationsAssembly = "RumahMakanPadangAuth.dal";

            services.AddIdentityServer(options =>
            {
                options.Authentication.CookieAuthenticationScheme = "none";
                options.IssuerUri = Configuration.GetValue<string>("AuthorizationServer:Address");
            })
            .AddSigningCredential(cert)
            .AddResourceOwnerValidator<ResourceOwnerPasswordValidatorService>()
            .AddProfileService<UserProfileService>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(
                        Configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(
                        Configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            })
            .AddPersistedGrantStore<PersistedGrantStore>();


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration.GetValue<string>("AuthorizationServer:Address");
                options.Audience = Configuration.GetValue<string>("Service:Name");
                options.RequireHttpsMetadata = false;
            })
            .AddCookie("none")
            .AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SaveTokens = true;
                options.ClientId = "679400509968-dusc9gib5lf95il2veg2doaqh0joc3v4.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });


            services.AddControllers();
            services.AddDbContext<RumahMakanPadangAuthDbContext>(options =>
              options
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                     builder => builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                         .WithOrigins("*")
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .Build());
            });

            services.AddDbContextPool<RumahMakanPadangAuthDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), x =>
                {
                    x.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                    x.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                });
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Rumah Makan Padang Auth API",
                    Description = "Login ke warung makan padang dengan API",
                    Contact = new OpenApiContact
                    {
                        Name = "Bimo",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/bimoimans"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rumah Makan Padang Auth API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseCors("CorsPolicy");
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
