using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;

namespace figjam2.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public ProfileModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void OnGet()
        {
        }

        // Müşteri Adres Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetCustomerAddress()
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

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerAddressUrl(customerId), HttpContext);

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

        // Müşteri Adres Kaydet
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostCustomerAddress()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_ADDRESS_CREATE, System.Text.Json.JsonSerializer.Deserialize<object>(body), HttpContext);

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

        // Müşteri İş Bilgisi Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetCustomerJobInfo()
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

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerJobInfoUrl(customerId), HttpContext);

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

        // Müşteri İş Profili Kaydet
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostCustomerJobProfile()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_JOB_PROFILE, System.Text.Json.JsonSerializer.Deserialize<object>(body), HttpContext);

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

