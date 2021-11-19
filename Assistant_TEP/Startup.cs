using Assistant_TEP.Controllers;
using Assistant_TEP.Models;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        // Цей метод викликається під час старту проги. Використовуйте цей метод, щоб додати послуги до контейнера.
        public void ConfigureServices(IServiceCollection services)
        {
            //отримуємо рядок підключення до БД
            string connection = Utils.Decrypt(Configuration.GetConnectionString("TEPConnection"));
            BillingUtils.Configuration = Configuration;
            ObminController.Configuration = Configuration;
            ImportController.Configuration = Configuration;

            // створюємо БД
            services.AddDbContext<MainContext>(options => options.UseSqlServer(connection));

            // авторизація користувача
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });
            
            //робимо перевірку на адміністратора
            services.AddAuthorization(opts => 
                {
                    opts.AddPolicy("OnlyForAdministrator", policy => {
                        policy.RequireClaim("AdminStatus", "1");
                    });
                });
            
            services.AddControllersWithViews();
            services.AddMvc();
        }

        // Цей метод викликається під час виконання. Використовуйте цей метод для налаштування протоколу HTTP-запиту.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                app.UseDeveloperExceptionPage();
            // если приложение в процессе разработки
            //if (env.IsDevelopment())
            //{
            //    // то выводим информацию об ошибке, при наличии ошибки
            //}
            //else
            //{
                //app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            //}

            app.UseStaticFiles();       //чтобы приложение могло бы отдавать статические файлы клиенту
            app.UseRouting();           // добавляем возможности маршрутизации
            app.UseAuthentication();    // аутентификация
            app.UseAuthorization();     // авторизация

            // устанавливаем адреса, которые будут обрабатываться
            app.UseEndpoints(endpoints =>
            {
                // само определение маршрута - он должен соответствовать запросу {controller}/{action}
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}"        //при старті проги запускаємо сторінку логіну
                    //pattern: "{controller=Home}/{action=Index}/{id?}"         //при старті проги запускаємо основну сторінку
                );
            });

            //наповнюємо табл юзерів  і права
            using (var scope = app.ApplicationServices.CreateScope())
            {
                MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
                DbInitialization.Initial(context);
            }
        }
        
    }
}
