using System.ComponentModel.DataAnnotations;

namespace figjam2.Models
{
    public class UserProfileViewModel
    {
        // Kişisel Bilgiler
        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "TC Kimlik No")]
        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No yalnızca rakam içermelidir")]
        public string TcKimlikNo { get; set; } = string.Empty;

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Telefon numarası 10-11 haneli olmalıdır")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? Email { get; set; }

        // Adres Bilgileri
        [Display(Name = "Adres")]
        [Required(ErrorMessage = "Adres alanı zorunludur")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "İl")]
        [Required(ErrorMessage = "İl seçimi zorunludur")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "İlçe")]
        [Required(ErrorMessage = "İlçe alanı zorunludur")]
        public string District { get; set; } = string.Empty;

        [Display(Name = "Posta Kodu")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Posta kodu 5 haneli olmalıdır")]
        public string? PostalCode { get; set; }

        // İş Bilgileri
        [Display(Name = "Meslek")]
        public string? Profession { get; set; }

        [Display(Name = "Gelir Aralığı")]
        public string? IncomeRange { get; set; }

        // Adres detayları için ek alanlar (API'den gelen)
        public int? AddressId { get; set; }
        public int? CustomerId { get; set; }
        public string? AddressType { get; set; }
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? NeighborhoodCode { get; set; }
        public string? Street { get; set; }
        public string? BuildingNo { get; set; }
        public string? ApartmentNo { get; set; }

        // Tam ad için helper property
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Maskelenmiş TC Kimlik için helper property
        public string MaskedTcKimlikNo
        {
            get
            {
                if (string.IsNullOrEmpty(TcKimlikNo) || TcKimlikNo.Length < 11)
                    return TcKimlikNo;
                return $"{TcKimlikNo[..2]}*****{TcKimlikNo[^2..]}";
            }
        }
    }

    // API'den gelen adres yanıtı için DTO (Gerçek API formatı)
    public class CustomerAddressResponse
    {
        public string? TCKN { get; set; }
        public int CustomerId { get; set; }
        public int? CityId { get; set; }
        public int? TownId { get; set; }
        public string? Address { get; set; }
        public int? EmployeeId { get; set; }
        public int? Source { get; set; }
        public DateTime? CreateDate { get; set; }
        
        // Eski alanlar (uyumluluk için)
        public int AddressId { get; set; }
        public string? AddressType { get; set; }
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? CityName { get; set; }
        public string? DistrictCode { get; set; }
        public string? DistrictName { get; set; }
        public string? NeighborhoodCode { get; set; }
        public string? NeighborhoodName { get; set; }
        public string? Street { get; set; }
        public string? BuildingNo { get; set; }
        public string? ApartmentNo { get; set; }
        public string? PostalCode { get; set; }
        public string? FullAddress { get; set; }
    }

    // API'den gelen iş bilgisi yanıtı için DTO (Gerçek API formatı)
    public class CustomerJobInfoResponse
    {
        public int CustomerId { get; set; }
        public string? JobGroupName { get; set; }
        public string? OccupationName { get; set; }
        public string? TitleCompany { get; set; }
        public string? CompanyPosition { get; set; }
        public int? WorkingYears { get; set; }
        public int? WorkingMonth { get; set; }
        
        // Eski alanlar (uyumluluk için)
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? IncomeRange { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public string? WorkType { get; set; }
        public int? ExperienceYears { get; set; }
    }

    // Adres güncelleme için request modeli
    public class UpdateAddressRequest
    {
        public int CustomerId { get; set; }
        public string AddressType { get; set; } = "HOME";
        public string? CountryCode { get; set; } = "TR";
        public string? CityCode { get; set; }
        public string? CityName { get; set; }
        public string? DistrictCode { get; set; }
        public string? DistrictName { get; set; }
        public string? NeighborhoodCode { get; set; }
        public string? Street { get; set; }
        public string? BuildingNo { get; set; }
        public string? ApartmentNo { get; set; }
        public string? PostalCode { get; set; }
        public string? FullAddress { get; set; }
    }

    // İş bilgisi güncelleme için request modeli
    public class UpdateJobProfileRequest
    {
        public int CustomerId { get; set; }
        public string? JobTitle { get; set; }
        public string? IncomeRange { get; set; }
        public decimal? MonthlyIncome { get; set; }
    }
}

