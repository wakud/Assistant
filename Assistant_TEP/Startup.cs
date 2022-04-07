using Assistant_TEP.Controllers;
using Assistant_TEP.Models;
using Assistant_TEP.MyClasses;
using ElmahCore;
using ElmahCore.Mvc;
using ElmahCore.Mvc.Notifiers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant_TEP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("Connection");
            BillingUtils.Configuration = Configuration;
            ObminController.Configuration = Configuration;
            ImportController.Configuration = Configuration;

            services.AddDbContext<MainContext>(options => options.UseSqlServer(connection));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });
            
            services.AddAuthorization(opts => 
                {
                    opts.AddPolicy("OnlyForAdministrator", policy => {
                        policy.RequireClaim("AdminStatus", "1");
                    });
                });
            
            services.AddControllersWithViews();
            services.AddMvc();
            _ = services.AddElmah();
            _ = services.AddElmah(options =>
            {
                options.OnPermissionCheck = context => context.User.Identity.IsAuthenticated;
            });
            _ = services.AddElmah(options => options.Path = "/Home/Error");
            EmailOptions emailOptions = new EmailOptions
            {
                MailRecipient = "v.kudryk@tepo.com.ua",
                MailSender = "Assistant_TEP",
                SmtpServer = "smtp.mail",
                AuthUserName = "ваш email",
                AuthPassword = "Тут ваш пароль"
            };
            _ = services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.Path = "/Home/Error";
                options.LogPath = "~/logs";
                options.Notifiers.Add(new ErrorMailNotifier("Email", emailOptions));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseElmahExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseElmah();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}"
                );
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
                DbInitialization.Initial(context);
            }
        }
        
    }
}
