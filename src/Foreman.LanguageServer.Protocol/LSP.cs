global using Foreman.Core;

using System.Text.Json;
using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Services;
using Foreman.LanguageServer.Protocol.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foreman.LanguageServer.Protocol;

public static class LSP {
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static HostApplicationBuilder BaseBuilder(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<LspServer>();
        builder.Services.AddSingleton<IDebugLogger, FileDebugLogger>();
        builder.Services.AddSingleton<IClientNotifier, ClientNotifier>();
        builder.Services.AddSingleton(JsonSerializerOptions);
        builder.Services.AddSingleton<ContentLengthParser>();
        builder.Services.AddSingleton<MessageHandler>();
        return builder;
    }

    public static HostApplicationBuilder StdioBuilder(string[] args) {
        HostApplicationBuilder builder = BaseBuilder(args);
        builder.Services.AddSingleton<IStreamingInput, StdioStreamingInput>();
        builder.Services.AddSingleton<IStreamingOutput, StdioStreamingOutput>();
        return builder;
    }

    public static IServiceCollection AddRequest<TRequest, THandler>(this IServiceCollection services) 
    where TRequest : ILspRequest 
    where THandler : RequestHandlerBase<TRequest> {
        services.AddScoped<RequestHandlerBase<TRequest>, THandler>();
        return services;
    }

    public static IServiceCollection AddNotification<TNotification, THandler>(this IServiceCollection services) 
    where TNotification : ILspNotification
    where THandler : NotificationHandlerBase<TNotification> {
        services.AddScoped<NotificationHandlerBase<TNotification>, THandler>();
        return services;
    }
}