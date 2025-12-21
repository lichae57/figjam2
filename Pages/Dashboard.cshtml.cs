using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;

namespace figjam2.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public DashboardModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void OnGet()
        {
            // Redirect to Index if not logged in
            if (User.Identity?.IsAuthenticated != true && 
                HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                Response.Redirect("/Index");
            }
        }

        // Müşteri Finansal Varlıkları Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetFinanceAssets()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(customerId))
                {
                    return new JsonResult(new { success = false, error = "Müşteri ID bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerFinanceAssetsUrl(customerId), HttpContext);

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
    }
}

