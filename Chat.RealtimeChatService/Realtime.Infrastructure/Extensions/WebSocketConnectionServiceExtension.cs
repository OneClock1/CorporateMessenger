using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Domain.Abstractions;
using Realtime.Domain.Options;
using Realtime.Infrastructure.Implementation;

namespace Realtime.Infrastructure.Extensions
{
    public static class WebSocketConnectionServiceExtension
    {
        public static IServiceCollection AddWebSocketConnectionServices(this IServiceCollection services)
        {
            services.AddSingleton<IWebSocketConnectionService, WebSocketConnectionService>();
            services.AddSingleton<IKeepAliveService, KeepAliveService>();

            return services;
        }
    }


}
