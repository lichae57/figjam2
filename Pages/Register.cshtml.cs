using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string? FirstName { get; set; }

        [BindProperty]
        public string? LastName { get; set; }

        [BindProperty]
        public string? TcNumber { get; set; }

        [BindProperty]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? OtpCode { get; set; }

        [BindProperty]
        public bool TermsAccepted { get; set; }

        [BindProperty]
        public bool PrivacyAccepted { get; set; }

        [BindProperty]
        public bool KvkkAccepted { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                return Page();
            }

            if (string.IsNullOrEmpty(TcNumber) || TcNumber.Length != 11)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(OtpCode) || OtpCode.Length != 6)
            {
                return Page();
            }

            if (!TermsAccepted || !PrivacyAccepted || !KvkkAccepted)
            {
                return Page();
            }

            // Set session as logged in after successful registration
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", $"{FirstName} {LastName}");
            HttpContext.Session.SetString("Identifier", TcNumber ?? "");
            HttpContext.Session.SetString("UserPhone", Phone ?? "");
            HttpContext.Session.SetString("UserEmail", Email ?? "");

            // Redirect to Dashboard
            return RedirectToPage("/Dashboard");
        }
    }
}

