using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostLogin()
        {
            // Set session as logged in
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
            HttpContext.Session.SetString("Identifier", Identifier ?? "");

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Ahmet Yılmaz"),
                new Claim(ClaimTypes.NameIdentifier, Identifier ?? ""),
                new Claim("IsLoggedIn", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Dashboard");
        }
    }
}
