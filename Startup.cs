using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using CStat.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CStat.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Http.Features;
using CStat.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
//using System.Diagnostics;
//using Microsoft.AspNetCore.DataProtection;
//using System.IO;

namespace CStat
{
    public class Startup
    {
        public static IConfiguration CSConfig = null;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnv = env;
            CSConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnv { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<CStatContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("CStatConnection")));

            //RJR services.Configure<ApplicationDbContext>(op => 
            //RJR      op.Database.Migrate()); // Make sure the identity database is created
            //            services.AddCors();
            //RJR            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //RJR                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromHours(1);
            });

            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizePage("/Index");
                    options.Conventions.AuthorizePage("/Docs");
                    options.Conventions.AuthorizePage("/Inventory/Index");
                    options.Conventions.AuthorizePage("/Index1");
                    options.Conventions.AuthorizeFolder("/Inventory");
                    options.Conventions.AuthorizeFolder("/Items");
                    options.Conventions.AuthorizeFolder("/People");
                    options.Conventions.AuthorizeFolder("/Tasks");
                    options.Conventions.AuthorizeFolder("/Docs");
                    options.Conventions.AuthorizeFolder("/Private");
                    options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
                    options.Conventions.AllowAnonymousToFolder("/Private/PublicPages");
                    options.Conventions.AllowAnonymousToFolder("/api/CStat");
                });

            //services.AddDefaultIdentity<IdentityUser>()/*.AddDefaultUI(UIFramework.Bootstrap4)*/.AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                    //Allow host access from any source
                    //Todo: the new CORS middleware has blocked allowing any origin, that is, setting allowanyorigin will not take effect
                    //AllowAnyOrigin()
                    //Set the domain allowed to access
                    //Todo: Currently, there are bugs in. Net core 3.1, which can be temporarily solved by setisoriginallowed
                    //.WithOrigins(Configuration["CorsConfig:Origin"])
                    .SetIsOriginAllowed(t => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            services.AddHttpContextAccessor();
            services.AddControllers();
            //services.AddSingleton<IWorker, Worker>();
            services.AddHostedService<CSBkgService>();
            services.AddScoped<IScopedProcessingService, CSProcService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //                Trace.WriteLine("DEVELOPMENT Env.");
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                //                Trace.WriteLine("PRODUCTION Env.");
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // after UseCors : app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();
            //app.UseCors(builder => builder
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
