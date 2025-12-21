using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace figjam2.Pages
{
    public class PinVerificationModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Eğer identifier yoksa Dashboard'a yönlendir
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Identifier")))
            {
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != 6)
            {
                return Page();
            }

            // PIN doğrulandı, kullanıcıyı giriş yapmış olarak işaretle
            HttpContext.Session.SetString("IsLoggedIn", "true");
            
            // Kullanıcı bilgilerini session'a kaydet (örnek veri)
            var identifier = HttpContext.Session.GetString("Identifier") ?? "";
            HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
            HttpContext.Session.SetString("UserEmail", "ahmet.yilmaz@email.com");
            HttpContext.Session.SetString("UserPhone", "5551234567");
            HttpContext.Session.SetString("UserJob", "Yazılım Geliştirici");
            HttpContext.Session.SetString("UserIncome", "25000-30000");
            HttpContext.Session.SetString("UserAddress", "Atatürk Mahallesi, Cumhuriyet Caddesi No: 123");
            HttpContext.Session.SetString("UserCity", "İstanbul");
            HttpContext.Session.SetString("UserDistrict", "Kadıköy");
            HttpContext.Session.SetString("UserPostalCode", "34700");

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Ahmet Yılmaz"),
                new Claim(ClaimTypes.NameIdentifier, identifier),
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

            // Dashboard'a yönlendir
            return RedirectToPage("/Dashboard");
        }
    }
}

