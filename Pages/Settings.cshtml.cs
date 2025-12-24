using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;
using figjam2.Models;
using System.Text.Json;
using System.Linq;

namespace figjam2.Pages
{
    [IgnoreAntiforgeryToken]
    public class SettingsModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<SettingsModel> _logger;

        public SettingsModel(ApiClient apiClient, ILogger<SettingsModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [BindProperty]
        public UserProfileViewModel Profile { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                _logger.LogInformation("=== AYARLAR SAYFASI YÜKLENİYOR - CustomerId: {CustomerId} ===", customerId ?? "YOK");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    LoadDefaultProfile();
                    return Page();
                }

                // Tüm API'leri paralel çağır (Profil sayfasıyla aynı)
                var addressTask = GetCustomerAddressAsync(customerId);
                var jobTask = GetCustomerJobInfoAsync(customerId);
                var financeTask = GetCustomerFinanceAssetsAsync(customerId);
                var wifeTask = GetCustomerWifeInfoAsync(customerId);

                await Task.WhenAll(addressTask, jobTask, financeTask, wifeTask);

                // Modeli doldur
                PopulateProfileFromApiResponses(await addressTask, await jobTask, await financeTask, await wifeTask);

                // Sabit Bilgiler (Session'dan)
                Profile.TcKimlikNo = HttpContext.Session.GetString("Tckn") ?? "";
                Profile.Phone = HttpContext.Session.GetString("Gsm") ?? "";
                Profile.CustomerId = int.TryParse(customerId, out var cid) ? cid : null;

                // Header Senkronizasyonu
                if (!string.IsNullOrEmpty(Profile.FullName))
                {
                    HttpContext.Session.SetString("UserName", Profile.FullName);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Settings OnGetAsync Hatası");
                LoadDefaultProfile();
                return Page();
            }
        }

        private void PopulateProfileFromApiResponses(CustomerAddressResponse? address, CustomerJobInfoResponse? jobInfo, CustomerFinanceAssetsResponse? finance, CustomerWifeInfoResponse? wifeInfo)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "";
            var nameParts = userName.Split(' ', 2);
            Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
            Profile.LastName = nameParts.Length > 1 ? nameParts[1] : "";

            if (address != null)
            {
                Profile.Address = address.Address ?? address.FullAddress ?? "";
                Profile.City = address.CityName ?? "";
                Profile.District = address.DistrictName ?? "";
            }

            if (jobInfo != null)
            {
                Profile.Profession = jobInfo.OccupationName ?? jobInfo.JobGroupName ?? jobInfo.JobTitle ?? "";
                Profile.IncomeRange = jobInfo.IncomeRange ?? "";
            }

            if (finance != null)
            {
                Profile.FinancialStatus = finance.FinancialStatus ?? finance.value ?? "";
                Profile.AssetInfo = finance.Assets ?? "";
            }

            if (wifeInfo != null)
            {
                Profile.SpouseWorkStatus = wifeInfo.WifeWorkStatus ?? "";
                Profile.SpouseIncomeRange = wifeInfo.WifeIncomeRange ?? "";
            }

            Profile.Email = HttpContext.Session.GetString("UserEmail");
            
            var sessionBirthDate = HttpContext.Session.GetString("UserBirthDate");
            if (!string.IsNullOrEmpty(sessionBirthDate) && DateTime.TryParse(sessionBirthDate, out var bDate))
            {
                Profile.BirthDate = bDate;
            }
        }

        private void LoadDefaultProfile()
        {
            Profile = new UserProfileViewModel();
            var userName = HttpContext.Session.GetString("UserName") ?? "";
            var nameParts = userName.Split(' ', 2);
            Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
            Profile.LastName = nameParts.Length > 1 ? nameParts[1] : "";
            Profile.TcKimlikNo = HttpContext.Session.GetString("Tckn") ?? "";
            Profile.Phone = HttpContext.Session.GetString("Gsm") ?? "";
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            _logger.LogInformation("GÜNCELLEME BAŞLADI (Settings)");
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("Gelen Güncelleme Paketi: {Body}", body);

                var updateData = JsonSerializer.Deserialize<JsonElement>(body);
                var customerId = HttpContext.Session.GetString("CustomerId");
                
                if (string.IsNullOrEmpty(customerId)) return new JsonResult(new { success = false, error = "Oturum geçersiz" });

                var results = new List<string>();

                // 1. Adres
                if (updateData.TryGetProperty("address", out var addr) && !string.IsNullOrEmpty(addr.GetString()))
                {
                    var req = new { 
                        tckn = HttpContext.Session.GetString("Tckn"), 
                        customerId = int.Parse(customerId), 
                        address = addr.GetString(),
                        cityId = 34, townId = 122, source = 2
                    };
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_ADDRESS_CREATE, req, HttpContext);
                    if (resp.IsSuccess) results.Add("Adres güncellendi");
                }

                // 2. İş
                if (updateData.TryGetProperty("profession", out var prof))
                {
                    var req = new { 
                        customerId = int.Parse(customerId), 
                        occupationName = prof.GetString(),
                        incomeRange = updateData.TryGetProperty("incomeRange", out var inc) ? inc.GetString() : ""
                    };
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_JOB_PROFILE, req, HttpContext);
                    if (resp.IsSuccess) results.Add("İş bilgileri güncellendi");
                }

                // 3. Finansal
                string financialStatus = "";
                string assetInfo = "";
                if (updateData.TryGetProperty("financialStatus", out var fin)) financialStatus = fin.GetString() ?? "";
                if (updateData.TryGetProperty("assetInfo", out var ass)) assetInfo = ass.GetString() ?? "";

                if (!string.IsNullOrEmpty(financialStatus) || !string.IsNullOrEmpty(assetInfo))
                {
                    var req = new { 
                        customerId = int.Parse(customerId), 
                        financialStatus = financialStatus,
                        assets = assetInfo
                    };
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_FINANCE_ASSETS_POST, req, HttpContext);
                    if (resp.IsSuccess) results.Add("Finansal bilgiler güncellendi");
                }

                return new JsonResult(new { success = true, message = string.Join(", ", results) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Settings Güncelleme Hatası");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public IActionResult OnPostUpdateSessionTimeout([FromForm] int timeoutMinutes)
        {
            HttpContext.Session.SetString("SessionTimeoutMinutes", timeoutMinutes.ToString());
            return new JsonResult(new { success = true, message = "Oturum zaman aşımı ayarı güncellendi." });
        }

        private async Task<CustomerAddressResponse?> GetCustomerAddressAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerAddressUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) {
                    var root = JsonDocument.Parse(JsonSerializer.Serialize(response.Data)).RootElement;
                    JsonElement? addr = null;
                    if (root.TryGetProperty("data", out var d) && d.TryGetProperty("value", out var v)) addr = v.ValueKind == JsonValueKind.Array && v.GetArrayLength() > 0 ? v[0] : v;
                    else if (root.TryGetProperty("value", out var v2)) addr = v2.ValueKind == JsonValueKind.Array && v2.GetArrayLength() > 0 ? v2[0] : v2;
                    if (addr.HasValue) return JsonSerializer.Deserialize<CustomerAddressResponse>(addr.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            } catch {}
            return null;
        }

        private async Task<CustomerJobInfoResponse?> GetCustomerJobInfoAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerJobInfoUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) {
                    var root = JsonDocument.Parse(JsonSerializer.Serialize(response.Data)).RootElement;
                    JsonElement? job = null;
                    if (root.TryGetProperty("data", out var d) && d.TryGetProperty("value", out var v)) job = v;
                    else if (root.TryGetProperty("value", out var v2)) job = v2;
                    if (job.HasValue) return JsonSerializer.Deserialize<CustomerJobInfoResponse>(job.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            } catch {}
            return null;
        }

        private async Task<CustomerFinanceAssetsResponse?> GetCustomerFinanceAssetsAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerFinanceAssetsUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) {
                    var root = JsonDocument.Parse(JsonSerializer.Serialize(response.Data)).RootElement;
                    var data = root.TryGetProperty("data", out var d) ? d : (root.TryGetProperty("value", out var v) ? v : root);
                    return JsonSerializer.Deserialize<CustomerFinanceAssetsResponse>(data.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            } catch {}
            return null;
        }

        private async Task<CustomerWifeInfoResponse?> GetCustomerWifeInfoAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerWifeInfoUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) {
                    var root = JsonDocument.Parse(JsonSerializer.Serialize(response.Data)).RootElement;
                    var data = root.TryGetProperty("data", out var d) ? d : (root.TryGetProperty("value", out var v) ? v : root);
                    if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0) return JsonSerializer.Deserialize<CustomerWifeInfoResponse>(data[0].GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return JsonSerializer.Deserialize<CustomerWifeInfoResponse>(data.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            } catch {}
            return null;
        }
    }
}
