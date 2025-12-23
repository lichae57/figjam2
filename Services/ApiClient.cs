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
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                // Hem Bearer Token hem de Azure Code gönderelim (Gereksiz olsa bile zarar vermez)
                var token = GetToken(httpContext);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                if (body != null)
                {
                    // Naming policy'yi kaldırıyoruz, caller ne gönderirse o gitsin
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    Console.WriteLine($"[ApiClient] Sending to {url}");
                    Console.WriteLine($"[ApiClient] Request Body: {json}");
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ApiClient] POST Error to: {url}");
                    Console.WriteLine($"[ApiClient] Status Code: {(int)response.StatusCode}");
                    Console.WriteLine($"[ApiClient] Request Body sent: {JsonSerializer.Serialize(body)}");
                    Console.WriteLine($"[ApiClient] Response Content: {responseContent}");
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
            string? error = null;

            if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    
                    // Azure API hata formatı kontrolü: { success, statusCode, message, value }
                    if (jsonDoc.RootElement.TryGetProperty("message", out var msgElement))
                    {
                        error = msgElement.GetString();
                    }
                    
                    // Next.js API formatı kontrolü
                    if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) && 
                        jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                    {
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
                            Error = error ?? (jsonDoc.RootElement.TryGetProperty("error", out var errorElement) ? errorElement.GetString() : null)
                        };
                    }
                    else
                    {
                        // Direkt Azure API response
                        data = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch
                {
                    // JSON değilse veya parse edilemezse hata mesajı olarak içeriği al
                    if (!response.IsSuccessStatusCode) error = responseContent;
                }
            }

            return new ApiResponse<T>
            {
                Status = (int)response.StatusCode,
                Data = data,
                IsSuccess = response.IsSuccessStatusCode,
                Error = error ?? (response.IsSuccessStatusCode ? null : (!string.IsNullOrEmpty(responseContent) ? responseContent : response.ReasonPhrase))
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

