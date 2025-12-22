using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace figjam2.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const string DEFAULT_TOKEN = "fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s";

        public ApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private string GetToken(HttpContext? httpContext)
        {
            // Önce session'dan token al
            if (httpContext != null)
            {
                var sessionToken = httpContext.Session.GetString("ApiToken");
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    return sessionToken;
                }
            }
            return DEFAULT_TOKEN;
        }

        public async Task<ApiResponse<T>> PostAsync<T>(
            string url,
            object? body = null,
            HttpContext? httpContext = null)
        {
            try
            {
                var token = GetToken(httpContext);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Connection refused veya benzeri hatalar için kontrol
                if (!response.IsSuccessStatusCode && string.IsNullOrEmpty(responseContent))
                {
                    return new ApiResponse<T>
                    {
                        Status = (int)response.StatusCode,
                        IsSuccess = false,
                        Error = $"API bağlantı hatası: {response.StatusCode}. Next.js API çalışmıyor olabilir (http://localhost:3000)"
                    };
                }

                return ParseNextJsResponse<T>(responseContent, response);
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    Status = 503,
                    IsSuccess = false,
                    Error = $"API bağlantı hatası: {httpEx.Message}. Next.js API çalışmıyor olabilir (http://localhost:3000)"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Status = 500,
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> GetAsync<T>(
            string url,
            HttpContext? httpContext = null)
        {
            try
            {
                var token = GetToken(httpContext);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                return ParseNextJsResponse<T>(responseContent, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Status = 500,
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
        }

        private ApiResponse<T> ParseNextJsResponse<T>(string responseContent, HttpResponseMessage response)
        {
            T? data = default;
            if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    // Önce Next.js API response formatını kontrol et: { status, data, elapsed, error? }
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) && 
                        jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                    {
                        // Next.js API response formatı: { status, data, elapsed }
                        data = JsonSerializer.Deserialize<T>(dataElement.GetRawText(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        var nextjsStatus = statusElement.GetInt32();
                        return new ApiResponse<T>
                        {
                            Status = nextjsStatus,
                            Data = data,
                            IsSuccess = nextjsStatus >= 200 && nextjsStatus < 300,
                            Error = jsonDoc.RootElement.TryGetProperty("error", out var errorElement) 
                                ? errorElement.GetString() 
                                : null
                        };
                    }
                    else
                    {
                        // Direkt Azure API response (Next.js API değil)
                        data = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch
                {
                    // JSON parse edilemezse string olarak döndür
                }
            }

            return new ApiResponse<T>
            {
                Status = (int)response.StatusCode,
                Data = data,
                IsSuccess = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : responseContent
            };
        }

        public void SetToken(HttpContext httpContext, string token)
        {
            httpContext.Session.SetString("ApiToken", token);
        }
    }

    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
    }
}

