using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;
using figjam2.Models;
using System.Text.Json;

namespace figjam2.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ApiClient apiClient, ILogger<DashboardModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public UserProfileViewModel Profile { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Login kontrolü
            if (User.Identity?.IsAuthenticated != true && HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                _logger.LogInformation("Dashboard Yükleniyor: {CustomerId}", customerId ?? "YOK");

                if (!string.IsNullOrEmpty(customerId))
                {
                    // Paralel veri çekme
                    var addressTask = GetCustomerAddressAsync(customerId);
                    var financeTask = GetCustomerFinanceAssetsAsync(customerId);
                    var jobTask = GetCustomerJobInfoAsync(customerId);

                    await Task.WhenAll(addressTask, financeTask, jobTask);

                    var address = await addressTask;
                    var finance = await financeTask;
                    var job = await jobTask;

                    // Modeli doldur
                    var userName = HttpContext.Session.GetString("UserName") ?? "";
                    var nameParts = userName.Split(' ', 2);
                    Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                    Profile.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                    Profile.TcKimlikNo = HttpContext.Session.GetString("Tckn") ?? "";
                    Profile.Phone = HttpContext.Session.GetString("Gsm") ?? "";
                    Profile.CustomerId = int.TryParse(customerId, out var cid) ? cid : null;

                    if (finance != null)
                    {
                        Profile.FinancialStatus = finance.FinancialStatus ?? finance.value ?? "";
                        Profile.AssetInfo = finance.Assets ?? "";
                    }
                }
                else
                {
                    // Session'dan temel bilgileri al
                    var userName = HttpContext.Session.GetString("UserName") ?? "";
                    var nameParts = userName.Split(' ', 2);
                    Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : "Kullanıcı";
                    Profile.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard OnGetAsync Hatası");
                return Page();
            }
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
    }
}
