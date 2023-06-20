using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Realtime.Infrastructure.Implementation.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realtime.Infrastructure.Extensions;
using Realtime.Domain.Options;
using Common.Implementations.Extensions;
using Common.Domain.Options;
using System.Reflection;
using System.IO;
using Common.Implementations.NotificationServices.Extensions;
using Common.Implementations.ExceptionImplementations.Extensions;
using Microsoft.AspNetCore.Hosting;
using Common.Implementations.InnerHttpClient;

namespace Realtime.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _swaggerOptions = new SwaggerOptions();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(_swaggerOptions);

            _realTimeOptions = new RealTimeOptions();
            Configuration.GetSection(nameof(RealTimeOptions)).Bind(_realTimeOptions);

            _OAuthOptions = new OAuthOptions();
            Configuration.GetSection(nameof(OAuthOptions)).Bind(_OAuthOptions);

            _httpServiceOptions = new HttpServiceOptions();
            Configuration.GetSection(nameof(HttpServiceOptions)).Bind(_httpServiceOptions);
        }

        private readonly SwaggerOptions _swaggerOptions;

        private readonly RealTimeOptions _realTimeOptions;

        private readonly OAuthOptions _OAuthOptions;

        private readonly HttpServiceOptions _httpServiceOptions;


        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureOptions(Configuration);
            services.Configure<RealTimeOptions>(Configuration.GetSection("RealTimeOptions"));


            services.AddControllers();

            services.AddWebSocketConnectionServices();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.AddSwagger(_swaggerOptions, _OAuthOptions, xmlPath);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.Authority = _OAuthOptions.AuthServer;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.Audience = _OAuthOptions.ApiResourse;

                options.RequireHttpsMetadata = false;
            });
            services.AddHttpClient<TokenProvider>(client =>
            {
                client.BaseAddress = new Uri(_OAuthOptions.AuthServer);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<ChatHttpService>(client =>
            {
                client.BaseAddress = new Uri(_httpServiceOptions.ChatApi);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddNotificationsService();
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(option => option.AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseHsts();
            app.UseWebSockets()
               .MapWebSocketConnections(_realTimeOptions.WebSocketPath);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();
            app.ConfigureSwagger(_swaggerOptions, _OAuthOptions);

            app.UseCustomExceptionMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
