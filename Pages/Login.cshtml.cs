using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return Page();
            }

            // PIN doğrulama sayfasına yönlendir
            return RedirectToPage("/PinVerification");
        }
    }
}

