using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Session'ı temizle
            HttpContext.Session.Clear();
            
            // Giriş sayfasına yönlendir
            return RedirectToPage("/Login");
        }
    }
}

