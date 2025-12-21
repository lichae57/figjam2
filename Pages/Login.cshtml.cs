using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using figjam2.Services;
using System.Text.Json;

namespace figjam2.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public LoginModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
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
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostVerifyTcknGsm()
        {
            try
            {
                var tckn = Request.Form["Tckn"].ToString()?.Trim() ?? "";
                var gsm = Request.Form["Gsm"].ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(tckn) || string.IsNullOrWhiteSpace(gsm))
                {
                    return new JsonResult(new { success = false, error = "TCKN ve telefon numarası gereklidir" })
                    {
                        StatusCode = 400
                    };
                }

                var requestBody = new
                {
                    tckn = tckn,
                    gsm = gsm
                };

                var response = await _apiClient.PostAsync<object>(ApiConfig.TCKN_GSM, requestBody, HttpContext);

                if (response.IsSuccess && response.Data != null)
                {
                    // Customer ID'yi session'a kaydet
                    var json = JsonSerializer.Serialize(response.Data);
                    var jsonDoc = JsonDocument.Parse(json);
                    if (jsonDoc.RootElement.TryGetProperty("customerId", out var customerId))
                    {
                        HttpContext.Session.SetString("CustomerId", customerId.GetString() ?? "");
                    }

                    // GSM numarasını session'a kaydet (OTP için gerekli)
                    HttpContext.Session.SetString("Gsm", gsm);

                    return new JsonResult(new { success = true, data = response.Data, gsm = gsm });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "Doğrulama başarısız" })
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

        // OTP Üret
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostGenerateOtp()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var jsonDoc = JsonDocument.Parse(body);
                
                var gsm = jsonDoc.RootElement.TryGetProperty("gsm", out var gsmElement) 
                    ? gsmElement.GetString() 
                    : HttpContext.Session.GetString("Gsm");

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
                    purpose = "verification"
                };

                var response = await _apiClient.PostAsync<object>(ApiConfig.GENERATE_OTP, requestBody, HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "OTP üretilemedi" })
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

        // OTP SMS Gönder
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostSendOtpSms()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var jsonDoc = JsonDocument.Parse(body);
                
                var gsm = jsonDoc.RootElement.TryGetProperty("gsm", out var gsmElement) 
                    ? gsmElement.GetString() 
                    : HttpContext.Session.GetString("Gsm");

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
                    messageType = "otp"
                };

                var response = await _apiClient.PostAsync<object>(ApiConfig.SEND_OTP_SMS, requestBody, HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error ?? "SMS gönderilemedi" })
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

        // KVKK Metni Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetKvkkText(int id = 2)
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
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostKvkkOnay()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId") ?? "";
                var requestBody = new
                {
                    customerId = customerId,
                    kvkkId = 2,
                    accepted = true
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
        [IgnoreAntiforgeryToken]
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
