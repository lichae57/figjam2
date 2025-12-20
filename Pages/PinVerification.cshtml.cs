using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public IActionResult OnPost(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != 6)
            {
                return Page();
            }

            // PIN doğrulandı, kullanıcıyı giriş yapmış olarak işaretle
            HttpContext.Session.SetString("IsLoggedIn", "true");
            
            // Kullanıcı bilgilerini session'a kaydet (örnek veri)
            HttpContext.Session.SetString("UserName", "Ahmet Yılmaz");
            HttpContext.Session.SetString("UserEmail", "ahmet.yilmaz@email.com");
            HttpContext.Session.SetString("UserPhone", "5551234567");
            HttpContext.Session.SetString("UserJob", "Yazılım Geliştirici");
            HttpContext.Session.SetString("UserIncome", "25000-30000");
            HttpContext.Session.SetString("UserAddress", "Atatürk Mahallesi, Cumhuriyet Caddesi No: 123");
            HttpContext.Session.SetString("UserCity", "İstanbul");
            HttpContext.Session.SetString("UserDistrict", "Kadıköy");
            HttpContext.Session.SetString("UserPostalCode", "34700");

            // Dashboard'a yönlendir
            return RedirectToPage("/Dashboard");
        }
    }
}

