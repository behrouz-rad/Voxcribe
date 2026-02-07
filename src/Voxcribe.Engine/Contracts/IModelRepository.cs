// © 2026 Behrouz Rad. All rights reserved.

using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Contracts;

/// <summary>
/// Manages speech recognition model lifecycle and metadata.
/// </summary>
public interface IModelRepository
{
    /// <summary>
    /// Retrieves metadata for all available models.
    /// </summary>
    public Task<IReadOnlyList<ModelDescriptor>> GetAllModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves metadata for a specific model.
    /// </summary>
    public Task<ModelDescriptor> GetModelAsync(ModelSize size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the local file path for a model.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if model is not available locally.</exception>
    public Task<string> GetModelFilePathAsync(ModelSize size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a model from the remote repository.
    /// </summary>
    /// <param name="size">The model size to download.</param>
    /// <param name="progress">Progress reporter (0.0 to 1.0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task AcquireModelAsync(
        ModelSize size,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a model from local storage.
    /// </summary>
    public Task RemoveModelAsync(ModelSize size, CancellationToken cancellationToken = default);
}
