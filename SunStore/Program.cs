using SunStore.Services;
using Microsoft.EntityFrameworkCore;
using SunStore.APIServices;
using BusinessObjects.Models;

namespace SunStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<SunStoreContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DBContext"));
            });

            builder.Services.AddScoped(typeof(SunStoreContext));

            //Add session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient("api", httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://localhost:7270/api/");
            });

            builder.Services.AddScoped<OrderAPIService>();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddSingleton<IVnPayService, VnPayService>();

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.WithOrigins("https://localhost:7127")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                                  });
            });
            


            var app = builder.Build();

            app.UseCors(MyAllowSpecificOrigins);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
