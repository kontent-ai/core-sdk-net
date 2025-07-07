using System.Text;
using System.Text.Json;
using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Extensions;

namespace Kontent.Ai.Core.Modules.ActionInvoker;

/// <summary>
/// Modern action invoker implementation using Microsoft.Extensions.Http.Resilience for resilience policies.
/// All resilience policies are configured on the HttpClient via Microsoft.Extensions.Http.Resilience.
/// Supports all CRUD operations.
/// </summary>
public class ActionInvoker : IActionInvoker
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Assembly _sdkAssembly;

    /// <summary>
    /// Constructor for ActionInvoker.
    /// Resilience policies should be configured on the HttpClient via Microsoft.Extensions.Http.Resilience.
    /// </summary>
    public ActionInvoker(
        HttpClient httpClient, 
        JsonSerializerOptions? jsonOptions = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        _sdkAssembly = Assembly.GetCallingAssembly(); // TODO: inject from the SDK
    }

    /// <summary>
    /// Executes a GET request and deserializes the response.
    /// </summary>
    public Task<TResponse> GetAsync<TResponse>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        => SendInternalAsync<TResponse>(HttpMethod.Get, endpoint, default, headers, cancellationToken);

    /// <summary>
    /// Executes a POST request with a payload and deserializes the response.
    /// </summary>
    public Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        => SendInternalAsync<TResponse>(HttpMethod.Post, endpoint, payload, headers, cancellationToken);

    /// <summary>
    /// Executes a PUT request with a payload and deserializes the response.
    /// </summary>
    public Task<TResponse> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        => SendInternalAsync<TResponse>(HttpMethod.Put, endpoint, payload, headers, cancellationToken);

    /// <summary>
    /// Executes a PATCH request with a payload and deserializes the response.
    /// </summary>
    public Task<TResponse> PatchAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        => SendInternalAsync<TResponse>(HttpMethod.Patch, endpoint, payload, headers, cancellationToken);

    /// <summary>
    /// Executes a DELETE request and deserializes the response.
    /// </summary>
    public Task<TResponse> DeleteAsync<TResponse>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        => SendInternalAsync<TResponse>(HttpMethod.Delete, endpoint, default, headers, cancellationToken);

    /// <summary>
    /// Executes a POST request without expecting a response body.
    /// </summary>
    public async Task PostAsync<TRequest>(
        string endpoint,
        TRequest payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequestWithPayload(HttpMethod.Post, endpoint, payload, headers);
        var response = await SendInternalAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Executes a DELETE request without expecting a response body.
    /// </summary>
    public async Task DeleteAsync(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Delete, endpoint, headers);
        var response = await SendInternalAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Uploads a file using multipart/form-data.
    /// </summary>
    public async Task<TResponse> UploadFileAsync<TResponse>(
        string endpoint,
        Stream fileStream,
        string fileName,
        string contentType,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var request = CreateRequest(HttpMethod.Post, endpoint, headers);
        request.Content = content;

        var response = await SendInternalAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response);
    }

    /// <summary>
    /// Internal method that handles all HTTP operations with resilience policies.
    /// </summary>
    private async Task<TResponse> SendInternalAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        object? payload,
        Dictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        var request = payload != null
            ? CreateRequestWithPayload(method, endpoint, payload, headers)
            : CreateRequest(method, endpoint, headers);

        var response = await SendInternalAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response);
    }

    /// <summary>
    /// Internal method that sends HTTP requests with tracking headers.
    /// Resilience policies are automatically applied by the HttpClient via Microsoft.Extensions.Http.Resilience.
    /// </summary>
    private async Task<HttpResponseMessage> SendInternalAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        AddTrackingHeaders(request, _sdkAssembly);
        
        // HttpClient automatically applies configured resilience policies (retry, circuit breaker, etc.)
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Creates an HTTP request with the specified method and endpoint.
    /// </summary>
    private static HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        if (headers != null)
        {
            foreach (var (name, value) in headers)
            {
                request.Headers.Add(name, value);
            }
        }

        return request;
    }

    /// <summary>
    /// Creates an HTTP request with a JSON payload.
    /// </summary>
    private HttpRequestMessage CreateRequestWithPayload<T>(HttpMethod method, string endpoint, T payload, Dictionary<string, string>? headers = null)
    {
        var request = CreateRequest(method, endpoint, headers);
        
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// Deserializes the HTTP response content to the specified type.
    /// </summary>
    private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(content))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
    }

    /// <summary>
    /// Adds SDK and source tracking headers to the request.
    /// </summary>
    private static void AddTrackingHeaders(HttpRequestMessage request, Assembly? sdkAssembly = null)
    {
        request.Headers.AddSdkTrackingHeader(sdkAssembly);
        request.Headers.AddSourceTrackingHeader();
    }
} 