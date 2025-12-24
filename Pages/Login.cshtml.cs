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
                _logger.LogInformation("OnPostVerifyTcknGsm başladı. SessionID: {SessionId}", HttpContext.Session.Id);
                
                string tckn = "";
                string gsm = "";
                
                // Form'dan oku (Daha güvenilir)
                if (Request.HasFormContentType)
                {
                    tckn = Request.Form["Tckn"].ToString()?.Trim() ?? "";
                    gsm = Request.Form["Gsm"].ToString()?.Trim() ?? "";
                    _logger.LogInformation("OnPostVerifyTcknGsm: Form verisi okundu. TCKN: '{Tckn}', GSM: '{Gsm}'", tckn, gsm);
                }
                
                // Eğer formdan okunmadıysa JSON'dan dene
                if (string.IsNullOrEmpty(tckn))
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    _logger.LogInformation("OnPostVerifyTcknGsm: JSON Body okundu: {Body}", body);
                    
                    if (!string.IsNullOrEmpty(body))
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        if (jsonDoc.RootElement.TryGetProperty("Tckn", out var tElement)) tckn = tElement.GetString() ?? "";
                        if (jsonDoc.RootElement.TryGetProperty("Gsm", out var gElement)) gsm = gElement.GetString() ?? "";
                        
                        // Küçük harf denemesi
                        if (string.IsNullOrEmpty(tckn) && jsonDoc.RootElement.TryGetProperty("tckn", out var tLower)) tckn = tLower.GetString() ?? "";
                        if (string.IsNullOrEmpty(gsm) && jsonDoc.RootElement.TryGetProperty("gsm", out var gLower)) gsm = gLower.GetString() ?? "";
                    }
                }

                // TCKN/GSM'yi temizle ve logla
                tckn = (tckn ?? "").Trim();
                gsm = (gsm ?? "").Trim();
                
                _logger.LogInformation("OnPostVerifyTcknGsm: İşlenen değerler - TCKN: '{Tckn}' (Uzunluk: {TcknLen}), GSM: '{Gsm}' (Uzunluk: {GsmLen})", 
                    tckn, tckn.Length, gsm, gsm.Length);

                if (string.IsNullOrWhiteSpace(tckn) || string.IsNullOrWhiteSpace(gsm))
                {
                    _logger.LogWarning("OnPostVerifyTcknGsm: TCKN veya GSM eksik!");
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

                // API'nin beklediği format (camelCase)
                var requestBody = new
                {
                    tckn = tckn,
                    gsm = cleanedGsm
                };

                _logger.LogInformation("OnPostVerifyTcknGsm: Azure API'ye istek gönderiliyor. URL: {Url}, Body: {Body}", 
                    ApiConfig.TCKN_GSM.Split('?')[0], JsonSerializer.Serialize(requestBody));
                
                ApiResponse<object> response;
                try
                {
                    response = await _apiClient.PostAsync<object>(ApiConfig.TCKN_GSM, requestBody, HttpContext);
                    _logger.LogInformation("OnPostVerifyTcknGsm: API'den yanıt alındı. Status: {Status}, Success: {IsSuccess}, Error: {Error}", 
                        response.Status, response.IsSuccess, response.Error);
                }
                catch (Exception apiEx)
                {
                    _logger.LogError(apiEx, "OnPostVerifyTcknGsm: API çağrısı sırasında hata!");
                    return new JsonResult(new { success = false, error = $"API bağlantı hatası: {apiEx.Message}" })
                    {
                        StatusCode = 500
                    };
                }

                if (response.IsSuccess && response.Data != null)
                {
                    var json = JsonSerializer.Serialize(response.Data);
                    _logger.LogInformation("OnPostVerifyTcknGsm: API Yanıtı: {Response}", json);
                    var jsonDoc = JsonDocument.Parse(json);
                    
                    string? customerIdStr = null;
                    
                    // JSON hiyerarşisini en derinden en yüzeye tara: data.value.CustomerId veya value.CustomerId
                    JsonElement root = jsonDoc.RootElement;
                    
                    // data.value.CustomerId kontrolü
                    if (root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
                    {
                        if (dataElem.TryGetProperty("value", out var valElem) && valElem.ValueKind == JsonValueKind.Object)
                        {
                            if (valElem.TryGetProperty("CustomerId", out var cId)) 
                                customerIdStr = cId.ValueKind == JsonValueKind.Number ? cId.GetInt32().ToString() : cId.GetString();
                            else if (valElem.TryGetProperty("customerId", out var cIdLow))
                                customerIdStr = cIdLow.ValueKind == JsonValueKind.Number ? cIdLow.GetInt32().ToString() : cIdLow.GetString();
                        }
                    }
                    
                    // value.CustomerId kontrolü (yedek)
                    if (string.IsNullOrEmpty(customerIdStr) && root.TryGetProperty("value", out var vElem) && vElem.ValueKind == JsonValueKind.Object)
                    {
                        if (vElem.TryGetProperty("CustomerId", out var cId2)) 
                            customerIdStr = cId2.ValueKind == JsonValueKind.Number ? cId2.GetInt32().ToString() : cId2.GetString();
                    }

                    if (!string.IsNullOrEmpty(customerIdStr))
                    {
                        HttpContext.Session.SetString("CustomerId", customerIdStr);
                        _logger.LogInformation("OnPostVerifyTcknGsm: CustomerId kaydedildi: {CustomerId}", customerIdStr);
                    }
                    else
                    {
                        // Fallback: Tüm JSON içinde string taraması
                        _logger.LogWarning("OnPostVerifyTcknGsm: Standart yollarla CustomerId bulunamadı, fallback aranıyor...");
                        if (json.Contains("\"CustomerId\":"))
                        {
                            var parts = json.Split("\"CustomerId\":");
                            var valPart = parts[1].Split(',')[0].Split('}')[0].Trim().Trim(':').Trim('"').Trim();
                            customerIdStr = valPart;
                            HttpContext.Session.SetString("CustomerId", customerIdStr);
                            _logger.LogInformation("OnPostVerifyTcknGsm: Fallback ile CustomerId bulundu: {CustomerId}", customerIdStr);
                        }
                    }

                    HttpContext.Session.SetString("Gsm", cleanedGsm);
                    HttpContext.Session.SetString("Tckn", tckn);
                    
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
                string? gsm = null;
                if (Request.HasFormContentType)
                {
                    gsm = Request.Form["gsm"].ToString()?.Trim();
                }

                if (string.IsNullOrEmpty(gsm))
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        try {
                            var jsonDoc = JsonDocument.Parse(body);
                            if (jsonDoc.RootElement.TryGetProperty("gsm", out var gElement)) gsm = gElement.GetString()?.Trim();
                        } catch { }
                    }
                }
                
                // Session'dan al
                if (string.IsNullOrEmpty(gsm))
                {
                    gsm = HttpContext.Session.GetString("Gsm");
                }

                var tckn = HttpContext.Session.GetString("Tckn");
                _logger.LogInformation("OnPostGenerateOtp: GSM={Gsm}, TCKN={Tckn}", gsm, tckn);

                if (string.IsNullOrEmpty(gsm))
                {
                    return new JsonResult(new { success = false, error = "GSM numarası bulunamadı." }) { StatusCode = 400 };
                }

                if (string.IsNullOrEmpty(tckn))
                {
                    return new JsonResult(new { success = false, error = "TCKN bulunamadı." }) { StatusCode = 400 };
                }

                // GSM numarasını temizle
                var cleanedGsm = new string(gsm.Where(char.IsDigit).ToArray());
                if (cleanedGsm.Length == 11 && cleanedGsm.StartsWith("0"))
                {
                    cleanedGsm = cleanedGsm.Substring(1);
                }

                // API'nin beklediği format (camelCase - VerifyTcknGsm ile aynı)
                var requestBody = new
                {
                    tckn = tckn,
                    gsm = cleanedGsm
                };

                _logger.LogInformation("OnPostGenerateOtp: Azure API'ye istek gönderiliyor. URL: {Url}, Body: {Body}", 
                    ApiConfig.GENERATE_OTP.Split('?')[0], JsonSerializer.Serialize(requestBody));
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.GENERATE_OTP, requestBody, HttpContext);
                
                _logger.LogInformation("OnPostGenerateOtp: API Yanıtı - Status: {Status}, Success: {IsSuccess}, Error: {Error}", 
                    response.Status, response.IsSuccess, response.Error);

                if (response.IsSuccess && response.Data != null)
                {
                    var responseContent = JsonSerializer.Serialize(response.Data);
                    _logger.LogInformation("OnPostGenerateOtp: Başarılı yanıt: {Response}", responseContent);
                    try {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        string? otpCode = null;
                        
                        // data.value.OTPCode formatını tara
                        if (jsonDoc.RootElement.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
                        {
                            if (dataElem.TryGetProperty("value", out var valElem) && valElem.ValueKind == JsonValueKind.Object)
                            {
                                if (valElem.TryGetProperty("OTPCode", out var otpVal))
                                    otpCode = otpVal.ValueKind == JsonValueKind.Number ? otpVal.GetInt32().ToString() : otpVal.GetString();
                                else if (valElem.TryGetProperty("otpCode", out var otpValLow))
                                    otpCode = otpValLow.ValueKind == JsonValueKind.Number ? otpValLow.GetInt32().ToString() : otpValLow.GetString();
                            }
                        }
                        
                        // Alternatif: doğrudan value.OTPCode
                        if (string.IsNullOrEmpty(otpCode) && jsonDoc.RootElement.TryGetProperty("value", out var valElemDirect) && valElemDirect.ValueKind == JsonValueKind.Object)
                        {
                            if (valElemDirect.TryGetProperty("OTPCode", out var otpVal))
                                otpCode = otpVal.ValueKind == JsonValueKind.Number ? otpVal.GetInt32().ToString() : otpVal.GetString();
                        }

                        if (!string.IsNullOrEmpty(otpCode))
                        {
                            HttpContext.Session.SetString("OtpCode", otpCode);
                            _logger.LogInformation("OnPostGenerateOtp: OTP kodu session'a kaydedildi: {OtpCode}", otpCode);
                        }
                        else
                        {
                            _logger.LogWarning("OnPostGenerateOtp: OTP kodu yanıtta bulunamadı!");
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, "OnPostGenerateOtp: Yanıt parse edilirken hata!");
                    }
                    
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "OTP üretilemedi" }) { StatusCode = response.Status };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostGenerateOtp: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = ex.Message }) { StatusCode = 500 };
            }
        }

        // OTP SMS Gönder
        public async Task<IActionResult> OnPostSendOtpSms()
        {
            try
            {
                string? gsm = null;
                if (Request.HasFormContentType)
                {
                    gsm = Request.Form["gsm"].ToString()?.Trim();
                }
                
                if (string.IsNullOrEmpty(gsm)) gsm = HttpContext.Session.GetString("Gsm");
                var otpCode = HttpContext.Session.GetString("OtpCode");

                _logger.LogInformation("OnPostSendOtpSms: GSM={Gsm}, OtpCode={OtpCode}", gsm, otpCode);

                if (string.IsNullOrEmpty(gsm) || string.IsNullOrEmpty(otpCode))
                {
                    return new JsonResult(new { success = false, error = "GSM veya OTP bulunamadı." }) { StatusCode = 400 };
                }

                var cleanedGsm = new string(gsm.Where(char.IsDigit).ToArray());
                if (cleanedGsm.Length == 11 && cleanedGsm.StartsWith("0")) cleanedGsm = cleanedGsm.Substring(1);

                // API'nin beklediği format (camelCase - VerifyTcknGsm ile aynı)
                var requestBody = new
                {
                    gsm = cleanedGsm,
                    otpCode = otpCode
                };

                _logger.LogInformation("OnPostSendOtpSms: Azure API'ye istek gönderiliyor. URL: {Url}, Body: {Body}", 
                    ApiConfig.SEND_OTP_SMS.Split('?')[0], JsonSerializer.Serialize(requestBody));
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.SEND_OTP_SMS, requestBody, HttpContext);
                
                _logger.LogInformation("OnPostSendOtpSms: API Yanıtı - Status: {Status}, Success: {IsSuccess}, Error: {Error}", 
                    response.Status, response.IsSuccess, response.Error);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "SMS gönderilemedi" }) { StatusCode = response.Status };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostSendOtpSms: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = ex.Message }) { StatusCode = 500 };
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
                _logger.LogInformation("OnPostKvkkOnay başladı. SessionID: {SessionId}", HttpContext.Session.Id);
                int kvkkId = 1; // Default
                if (Request.HasFormContentType && Request.Form.ContainsKey("kvkkId"))
                {
                    int.TryParse(Request.Form["kvkkId"], out kvkkId);
                }
                else
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        try {
                            var jsonDoc = JsonDocument.Parse(body);
                            if (jsonDoc.RootElement.TryGetProperty("kvkkId", out var kvkkElement)) kvkkId = kvkkElement.GetInt32();
                        } catch { }
                    }
                }

                var customerId = HttpContext.Session.GetString("CustomerId") ?? "";
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("OnPostKvkkOnay: CustomerId session'da bulunamadı!");
                    return new JsonResult(new { success = false, error = "Müşteri bilgisi (ID) bulunamadı. Lütfen sayfayı yenileyip tekrar deneyin." }) { StatusCode = 400 };
                }

                var requestBody = new
                {
                    customerId = int.Parse(customerId),
                    kvkkId = kvkkId,
                    approved = true,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                _logger.LogInformation("OnPostKvkkOnay: İstek gönderiliyor (camelCase). URL: {Url}, Body: {Body}", 
                    ApiConfig.KVKK_ONAY.Split('?')[0], JsonSerializer.Serialize(requestBody));
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.KVKK_ONAY, requestBody, HttpContext);
                
                _logger.LogInformation("OnPostKvkkOnay: API Yanıtı - Status: {Status}, Success: {IsSuccess}, Error: {Error}", 
                    response.Status, response.IsSuccess, response.Error);

                if (response.IsSuccess) return new JsonResult(new { success = true, data = response.Data });
                return new JsonResult(new { success = false, error = response.Error }) { StatusCode = response.Status };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message }) { StatusCode = 500 };
            }
        }

        // OTP Doğrulama
        public async Task<IActionResult> OnPostVerifyOtp()
        {
            try
            {
                var otpCode = Request.Form["OtpCode"].ToString()?.Trim() ?? "";
                var gsm = Request.Form["Gsm"].ToString()?.Trim() ?? HttpContext.Session.GetString("Gsm") ?? "";

                _logger.LogInformation("OnPostVerifyOtp: OTP doğrulama isteği. GSM: {Gsm}, OtpCode: {OtpCode}", gsm, otpCode);

                if (string.IsNullOrEmpty(gsm))
                {
                    _logger.LogWarning("OnPostVerifyOtp: GSM numarası bulunamadı");
                    return new JsonResult(new { success = false, error = "GSM numarası bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }

                // API'nin beklediği format (camelCase - VerifyTcknGsm ile aynı)
                var requestBody = new
                {
                    gsm = gsm,
                    otpCode = otpCode
                };

                _logger.LogInformation("OnPostVerifyOtp: Azure API'ye istek gönderiliyor. URL: {Url}, Body: {Body}", 
                    ApiConfig.VERIFY_OTP.Split('?')[0], JsonSerializer.Serialize(requestBody));
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.VERIFY_OTP, requestBody, HttpContext);
                
                _logger.LogInformation("OnPostVerifyOtp: API Yanıtı - Status: {Status}, Success: {IsSuccess}, Error: {Error}", 
                    response.Status, response.IsSuccess, response.Error);

                if (response.IsSuccess && response.Data != null)
                {
                    // Token'ı session'a kaydet
                    var json = JsonSerializer.Serialize(response.Data);
                    var jsonDoc = JsonDocument.Parse(json);
                    
                    string? token = null;
                    string? firstName = null;
                    string? lastName = null;
                    string? email = null;
                    string? phone = null;

                    // Token ve kullanıcı bilgilerini parse et
                    ParseUserInfoFromResponse(jsonDoc.RootElement, ref token, ref firstName, ref lastName, ref email, ref phone);

                    if (!string.IsNullOrEmpty(token))
                    {
                        _apiClient.SetToken(HttpContext, token);
                    }

                    // Kullanıcı adını oluştur
                    var fullName = !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName)
                        ? $"{firstName} {lastName}".Trim()
                        : "Ahmet Yılmaz"; // Varsayılan

                    // Set session as logged in
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetString("UserName", fullName);
                    HttpContext.Session.SetString("UserFirstName", firstName ?? "Ahmet");
                    HttpContext.Session.SetString("UserLastName", lastName ?? "Yılmaz");
                    HttpContext.Session.SetString("Identifier", Identifier ?? "");
                    if (!string.IsNullOrEmpty(email))
                        HttpContext.Session.SetString("UserEmail", email);
                    if (!string.IsNullOrEmpty(phone))
                        HttpContext.Session.SetString("UserPhone", phone);

                    // Create claims for cookie authentication
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, fullName),
                        new Claim("FullName", fullName), // Custom claim for header
                        new Claim(ClaimTypes.GivenName, firstName ?? "Ahmet"),
                        new Claim(ClaimTypes.Surname, lastName ?? "Yılmaz"),
                        new Claim(ClaimTypes.NameIdentifier, Identifier ?? ""),
                        new Claim("IsLoggedIn", "true")
                    };

                    if (!string.IsNullOrEmpty(email))
                        claims.Add(new Claim(ClaimTypes.Email, email));
                    if (!string.IsNullOrEmpty(phone))
                        claims.Add(new Claim(ClaimTypes.MobilePhone, phone));

                    var customerId = HttpContext.Session.GetString("CustomerId");
                    if (!string.IsNullOrEmpty(customerId))
                        claims.Add(new Claim("CustomerId", customerId));

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation("OnPostVerifyOtp: Kullanıcı giriş yaptı. FullName: {FullName}, CustomerId: {CustomerId}", 
                        fullName, customerId);

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
            // Session'dan kullanıcı bilgilerini al (varsa)
            var firstName = HttpContext.Session.GetString("UserFirstName") ?? "Ahmet";
            var lastName = HttpContext.Session.GetString("UserLastName") ?? "Yılmaz";
            var fullName = $"{firstName} {lastName}".Trim();
            var customerId = HttpContext.Session.GetString("CustomerId");
            var email = HttpContext.Session.GetString("UserEmail");
            var phone = HttpContext.Session.GetString("Gsm");

            // Set session as logged in
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", fullName);
            HttpContext.Session.SetString("Identifier", Identifier ?? "");

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullName),
                new Claim("FullName", fullName), // Custom claim for header
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim(ClaimTypes.NameIdentifier, Identifier ?? ""),
                new Claim("IsLoggedIn", "true")
            };

            if (!string.IsNullOrEmpty(email))
                claims.Add(new Claim(ClaimTypes.Email, email));
            if (!string.IsNullOrEmpty(phone))
                claims.Add(new Claim(ClaimTypes.MobilePhone, phone));
            if (!string.IsNullOrEmpty(customerId))
                claims.Add(new Claim("CustomerId", customerId));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            _logger.LogInformation("OnPostLogin: Kullanıcı giriş yaptı. FullName: {FullName}", fullName);

            return RedirectToPage("/Dashboard");
        }

        /// <summary>
        /// API yanıtından kullanıcı bilgilerini parse eder
        /// </summary>
        private void ParseUserInfoFromResponse(JsonElement root, ref string? token, ref string? firstName, ref string? lastName, ref string? email, ref string? phone)
        {
            // Token ara
            if (root.TryGetProperty("token", out var tokenElement))
                token = tokenElement.GetString();
            else if (root.TryGetProperty("access_token", out var accessToken))
                token = accessToken.GetString();
            else if (root.TryGetProperty("jwt", out var jwt))
                token = jwt.GetString();

            // data.value veya value içinden kullanıcı bilgilerini ara
            JsonElement? userElement = null;

            if (root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
            {
                if (dataElem.TryGetProperty("value", out var valElem) && valElem.ValueKind == JsonValueKind.Object)
                    userElement = valElem;
                else if (dataElem.TryGetProperty("user", out var userElem) && userElem.ValueKind == JsonValueKind.Object)
                    userElement = userElem;
            }
            else if (root.TryGetProperty("value", out var valElemDirect) && valElemDirect.ValueKind == JsonValueKind.Object)
            {
                userElement = valElemDirect;
            }
            else if (root.TryGetProperty("user", out var userElemDirect) && userElemDirect.ValueKind == JsonValueKind.Object)
            {
                userElement = userElemDirect;
            }

            if (userElement.HasValue)
            {
                var user = userElement.Value;

                // Ad
                if (user.TryGetProperty("FirstName", out var fn))
                    firstName = fn.GetString();
                else if (user.TryGetProperty("firstName", out var fnLower))
                    firstName = fnLower.GetString();
                else if (user.TryGetProperty("name", out var name))
                    firstName = name.GetString();

                // Soyad
                if (user.TryGetProperty("LastName", out var ln))
                    lastName = ln.GetString();
                else if (user.TryGetProperty("lastName", out var lnLower))
                    lastName = lnLower.GetString();
                else if (user.TryGetProperty("surname", out var surname))
                    lastName = surname.GetString();

                // E-posta
                if (user.TryGetProperty("Email", out var em))
                    email = em.GetString();
                else if (user.TryGetProperty("email", out var emLower))
                    email = emLower.GetString();

                // Telefon
                if (user.TryGetProperty("Phone", out var ph))
                    phone = ph.GetString();
                else if (user.TryGetProperty("phone", out var phLower))
                    phone = phLower.GetString();
                else if (user.TryGetProperty("Gsm", out var gsm))
                    phone = gsm.GetString();
                else if (user.TryGetProperty("gsm", out var gsmLower))
                    phone = gsmLower.GetString();
            }

            // Root seviyesinde de kontrol et
            if (string.IsNullOrEmpty(firstName) && root.TryGetProperty("firstName", out var rootFn))
                firstName = rootFn.GetString();
            if (string.IsNullOrEmpty(lastName) && root.TryGetProperty("lastName", out var rootLn))
                lastName = rootLn.GetString();
        }
    }
}
