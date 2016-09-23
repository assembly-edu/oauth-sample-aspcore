using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AssemblyOAuthSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            // A simple error page to avoid pulling in another dependency.
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    if (context.Response.HasStarted)
                    {
                        throw;
                    }
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(ex.ToString());
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/")
            });

            app.UseOAuthAuthentication(new OAuthOptions()
            {
                AuthenticationScheme = "Assembly",
                DisplayName = "Assembly",
                ClientId = Configuration["assembly:clientid"],
                ClientSecret = Configuration["assembly:clientsecret"],
                CallbackPath = new PathString("/assembly-auth"),
                AuthorizationEndpoint = "http://192.168.201.1:3000/oauth/authorize",
                TokenEndpoint = "http://192.168.201.1:3000/oauth/token",
                SaveTokens = true,
                Scope =
                {
                    "students",
                    "teaching_groups"
                }
            });

            app.Map("/logout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("User cookie removed. Goodbye <br>");
                    await context.Response.WriteAsync("<a href=\"/\">Home</a>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });

            app.Map("/accepted", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    // Display token information.
                    // Clearly you'll want to store these details securely in your app in order to make calls to the Platform API
                    // with access to this school's data. 
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("Access Token: " + await context.Authentication.GetTokenAsync("access_token") + "<br>");
                    await context.Response.WriteAsync("Refresh Token: " + await context.Authentication.GetTokenAsync("refresh_token") + "<br>");
                    await context.Response.WriteAsync("expires_at: " + await context.Authentication.GetTokenAsync("expires_at") + "<br>");
                    await context.Response.WriteAsync("<a href=\"/logout\">Done</a><br>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });

            app.Run(async context =>
            {
                var authType = context.Request.Query["authscheme"];
                if (!string.IsNullOrEmpty(authType))
                {
                    // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo)
                    // Instead we're redirecting to our 'accepted' endpoint since we're slightly mis-using the standard OAuth middleware.
                    await context.Authentication.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "/accepted" });
                    return;
                }

                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<html><body>");
                await context.Response.WriteAsync("Something here about authorizing your app with Assembly: <br>");

                var assembly = context.Authentication.GetAuthenticationSchemes().First(t => t.DisplayName == "Assembly");
                await context.Response.WriteAsync("<a href=\"?authscheme=" + assembly.AuthenticationScheme + "\">" + (assembly.DisplayName ?? "(suppressed)") + "</a><br>");
                await context.Response.WriteAsync("</body></html>");
            });
        }
    }
}
