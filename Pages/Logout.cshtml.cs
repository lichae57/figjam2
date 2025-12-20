using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Session'ı temizle
            HttpContext.Session.Clear();
            
            // Dashboard'a yönlendir
            return RedirectToPage("/Dashboard");
        }
    }
}

