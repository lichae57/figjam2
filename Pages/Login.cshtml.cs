using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Identifier { get; set; }

        [BindProperty]
        public string? OtpCode { get; set; }

        [BindProperty]
        public bool KvkkAccepted { get; set; }

        public IActionResult OnGet()
        {
            // If already logged in, redirect to Dashboard
            if (HttpContext.Session.GetString("IsLoggedIn") == "true")
            {
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }

        public IActionResult OnPostLogin()
        {
            // Set session as logged in
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
            HttpContext.Session.SetString("Identifier", Identifier ?? "");

            return RedirectToPage("/Dashboard");
        }
    }
}
