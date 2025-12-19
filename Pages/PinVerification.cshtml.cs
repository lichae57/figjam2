using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class PinVerificationModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != 6)
            {
                return Page();
            }

            // Dashboard'a yönlendir
            return RedirectToPage("/Dashboard");
        }
    }
}

