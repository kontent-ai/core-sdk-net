namespace Kontent.Ai.Core.Abstractions;

/// <summary>
/// Interface for executing HTTP requests with automatic serialization, resilience, and tracking headers.
/// Supports all CRUD operations.
/// </summary>
public interface IActionInvoker
{
    /// <summary>
    /// Executes a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> GetAsync<TResponse>(
        string endpoint, 
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a POST request with a payload and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a PUT request with a payload and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a PATCH request with a payload and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> PatchAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a DELETE request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> DeleteAsync<TResponse>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a POST request without expecting a response body.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PostAsync<TRequest>(
        string endpoint, 
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a DELETE request without expecting a response body.
    /// </summary>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file using multipart/form-data.
    /// </summary>
    /// <typeparam name="TResponse">The response type to deserialize to.</typeparam>
    /// <param name="endpoint">The endpoint URL (relative or absolute).</param>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> UploadFileAsync<TResponse>(
        string endpoint,
        Stream fileStream,
        string fileName,
        string contentType,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
} 