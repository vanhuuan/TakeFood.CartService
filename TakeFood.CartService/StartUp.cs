﻿using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using CartService.Extension;
using CartService.Model.Entities.Address;
using CartService.Model.Entities.Category;
using CartService.Model.Entities.Food;
using CartService.Model.Entities.Order;
using CartService.Model.Entities.Review;
using CartService.Model.Entities.Role;
using CartService.Model.Entities.Store;
using CartService.Model.Entities.Topping;
using CartService.Model.Entities.User;
using CartService.Model.Entities.WorkTime;
using CartService.Model.Repository;
using CartService.Settings;
using System.Diagnostics;
using System.Text.Json;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using StoreService.Service;
using CartService.Model.Entities.Image;
using StoreService.Service.Implement;
using TakeFood.CartService.Service;
using TakeFood.CartService.Service.Implement;
using CartService.Middleware;

namespace TakeFood.CartService;

public class Startup
{
    /// <summary>
    /// Start up
    /// </summary>
    /// <param name="env"></param>
    public Startup(IWebHostEnvironment env)
    {
        try
        {
            var appSettingString = GetAppSettingString(env);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{appSettingString}.json", optional: true)
                .AddEnvironmentVariables("APPSETTING_");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get AppSetting String
    /// </summary>
    /// <param name="env"></param>
    /// <returns></returns>
    private string GetAppSettingString(IWebHostEnvironment env)
    {
        string hostName = env.EnvironmentName;
        if (!string.IsNullOrEmpty(hostName))
        {
            if (hostName.ToLower().Contains("test"))
            {
                return "test";
            }
            else if (hostName.ToLower().Contains("dev"))
            {
                return "dev";
            }
        }
        return env.EnvironmentName;
    }

    /// <summary>
    /// Configuration
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// App setting
    /// </summary>
    private AppSetting appSetting { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        var appSettingsSection = Configuration.GetSection("AppSettings");
        services.Configure<AppSetting>(appSettingsSection);
        appSetting = appSettingsSection.Get<AppSetting>();

        services.AddMvc((options) =>
        {
            options.EnableEndpointRouting = true;
        }).AddJsonOptions((options) =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        services.AddControllers();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Store API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
        });
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();
        services.AddAuthentication();

        string databaseName = appSetting.NoSQL.DatabaseName;
        string mongoConnectionString = $"{appSetting.NoSQL?.ConnectionString}{appSetting.NoSQL?.ConnectionSetting}";

        services.AddMongoDb(mongoConnectionString, databaseName);

        services.AddSingleton(appSetting);

        // setting serialize decimal data type to bson
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

        services.AddMongoRepository<User>(appSetting.NoSQL.Collections.User);
        services.AddMongoRepository<Role>(appSetting.NoSQL.Collections.Role);
        services.AddMongoRepository<UserRefreshToken>(appSetting.NoSQL.Collections.UserRefreshToken);
        services.AddMongoRepository<Account>(appSetting.NoSQL.Collections.Account);
        services.AddMongoRepository<Store>(appSetting.NoSQL.Collections.Store);
        services.AddMongoRepository<Food>(appSetting.NoSQL.Collections.Food);
        services.AddMongoRepository<Category>(appSetting.NoSQL.Collections.Category);
        services.AddMongoRepository<Address>(appSetting.NoSQL.Collections.Address);
        services.AddMongoRepository<Image>(appSetting.NoSQL.Collections.Image);
        services.AddMongoRepository<StoreCategory>(appSetting.NoSQL.Collections.StoreCategory);
        services.AddMongoRepository<Topping>(appSetting.NoSQL.Collections.Topping);
        services.AddMongoRepository<FoodTopping>(appSetting.NoSQL.Collections.FoodTopping);
        services.AddMongoRepository<WorkTime>(appSetting.NoSQL.Collections.WorkTime);
        services.AddMongoRepository<Review>(appSetting.NoSQL.Collections.Review);
        services.AddMongoRepository<Order>(appSetting.NoSQL.Collections.Order);

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMailService, MailService>();
/*        services.AddScoped<IStoreService, StoreService>();*/
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IFoodService, FoodService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IToppingService, ToppingService>();

        services.AddScoped<IJwtService, JwtService>(x => new JwtService(x.GetRequiredService<IMongoRepository<UserRefreshToken>>()
           , appSetting.JwtConfig.Secret, appSetting.JwtConfig.Secret2, appSetting.JwtConfig.ExpirationInHours, appSetting.JwtConfig.ExpirationInMonths));

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                    //.AllowCredentials();
                }
            );
        });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    /// <param name="app"></param>
    public void Configure(IApplicationBuilder app)
    {
        try
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseRouting();

            app.UseMiddleware<AuthenticationMiddleware>();
            // app.UseMiddleware<UserMiddleware>();

            app.UseDefaultFiles();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
        catch (Exception ex)
        {

        }
    }
}
