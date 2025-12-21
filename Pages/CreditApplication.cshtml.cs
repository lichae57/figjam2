using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace figjam2.Pages
{
    public class CreditApplicationModel : PageModel
    {
        public string CreditType { get; set; } = "";
        public string CreditTypeName { get; set; } = "";

        public void OnGet()
        {
            CreditType = Request.Query["type"].ToString();
            
            CreditTypeName = CreditType switch
            {
                "housing" => "Konut Kredisi",
                "vehicle" => "Taşıt Kredisi",
                "consumer" => "İhtiyaç Kredisi",
                _ => "Kredi"
            };
        }
    }
}

