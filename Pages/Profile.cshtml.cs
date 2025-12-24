using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;
using figjam2.Models;
using System.Text.Json;
using System.Linq;

namespace figjam2.Pages
{
    [IgnoreAntiforgeryToken]
    public class ProfileModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(ApiClient apiClient, ILogger<ProfileModel> logger)
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
                _logger.LogInformation("=== PROFIL SAYFASI YÜKLENİYOR - CustomerId: {CustomerId} ===", customerId ?? "YOK");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    LoadDefaultProfile();
                    return Page();
                }

                // Tüm API'leri paralel çağır
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
                _logger.LogError(ex, "OnGetAsync Hatası");
                LoadDefaultProfile();
                return Page();
            }
        }

        private void PopulateProfileFromApiResponses(CustomerAddressResponse? address, CustomerJobInfoResponse? jobInfo, CustomerFinanceAssetsResponse? finance, CustomerWifeInfoResponse? wifeInfo)
        {
            // İsim bilgileri
            var userName = HttpContext.Session.GetString("UserName") ?? "";
            var nameParts = userName.Split(' ', 2);
            Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
            Profile.LastName = nameParts.Length > 1 ? nameParts[1] : "";

            // 1. Adres
            if (address != null)
            {
                Profile.Address = address.Address ?? address.FullAddress ?? "";
                Profile.City = address.CityName ?? "";
                Profile.District = address.DistrictName ?? "";
                Profile.PostalCode = address.PostalCode ?? "";
            }

            // 2. İş
            if (jobInfo != null)
            {
                Profile.Profession = jobInfo.OccupationName ?? jobInfo.JobGroupName ?? jobInfo.JobTitle ?? "";
                Profile.IncomeRange = jobInfo.IncomeRange ?? "";
            }

            // 3. Finansal
            if (finance != null)
            {
                Profile.FinancialStatus = finance.FinancialStatus ?? finance.value ?? "";
                Profile.AssetInfo = finance.Assets ?? "";
            }

            // 4. Eş
            if (wifeInfo != null)
            {
                Profile.SpouseWorkStatus = wifeInfo.WifeWorkStatus ?? "";
                Profile.SpouseIncomeRange = wifeInfo.WifeIncomeRange ?? "";
            }

            Profile.Email = HttpContext.Session.GetString("UserEmail") ?? "";
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
            _logger.LogInformation("GÜNCELLEME BAŞLADI");
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
                if (updateData.TryGetProperty("address", out var addr))
                {
                    var req = new { 
                        tckn = HttpContext.Session.GetString("Tckn"), 
                        customerId = int.Parse(customerId), 
                        address = addr.GetString(),
                        cityId = 34, townId = 122, source = 2
                    };
                    _logger.LogInformation("Adres API İsteği: {Req}", JsonSerializer.Serialize(req));
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_ADDRESS_CREATE, req, HttpContext);
                    if (resp.IsSuccess) results.Add("Adres güncellendi");
                    else _logger.LogWarning("Adres API Hatası: {Error}", resp.Error);
                }

                // 2. İş
                if (updateData.TryGetProperty("profession", out var prof))
                {
                    var req = new { 
                        customerId = int.Parse(customerId), 
                        occupationName = prof.GetString(),
                        incomeRange = updateData.TryGetProperty("incomeRange", out var inc) ? inc.GetString() : ""
                    };
                    _logger.LogInformation("İş API İsteği: {Req}", JsonSerializer.Serialize(req));
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_JOB_PROFILE, req, HttpContext);
                    if (resp.IsSuccess) results.Add("İş bilgileri güncellendi");
                    else _logger.LogWarning("İş API Hatası: {Error}", resp.Error);
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
                    _logger.LogInformation("Finansal API İsteği: {Req}", JsonSerializer.Serialize(req));
                    var resp = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_FINANCE_ASSETS_POST, req, HttpContext);
                    if (resp.IsSuccess) results.Add("Finansal bilgiler güncellendi");
                    else _logger.LogWarning("Finansal API Hatası: {Error}", resp.Error);
                }

                return new JsonResult(new { success = true, message = string.Join(", ", results) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Güncelleme İşleminde Kritik Hata");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        private async Task<CustomerAddressResponse?> GetCustomerAddressAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerAddressUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) return ParseAddressResponse(JsonSerializer.Serialize(response.Data));
            } catch {}
            return null;
        }

        private async Task<CustomerJobInfoResponse?> GetCustomerJobInfoAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerJobInfoUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) return ParseJobInfoResponse(JsonSerializer.Serialize(response.Data));
            } catch {}
            return null;
        }

        private async Task<CustomerFinanceAssetsResponse?> GetCustomerFinanceAssetsAsync(string customerId)
        {
            try {
                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerFinanceAssetsUrl(customerId), HttpContext);
                if (response.IsSuccess && response.Data != null) {
                    var json = JsonSerializer.Serialize(response.Data);
                    var root = JsonDocument.Parse(json).RootElement;
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
                    var json = JsonSerializer.Serialize(response.Data);
                    var root = JsonDocument.Parse(json).RootElement;
                    var data = root.TryGetProperty("data", out var d) ? d : (root.TryGetProperty("value", out var v) ? v : root);
                    if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0) return JsonSerializer.Deserialize<CustomerWifeInfoResponse>(data[0].GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return JsonSerializer.Deserialize<CustomerWifeInfoResponse>(data.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            } catch {}
            return null;
        }

        private CustomerAddressResponse? ParseAddressResponse(string json)
        {
            try {
                var root = JsonDocument.Parse(json).RootElement;
                JsonElement? addr = null;
                if (root.TryGetProperty("data", out var d) && d.TryGetProperty("value", out var v)) addr = v.ValueKind == JsonValueKind.Array && v.GetArrayLength() > 0 ? v[0] : v;
                else if (root.TryGetProperty("value", out var v2)) addr = v2.ValueKind == JsonValueKind.Array && v2.GetArrayLength() > 0 ? v2[0] : v2;
                if (addr.HasValue) return JsonSerializer.Deserialize<CustomerAddressResponse>(addr.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } catch {}
            return null;
        }

        private CustomerJobInfoResponse? ParseJobInfoResponse(string json)
        {
            try {
                var root = JsonDocument.Parse(json).RootElement;
                JsonElement? job = null;
                if (root.TryGetProperty("data", out var d) && d.TryGetProperty("value", out var v)) job = v;
                else if (root.TryGetProperty("value", out var v2)) job = v2;
                if (job.HasValue) return JsonSerializer.Deserialize<CustomerJobInfoResponse>(job.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } catch {}
            return null;
        }

        private static readonly Dictionary<int, string> CityLookup = new() { {34, "İstanbul"}, {35, "İzmir"}, {6, "Ankara"} };
        private string GetCityName(int id) => CityLookup.TryGetValue(id, out var n) ? n : $"Şehir {id}";
        private string GetDistrictName(int id) => $"İlçe {id}";
        private int? GetCityId(string name) => CityLookup.FirstOrDefault(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;
        private int? GetDistrictId(string name) => 122; // Default Kadıköy
    }
}
