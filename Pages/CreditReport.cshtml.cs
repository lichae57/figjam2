using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;

namespace figjam2.Pages
{
    public class CreditReportModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public CreditReportModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void OnGet()
        {
        }

        // Rapor Listesi Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetReportList()
        {
            try
            {
                var response = await _apiClient.GetAsync<object>(ApiConfig.DUMMY_REPORT_LIST, HttpContext);

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

        // Rapor Detayı Getir
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetReportDetail(string reportId)
        {
            try
            {
                if (string.IsNullOrEmpty(reportId))
                {
                    return new JsonResult(new { success = false, error = "reportId gerekli" })
                    {
                        StatusCode = 400
                    };
                }

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetReportDetailUrl(reportId), HttpContext);

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


