using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Domain.Abstractions;
using Chat.Infrastructure.Extensions;
using Chat.Infrastructure.Implementation;
using Chat.Persistence.Contexts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Chat.Infrastructure.Validations;
using Chat.Domain.DTOs.FiltersDTO;
using Common.Implementations.Extensions;
using Common.Domain.Options;
using System.Reflection;
using System.IO;
using Common.Domain.DTOs.ChatDTOs;
using Common.Domain.DTOs.MessageDTOs;
using Common.Implementations.InnerHttpClient;
using Common.Implementations.NotificationServices.Extensions;
using Common.Implementations.ExceptionImplementations.Extensions;
using System.Net.Http;

namespace Chat.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            SwaggerOptions = new SwaggerOptions();

            OAuthOptions = new OAuthOptions();

            Configuration.GetSection(nameof(SwaggerOptions)).Bind(SwaggerOptions);

            Configuration.GetSection(nameof(OAuthOptions)).Bind(OAuthOptions);
        }


        public SwaggerOptions SwaggerOptions { get; }

        public OAuthOptions OAuthOptions { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureOptions(Configuration);
            services.AddControllers()
                    .AddFluentValidation();


            services.AddTransient<IValidator<CreateChatDTO>, CreateChatValidator>();
            services.AddTransient<IValidator<CreateMessageDTO>, CreateMessageValidator>();
            services.AddTransient<IValidator<MessageFilterModel>, MessageFilterModelValidator>();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ChatDbContext>(options =>
                options.UseMySql(connectionString)).AddUnitOfWork<ChatDbContext>();


            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.AddSwagger(SwaggerOptions, OAuthOptions, xmlPath);
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.Authority = OAuthOptions.AuthServer;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.Audience = OAuthOptions.ApiResourse;

                options.RequireHttpsMetadata = false;
            });

            services.AddHttpClient<TokenProvider>(client =>
            {
                client.BaseAddress = new Uri(OAuthOptions.AuthServer);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IdentityHttpService>(client =>
            {
                client.BaseAddress = new Uri(OAuthOptions.AuthServer);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();

            
            services.AddNotificationsService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCustomExceptionMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();

            app.UseHttpsRedirection();
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ChatDbContext>().Database.Migrate();
            }

            app.UseCors(option => option
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            app.ConfigureSwagger(SwaggerOptions, OAuthOptions);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
