using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    [IgnoreAntiforgeryToken]
    public class ProfileManagementModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostUpdateAddress([FromForm] string address, [FromForm] string city, [FromForm] string district, [FromForm] string postalCode)
        {
            if (!string.IsNullOrEmpty(address))
                HttpContext.Session.SetString("UserAddress", address);
            if (!string.IsNullOrEmpty(city))
            {
                // Şehir adını doğru formatta kaydet (ilk harf büyük)
                var cityFormatted = char.ToUpper(city[0]) + city.Substring(1).ToLower();
                HttpContext.Session.SetString("UserCity", cityFormatted);
            }
            if (!string.IsNullOrEmpty(district))
                HttpContext.Session.SetString("UserDistrict", district);
            if (!string.IsNullOrEmpty(postalCode))
                HttpContext.Session.SetString("UserPostalCode", postalCode);

            return new JsonResult(new { success = true, message = "Adres bilgileri başarıyla güncellendi!" });
        }

        public IActionResult OnPostUpdateJob([FromForm] string profession, [FromForm] string income)
        {
            if (!string.IsNullOrEmpty(profession))
                HttpContext.Session.SetString("UserJob", profession);
            
            // Income değerini her zaman kaydet
            if (!string.IsNullOrEmpty(income))
            {
                HttpContext.Session.SetString("UserIncome", income.Trim());
            }

            var savedIncome = HttpContext.Session.GetString("UserIncome");
            
            return new JsonResult(new { 
                success = true, 
                message = "İş bilgileri başarıyla güncellendi!",
                receivedIncome = income,
                savedIncome = savedIncome
            });
        }
    }
}

