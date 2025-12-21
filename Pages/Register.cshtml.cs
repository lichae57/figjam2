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

            // Simulate successful registration
            return RedirectToPage("/Login");
        }
    }
}

