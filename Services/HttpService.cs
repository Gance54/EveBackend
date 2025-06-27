using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IndyBackend.Services
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data);
        Task<T> PutAsync<T>(string url, object data);
        Task<bool> DeleteAsync(string url);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpService()
        {
            _httpClient = new HttpClient();
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpServiceException($"HTTP GET request failed: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new HttpServiceException($"Failed to deserialize response: {ex.Message}", ex);
            }
        }

        public async Task<T> PostAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpServiceException($"HTTP POST request failed: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new HttpServiceException($"Failed to serialize/deserialize data: {ex.Message}", ex);
            }
        }

        public async Task<T> PutAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpServiceException($"HTTP PUT request failed: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new HttpServiceException($"Failed to serialize/deserialize data: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpServiceException($"HTTP DELETE request failed: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class HttpServiceException : Exception
    {
        public HttpServiceException(string message) : base(message) { }
        public HttpServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
} 