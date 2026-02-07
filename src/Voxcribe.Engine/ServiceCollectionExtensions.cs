// © 2026 Behrouz Rad. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Infrastructure;
using Voxcribe.Engine.Services;

namespace Voxcribe.Engine;

/// <summary>
/// Extension methods for registering Voxcribe Engine services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Voxcribe Engine services with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration callback.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoxcribeEngine(
        this IServiceCollection services,
        Action<EngineConfiguration>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<EngineConfiguration>(_ => { });
        }

        services.TryAddSingleton<IModelRepository, ModelRepository>();
        services.TryAddSingleton<IMediaProcessor, MediaProcessor>();
        services.TryAddSingleton<ISpeechRecognitionEngine, SpeechRecognitionEngine>();

        services.TryAddSingleton<ITranscriptionOrchestrator, TranscriptionOrchestrator>();

        services.AddHttpClient<ModelRepository>();

        return services;
    }
}
