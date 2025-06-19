using SunStore.Services;
using Microsoft.EntityFrameworkCore;
using SunStore.APIServices;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http.Headers;
using SunStore.Configs;

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

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient("api", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7270/api/");
            })
            // Use Bearer Token
            .AddHttpMessageHandler<AuthTokenHandler>();

            #region DIs

            builder.Services.AddTransient<AuthTokenHandler>();
            builder.Services.AddScoped<OrderAPIService>();
            builder.Services.AddScoped<AuthAPIService>();
            builder.Services.AddScoped<ProductAPIService>();
            builder.Services.AddScoped<ProductOptionAPIService>();
            builder.Services.AddScoped<CartAPIService>();
          
            #endregion

            //Add session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
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

            #region Jwt Authentication configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!));

            builder.Services.AddAuthentication(options =>
            {
                // Scheme used to parse user principal from requests.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Scheme used to handle unauthorized challenge
                // it will return a 401 error if the user is unauthorized.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                };

                // This event will be called whenever the authentication middleware for Jwt is invoked.
                // options.Events is used to manipulated Jwt bearer authentication lifecycle.
                // By default, OnMessageReceived will read the Jwt from the request header
                // but in this case we override the event and read the Jwt token from cookie instead.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.HttpContext.Request.Cookies["jwtToken"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            #endregion

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

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

            // Redirect to a specific page according to error status code.
            app.UseStatusCodePages(context =>
            {
                var response = context.HttpContext.Response;

                if (response.StatusCode == 401)
                {
                    response.Redirect("/Users/Login");
                }
                else if (response.StatusCode == 403)
                {
                    response.Redirect("/Users/AccessDenied");
                }
                else if (response.StatusCode == 404)
                {
                    response.Redirect("/Users/NotFoundPage");
                }

                return Task.CompletedTask;
            });

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
