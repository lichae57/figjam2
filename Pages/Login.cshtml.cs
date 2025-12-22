using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using figjam2.Services;
using System.Text.Json;
using System.Linq;

namespace figjam2.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ApiClient apiClient, ILogger<LoginModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [BindProperty]
        public string? Identifier { get; set; }

        [BindProperty]
        public string? OtpCode { get; set; }

        [BindProperty]
        public bool KvkkAccepted { get; set; }

        [BindProperty]
        public string? LoginType { get; set; } // "tc" or "phone"

        public IActionResult OnGet()
        {
            // If already logged in, redirect to Dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Dashboard");
            }
                return Page();
            }

        // TCKN/GSM Doğrulama
        public async Task<IActionResult> OnPostVerifyTcknGsm()
        {
            try
            {
                _logger.LogInformation("OnPostVerifyTcknGsm çağrıldı. ContentType: {ContentType}", Request.ContentType);
                
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                string tckn = "";
                string gsm = "";
                
                // Önce FormData'dan dene
                if (Request.HasFormContentType && Request.Form.ContainsKey("Tckn"))
                {
                    tckn = Request.Form["Tckn"].ToString()?.Trim() ?? "";
                    gsm = Request.Form["Gsm"].ToString()?.Trim() ?? "";
                    _logger.LogInformation("OnPostVerifyTcknGsm: FormData'dan okundu. TCKN: '{Tckn}', GSM: '{Gsm}'", tckn, gsm);
                }
                else
                {
                    // JSON body'den oku
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true, bufferSize: 1024);
                    var body = await reader.ReadToEndAsync();
                    
                    _logger.LogInformation("OnPostVerifyTcknGsm: Request body uzunluğu: {Length}, İlk 200 karakter: {Body}", 
                        body?.Length ?? 0, body?.Substring(0, Math.Min(200, body?.Length ?? 0)) ?? "BOŞ");
                    
                    if (Request.Body.CanSeek)
                    {
                        Request.Body.Position = 0;
                    }
                    
                    if (!string.IsNullOrEmpty(body))
                    {
                        try
                        {
                            var jsonDoc = JsonDocument.Parse(body);
                            if (jsonDoc.RootElement.TryGetProperty("Tckn", out var tcknElement))
                            {
                                tckn = tcknElement.GetString()?.Trim() ?? "";
                            }
                            if (jsonDoc.RootElement.TryGetProperty("Gsm", out var gsmElement))
                            {
                                gsm = gsmElement.GetString()?.Trim() ?? "";
                            }
                            // Alternatif property isimleri
                            if (string.IsNullOrEmpty(tckn) && jsonDoc.RootElement.TryGetProperty("tckn", out var tcknLower))
                            {
                                tckn = tcknLower.GetString()?.Trim() ?? "";
                            }
                            if (string.IsNullOrEmpty(gsm) && jsonDoc.RootElement.TryGetProperty("gsm", out var gsmLower))
                            {
                                gsm = gsmLower.GetString()?.Trim() ?? "";
                            }
                            _logger.LogInformation("OnPostVerifyTcknGsm: JSON'dan okundu. TCKN: '{Tckn}', GSM: '{Gsm}'", tckn, gsm);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "OnPostVerifyTcknGsm: Geçersiz JSON formatı. Body: {Body}", body?.Substring(0, Math.Min(100, body?.Length ?? 0)));
                            return new JsonResult(new { success = false, error = $"Geçersiz JSON formatı: {ex.Message}" })
                            {
                                StatusCode = 400
                            };
                        }
                    }
                    else
                    {
                        _logger.LogWarning("OnPostVerifyTcknGsm: Request body boş!");
                    }
                }

                _logger.LogInformation("OnPostVerifyTcknGsm: Request body okundu. TCKN: '{Tckn}', GSM: '{Gsm}', ContentType: {ContentType}", 
                    tckn, gsm, Request.ContentType);

                if (string.IsNullOrWhiteSpace(tckn) || string.IsNullOrWhiteSpace(gsm))
                {
                    _logger.LogWarning("OnPostVerifyTcknGsm: TCKN veya GSM eksik. TCKN: '{Tckn}', GSM: '{Gsm}'", tckn, gsm);
                    return new JsonResult(new { success = false, error = "TCKN ve telefon numarası gereklidir" })
                    {
                        StatusCode = 400
                    };
                }

                // GSM numarasını temizle (başındaki 0'ı kaldır)
                var cleanedGsm = new string(gsm.Where(char.IsDigit).ToArray());
                if (cleanedGsm.Length == 11 && cleanedGsm.StartsWith("0"))
                {
                    cleanedGsm = cleanedGsm.Substring(1);
                }

                var requestBody = new
                {
                    tckn = tckn,
                    gsm = cleanedGsm
                };

                _logger.LogInformation("OnPostVerifyTcknGsm: TCKN={Tckn}, GSM={Gsm}, API URL={Url}", tckn, cleanedGsm, ApiConfig.TCKN_GSM);
                
                ApiResponse<object> response;
                try
                {
                    response = await _apiClient.PostAsync<object>(ApiConfig.TCKN_GSM, requestBody, HttpContext);
                    _logger.LogInformation("OnPostVerifyTcknGsm: API Response Status={Status}, IsSuccess={IsSuccess}, Error={Error}", 
                        response.Status, response.IsSuccess, response.Error);
                }
                catch (Exception apiEx)
                {
                    _logger.LogError(apiEx, "OnPostVerifyTcknGsm: API çağrısı başarısız. Next.js API çalışmıyor olabilir. URL: {Url}", ApiConfig.TCKN_GSM);
                    return new JsonResult(new { success = false, error = $"API bağlantı hatası: {apiEx.Message}. Lütfen Next.js API'nin çalıştığından emin olun (http://localhost:3000)" })
                    {
                        StatusCode = 503
                    };
                }

                if (response.IsSuccess && response.Data != null)
                {
                    // Customer ID'yi session'a kaydet
                    var json = JsonSerializer.Serialize(response.Data);
                    var jsonDoc = JsonDocument.Parse(json);
                    
                    // Farklı response formatlarını kontrol et
                    string? customerIdStr = null;
                    
                    // data.value.CustomerId formatı
                    if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.ValueKind == JsonValueKind.Object)
                        {
                            if (dataElement.TryGetProperty("value", out var valueElement))
                            {
                                if (valueElement.ValueKind == JsonValueKind.Object)
                                {
                                    if (valueElement.TryGetProperty("CustomerId", out var customerIdUpper))
                                    {
                                        customerIdStr = customerIdUpper.ValueKind == JsonValueKind.String 
                                            ? customerIdUpper.GetString() 
                                            : customerIdUpper.GetInt32().ToString();
                                    }
                                    else if (valueElement.TryGetProperty("customerId", out var customerIdLower))
                                    {
                                        customerIdStr = customerIdLower.ValueKind == JsonValueKind.String 
                                            ? customerIdLower.GetString() 
                                            : customerIdLower.GetInt32().ToString();
                                    }
                                }
                            }
                        }
                    }
                    // Direkt customerId
                    else if (jsonDoc.RootElement.TryGetProperty("customerId", out var customerIdDirect))
                    {
                        customerIdStr = customerIdDirect.ValueKind == JsonValueKind.String 
                            ? customerIdDirect.GetString() 
                            : customerIdDirect.GetInt32().ToString();
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("CustomerId", out var customerIdDirectUpper))
                    {
                        customerIdStr = customerIdDirectUpper.ValueKind == JsonValueKind.String 
                            ? customerIdDirectUpper.GetString() 
                            : customerIdDirectUpper.GetInt32().ToString();
                    }
                    
                    if (!string.IsNullOrEmpty(customerIdStr))
                    {
                        HttpContext.Session.SetString("CustomerId", customerIdStr);
                        _logger.LogInformation("CustomerId session'a kaydedildi: {CustomerId}", customerIdStr);
                    }
                    else
                    {
                        _logger.LogWarning("CustomerId response'da bulunamadı. Response: {Response}", json);
                    }

                    // GSM numarasını session'a kaydet (OTP için gerekli)
                    HttpContext.Session.SetString("Gsm", cleanedGsm);
                    HttpContext.Session.SetString("Tckn", tckn);
                    _logger.LogInformation("GSM ve TCKN session'a kaydedildi. GSM: {Gsm}, TCKN: {Tckn}", cleanedGsm, tckn);

                    return new JsonResult(new { success = true, data = response.Data, gsm = cleanedGsm });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "Doğrulama başarısız" })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostVerifyTcknGsm: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = $"Sunucu hatası: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }

        // OTP Üret
        public async Task<IActionResult> OnPostGenerateOtp()
        {
            try
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                string body;
                using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true, bufferSize: 1024))
                {
                    body = await reader.ReadToEndAsync();
                }
                
                if (Request.Body.CanSeek)
                {
                    Request.Body.Position = 0;
                }

                string? gsm = null;
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        if (jsonDoc.RootElement.TryGetProperty("gsm", out var gsmElement))
                        {
                            gsm = gsmElement.GetString()?.Trim();
                        }
                    }
                    catch (JsonException)
                    {
                        // JSON parse edilemezse session'dan al
                    }
                }
                
                // Session'dan al
                if (string.IsNullOrEmpty(gsm))
                {
                    gsm = HttpContext.Session.GetString("Gsm");
                }

                var tckn = HttpContext.Session.GetString("Tckn");

                if (string.IsNullOrEmpty(gsm))
                {
                    _logger.LogWarning("OnPostGenerateOtp: GSM numarası bulunamadı");
                    return new JsonResult(new { success = false, error = "GSM numarası bulunamadı. Lütfen önce TCKN/GSM doğrulaması yapın." })
                    {
                        StatusCode = 400
                    };
                }

                if (string.IsNullOrEmpty(tckn))
                {
                    _logger.LogWarning("OnPostGenerateOtp: TCKN bulunamadı");
                    return new JsonResult(new { success = false, error = "TCKN bulunamadı. Lütfen önce TCKN/GSM doğrulaması yapın." })
                    {
                        StatusCode = 400
                    };
                }

                // GSM numarasını temizle
                var cleanedGsm = new string(gsm.Where(char.IsDigit).ToArray());
                if (cleanedGsm.Length == 11 && cleanedGsm.StartsWith("0"))
                {
                    cleanedGsm = cleanedGsm.Substring(1);
                }

                // API hem TCKN hem de GSM bekliyor
                var requestBody = new
                {
                    tckn = tckn,
                    gsm = cleanedGsm
                };

                _logger.LogInformation("OnPostGenerateOtp: TCKN={Tckn}, GSM={Gsm}", tckn, cleanedGsm);
                var response = await _apiClient.PostAsync<object>(ApiConfig.GENERATE_OTP, requestBody, HttpContext);

                if (response.IsSuccess && response.Data != null)
                {
                    // OTP kodunu session'a kaydet
                    var json = JsonSerializer.Serialize(response.Data);
                    var jsonDoc = JsonDocument.Parse(json);
                    
                    string? otpCode = null;
                    
                    // Farklı response formatlarını kontrol et
                    if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.ValueKind == JsonValueKind.Object)
                        {
                            if (dataElement.TryGetProperty("value", out var valueElement))
                            {
                                if (valueElement.ValueKind == JsonValueKind.Object)
                                {
                                    if (valueElement.TryGetProperty("OTPCode", out var otpCodeUpper))
                                    {
                                        otpCode = otpCodeUpper.ValueKind == JsonValueKind.String 
                                            ? otpCodeUpper.GetString() 
                                            : otpCodeUpper.GetInt32().ToString();
                                    }
                                    else if (valueElement.TryGetProperty("otpCode", out var otpCodeLower))
                                    {
                                        otpCode = otpCodeLower.ValueKind == JsonValueKind.String 
                                            ? otpCodeLower.GetString() 
                                            : otpCodeLower.GetInt32().ToString();
                                    }
                                }
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(otpCode))
                    {
                        HttpContext.Session.SetString("OtpCode", otpCode);
                        _logger.LogInformation("OnPostGenerateOtp: OTP kodu session'a kaydedildi: {OtpCode}", otpCode);
                    }
                    else
                    {
                        _logger.LogWarning("OnPostGenerateOtp: OTP kodu response'da bulunamadı. Response: {Response}", json);
                    }
                    
                    return new JsonResult(new { success = true, data = response.Data });
                }

                _logger.LogWarning("OnPostGenerateOtp: API başarısız. Status={Status}, Error={Error}", response.Status, response.Error);
                return new JsonResult(new { success = false, error = response.Error ?? "OTP üretilemedi" })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostGenerateOtp: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = $"Sunucu hatası: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }

        // OTP SMS Gönder
        public async Task<IActionResult> OnPostSendOtpSms()
        {
            try
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                string body;
                using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true, bufferSize: 1024))
                {
                    body = await reader.ReadToEndAsync();
                }
                
                if (Request.Body.CanSeek)
                {
                    Request.Body.Position = 0;
                }

                string? gsm = null;
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        if (jsonDoc.RootElement.TryGetProperty("gsm", out var gsmElement))
                        {
                            gsm = gsmElement.GetString()?.Trim();
                        }
                    }
                    catch (JsonException)
                    {
                        // JSON parse edilemezse session'dan al
                    }
                }
                
                // Session'dan al
                if (string.IsNullOrEmpty(gsm))
                {
                    gsm = HttpContext.Session.GetString("Gsm");
                }

                var otpCode = HttpContext.Session.GetString("OtpCode");

                if (string.IsNullOrEmpty(gsm))
                {
                    _logger.LogWarning("OnPostSendOtpSms: GSM numarası bulunamadı");
                    return new JsonResult(new { success = false, error = "GSM numarası bulunamadı. Lütfen önce TCKN/GSM doğrulaması yapın." })
                    {
                        StatusCode = 400
                    };
                }

                if (string.IsNullOrEmpty(otpCode))
                {
                    _logger.LogWarning("OnPostSendOtpSms: OTP kodu bulunamadı");
                    return new JsonResult(new { success = false, error = "OTP kodu bulunamadı. Lütfen önce OTP üretin." })
                    {
                        StatusCode = 400
                    };
                }

                // GSM numarasını temizle
                var cleanedGsm = new string(gsm.Where(char.IsDigit).ToArray());
                if (cleanedGsm.Length == 11 && cleanedGsm.StartsWith("0"))
                {
                    cleanedGsm = cleanedGsm.Substring(1);
                }

                // API hem GSM hem de OTP kodu bekliyor
                var requestBody = new
                {
                    gsm = cleanedGsm,
                    otpCode = otpCode
                };

                _logger.LogInformation("OnPostSendOtpSms: GSM={Gsm}, OtpCode={OtpCode}", cleanedGsm, otpCode);
                var response = await _apiClient.PostAsync<object>(ApiConfig.SEND_OTP_SMS, requestBody, HttpContext);

                if (response.IsSuccess)
                {
                    _logger.LogInformation("OnPostSendOtpSms: SMS başarıyla gönderildi");
                    return new JsonResult(new { success = true, data = response.Data });
                }

                _logger.LogWarning("OnPostSendOtpSms: API başarısız. Status={Status}, Error={Error}", response.Status, response.Error);
                return new JsonResult(new { success = false, error = response.Error ?? "SMS gönderilemedi" })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostSendOtpSms: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = $"Sunucu hatası: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }

        // KVKK Metni Getir
        public async Task<IActionResult> OnGetKvkkText(int id = 1)
        {
            try
            {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetKvkkTextUrl(id), HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // KVKK Onay
        public async Task<IActionResult> OnPostKvkkOnay()
        {
            try
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                string body;
                using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true, bufferSize: 1024))
                {
                    body = await reader.ReadToEndAsync();
                }
                
                if (Request.Body.CanSeek)
                {
                    Request.Body.Position = 0;
                }

                int kvkkId = 1; // Default
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        if (jsonDoc.RootElement.TryGetProperty("kvkkId", out var kvkkIdElement))
                        {
                            kvkkId = kvkkIdElement.GetInt32();
                        }
                    }
                    catch (JsonException)
                    {
                        // JSON parse edilemezse default değer kullan
                    }
                }

                var customerId = HttpContext.Session.GetString("CustomerId") ?? "";
                if (string.IsNullOrEmpty(customerId))
                {
                    return new JsonResult(new { success = false, error = "Müşteri bilgisi bulunamadı. Lütfen önce TCKN/GSM doğrulaması yapın." })
                    {
                        StatusCode = 400
                    };
                }

                var requestBody = new
                {
                    customerId = int.Parse(customerId),
                    kvkkId = kvkkId,
                    approved = true,
                    timestamp = DateTime.UtcNow
                };

                var response = await _apiClient.PostAsync<object>(ApiConfig.KVKK_ONAY, requestBody, HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // OTP Doğrulama
        public async Task<IActionResult> OnPostVerifyOtp()
        {
            try
            {
                var otpCode = Request.Form["OtpCode"].ToString()?.Trim() ?? "";
                var gsm = Request.Form["Gsm"].ToString()?.Trim() ?? HttpContext.Session.GetString("Gsm") ?? "";

                if (string.IsNullOrEmpty(gsm))
                {
                    return new JsonResult(new { success = false, error = "GSM numarası bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }

                var requestBody = new
                {
                    gsm = gsm,
                    otpCode = otpCode
                };

                var response = await _apiClient.PostAsync<object>(ApiConfig.VERIFY_OTP, requestBody, HttpContext);

                if (response.IsSuccess && response.Data != null)
                {
                    // Token'ı session'a kaydet
                    var json = JsonSerializer.Serialize(response.Data);
                    var jsonDoc = JsonDocument.Parse(json);
                    
                    string? token = null;
                    if (jsonDoc.RootElement.TryGetProperty("token", out var tokenElement))
                    {
                        token = tokenElement.GetString();
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessToken))
                    {
                        token = accessToken.GetString();
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("jwt", out var jwt))
                    {
                        token = jwt.GetString();
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        _apiClient.SetToken(HttpContext, token);
                    }

                    // Set session as logged in
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
                    HttpContext.Session.SetString("Identifier", Identifier ?? "");

                    // Create claims for cookie authentication
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "Ahmet Yılmaz"),
                        new Claim(ClaimTypes.NameIdentifier, Identifier ?? ""),
                        new Claim("IsLoggedIn", "true")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    return new JsonResult(new { success = true, redirect = "/Dashboard" });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "OTP doğrulama başarısız" })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> OnPostLogin()
        {
            // Set session as logged in
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
            HttpContext.Session.SetString("Identifier", Identifier ?? "");

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Ahmet Yılmaz"),
                new Claim(ClaimTypes.NameIdentifier, Identifier ?? ""),
                new Claim("IsLoggedIn", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Dashboard");
        }
    }
}
