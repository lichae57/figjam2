using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class SettingsModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostUpdateSessionTimeout([FromForm] int timeoutMinutes)
        {
            HttpContext.Session.SetString("SessionTimeoutMinutes", timeoutMinutes.ToString());
            return new JsonResult(new { success = true, message = "Oturum zaman aşımı ayarı güncellendi." });
        }
    }
}

