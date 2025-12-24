using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using figjam2.Services;
using figjam2.Models;
using System.Text.Json;

namespace figjam2.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(ApiClient apiClient, ILogger<ProfileModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [BindProperty]
        public UserProfileViewModel Profile { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                
                _logger.LogInformation("=== PROFILE PAGE LOAD START ===");
                _logger.LogInformation("Session CustomerId: {CustomerId}", customerId ?? "NULL");
                _logger.LogInformation("Session UserName: {UserName}", HttpContext.Session.GetString("UserName") ?? "NULL");
                _logger.LogInformation("Session Tckn: {Tckn}", HttpContext.Session.GetString("Tckn") ?? "NULL");
                _logger.LogInformation("Session Gsm: {Gsm}", HttpContext.Session.GetString("Gsm") ?? "NULL");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("OnGetAsync: CustomerId bulunamadı, varsayılan değerler kullanılacak");
                    LoadDefaultProfile();
                    _logger.LogInformation("=== PROFILE LOADED (DEFAULT) ===");
                    _logger.LogInformation("Profile.FirstName: {FirstName}", Profile.FirstName);
                    _logger.LogInformation("Profile.LastName: {LastName}", Profile.LastName);
                    _logger.LogInformation("Profile.FullName: {FullName}", Profile.FullName);
                    return Page();
                }

                _logger.LogInformation("OnGetAsync: Profil bilgileri çekiliyor. CustomerId: {CustomerId}", customerId);

                // Paralel olarak adres ve iş bilgilerini çek
                var addressTask = GetCustomerAddressAsync(customerId);
                var jobInfoTask = GetCustomerJobInfoAsync(customerId);

                await Task.WhenAll(addressTask, jobInfoTask);

                var addressResult = await addressTask;
                var jobInfoResult = await jobInfoTask;

                _logger.LogInformation("API Results - Address: {AddressNotNull}, JobInfo: {JobInfoNotNull}", 
                    addressResult != null, jobInfoResult != null);

                // Modeli doldur
                PopulateProfileFromApiResponses(addressResult, jobInfoResult);

                // Session'dan ek bilgileri al
                Profile.TcKimlikNo = HttpContext.Session.GetString("Tckn") ?? Profile.TcKimlikNo;
                Profile.Phone = HttpContext.Session.GetString("Gsm") ?? Profile.Phone;
                Profile.CustomerId = int.TryParse(customerId, out var cid) ? cid : null;

                // Session'a profil bilgilerini kaydet (Header için)
                if (!string.IsNullOrEmpty(Profile.FullName) && Profile.FullName != " ")
                {
                    HttpContext.Session.SetString("UserName", Profile.FullName);
                }

                _logger.LogInformation("=== PROFILE LOADED (API) ===");
                _logger.LogInformation("Profile.FirstName: {FirstName}", Profile.FirstName);
                _logger.LogInformation("Profile.LastName: {LastName}", Profile.LastName);
                _logger.LogInformation("Profile.FullName: {FullName}", Profile.FullName);
                _logger.LogInformation("Profile.TcKimlikNo: {TcKimlikNo}", Profile.TcKimlikNo);
                _logger.LogInformation("Profile.Phone: {Phone}", Profile.Phone);
                _logger.LogInformation("Profile.Address: {Address}", Profile.Address);
                _logger.LogInformation("Profile.City: {City}", Profile.City);
                _logger.LogInformation("Profile.District: {District}", Profile.District);
                _logger.LogInformation("Profile.Profession: {Profession}", Profile.Profession);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnGetAsync: Profil bilgileri çekilirken hata oluştu");
                LoadDefaultProfile();
                ErrorMessage = "Profil bilgileri yüklenirken bir hata oluştu.";
                return Page();
            }
        }

        private async Task<CustomerAddressResponse?> GetCustomerAddressAsync(string customerId)
        {
            try
            {
                var apiUrl = ApiConfig.GetCustomerAddressUrl(customerId);
                _logger.LogInformation("=== ADDRESS API CALL START ===");
                _logger.LogInformation("GetCustomerAddressAsync: API URL = {Url}", apiUrl);
                
                var response = await _apiClient.GetAsync<object>(apiUrl, HttpContext);
                
                _logger.LogInformation("GetCustomerAddressAsync: Response Status = {Status}", response.Status);
                _logger.LogInformation("GetCustomerAddressAsync: IsSuccess = {IsSuccess}", response.IsSuccess);
                _logger.LogInformation("GetCustomerAddressAsync: Error = {Error}", response.Error ?? "NULL");
                _logger.LogInformation("GetCustomerAddressAsync: Data is null = {IsNull}", response.Data == null);
                
                if (response.Data != null)
                {
                    var json = JsonSerializer.Serialize(response.Data, new JsonSerializerOptions { WriteIndented = true });
                    _logger.LogInformation("GetCustomerAddressAsync: RAW JSON Response:\n{Response}", json);
                }
                
                if (response.IsSuccess && response.Data != null)
                {
                    var json = JsonSerializer.Serialize(response.Data);
                    var parsed = ParseAddressResponse(json);
                    _logger.LogInformation("GetCustomerAddressAsync: Parsed Result = {Result}", parsed != null ? "SUCCESS" : "NULL");
                    if (parsed != null)
                    {
                        _logger.LogInformation("GetCustomerAddressAsync: Parsed CityName={City}, DistrictName={District}, FullAddress={Address}", 
                            parsed.CityName, parsed.DistrictName, parsed.FullAddress);
                    }
                    _logger.LogInformation("=== ADDRESS API CALL END ===");
                    return parsed;
                }
                
                _logger.LogWarning("GetCustomerAddressAsync: API başarısız. Status: {Status}, Error: {Error}", 
                    response.Status, response.Error);
                _logger.LogInformation("=== ADDRESS API CALL END (FAILED) ===");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerAddressAsync: Hata oluştu");
                _logger.LogInformation("=== ADDRESS API CALL END (EXCEPTION) ===");
                return null;
            }
        }

        private async Task<CustomerJobInfoResponse?> GetCustomerJobInfoAsync(string customerId)
        {
            try
            {
                var apiUrl = ApiConfig.GetCustomerJobInfoUrl(customerId);
                _logger.LogInformation("=== JOB INFO API CALL START ===");
                _logger.LogInformation("GetCustomerJobInfoAsync: API URL = {Url}", apiUrl);
                
                var response = await _apiClient.GetAsync<object>(apiUrl, HttpContext);
                
                _logger.LogInformation("GetCustomerJobInfoAsync: Response Status = {Status}", response.Status);
                _logger.LogInformation("GetCustomerJobInfoAsync: IsSuccess = {IsSuccess}", response.IsSuccess);
                _logger.LogInformation("GetCustomerJobInfoAsync: Error = {Error}", response.Error ?? "NULL");
                _logger.LogInformation("GetCustomerJobInfoAsync: Data is null = {IsNull}", response.Data == null);
                
                if (response.Data != null)
                {
                    var json = JsonSerializer.Serialize(response.Data, new JsonSerializerOptions { WriteIndented = true });
                    _logger.LogInformation("GetCustomerJobInfoAsync: RAW JSON Response:\n{Response}", json);
                }
                
                if (response.IsSuccess && response.Data != null)
                {
                    var json = JsonSerializer.Serialize(response.Data);
                    var parsed = ParseJobInfoResponse(json);
                    _logger.LogInformation("GetCustomerJobInfoAsync: Parsed Result = {Result}", parsed != null ? "SUCCESS" : "NULL");
                    if (parsed != null)
                    {
                        _logger.LogInformation("GetCustomerJobInfoAsync: Parsed JobTitle={JobTitle}, IncomeRange={IncomeRange}", 
                            parsed.JobTitle, parsed.IncomeRange);
                    }
                    _logger.LogInformation("=== JOB INFO API CALL END ===");
                    return parsed;
                }
                
                _logger.LogWarning("GetCustomerJobInfoAsync: API başarısız. Status: {Status}, Error: {Error}", 
                    response.Status, response.Error);
                _logger.LogInformation("=== JOB INFO API CALL END (FAILED) ===");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerJobInfoAsync: Hata oluştu");
                _logger.LogInformation("=== JOB INFO API CALL END (EXCEPTION) ===");
                return null;
            }
        }

        private CustomerAddressResponse? ParseAddressResponse(string json)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                // data.value veya value veya doğrudan root formatlarını kontrol et
                JsonElement? addressElement = null;

                if (root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
                {
                    if (dataElem.TryGetProperty("value", out var valElem))
                    {
                        addressElement = valElem.ValueKind == JsonValueKind.Array && valElem.GetArrayLength() > 0
                            ? valElem[0]
                            : valElem.ValueKind == JsonValueKind.Object ? valElem : null;
                    }
                }
                else if (root.TryGetProperty("value", out var valElemDirect))
                {
                    addressElement = valElemDirect.ValueKind == JsonValueKind.Array && valElemDirect.GetArrayLength() > 0
                        ? valElemDirect[0]
                        : valElemDirect.ValueKind == JsonValueKind.Object ? valElemDirect : null;
                }
                else if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    addressElement = root[0];
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    addressElement = root;
                }

                if (addressElement.HasValue)
                {
                    return JsonSerializer.Deserialize<CustomerAddressResponse>(
                        addressElement.Value.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParseAddressResponse: JSON parse hatası");
                return null;
            }
        }

        private CustomerJobInfoResponse? ParseJobInfoResponse(string json)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                JsonElement? jobElement = null;

                if (root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
                {
                    if (dataElem.TryGetProperty("value", out var valElem) && valElem.ValueKind == JsonValueKind.Object)
                    {
                        jobElement = valElem;
                    }
                }
                else if (root.TryGetProperty("value", out var valElemDirect) && valElemDirect.ValueKind == JsonValueKind.Object)
                {
                    jobElement = valElemDirect;
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    jobElement = root;
                }

                if (jobElement.HasValue)
                {
                    return JsonSerializer.Deserialize<CustomerJobInfoResponse>(
                        jobElement.Value.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParseJobInfoResponse: JSON parse hatası");
                return null;
            }
        }

        private void PopulateProfileFromApiResponses(CustomerAddressResponse? address, CustomerJobInfoResponse? jobInfo)
        {
            // Session'dan temel bilgileri al (statik değer kullanma!)
            var userName = HttpContext.Session.GetString("UserName") ?? "";
            var nameParts = !string.IsNullOrEmpty(userName) ? userName.Split(' ', 2) : Array.Empty<string>();
            Profile.FirstName = nameParts.Length > 0 ? nameParts[0] : HttpContext.Session.GetString("UserFirstName") ?? "";
            Profile.LastName = nameParts.Length > 1 ? nameParts[1] : HttpContext.Session.GetString("UserLastName") ?? "";

            _logger.LogInformation("PopulateProfileFromApiResponses: Session UserName='{UserName}', Parsed FirstName='{FirstName}', LastName='{LastName}'", 
                userName, Profile.FirstName, Profile.LastName);

            // Adres bilgileri
            if (address != null)
            {
                _logger.LogInformation("PopulateProfileFromApiResponses: API'den adres bilgisi alındı. Address={Address}, CityId={CityId}, TownId={TownId}", 
                    address.Address, address.CityId, address.TownId);
                
                // API'den gelen gerçek alanları kullan
                Profile.Address = address.Address ?? address.FullAddress ?? HttpContext.Session.GetString("UserAddress") ?? "";
                
                // CityId'yi şehir adına çevir (şimdilik ID olarak göster, sonra lookup eklenebilir)
                Profile.City = address.CityId.HasValue 
                    ? GetCityName(address.CityId.Value) 
                    : (address.CityName ?? HttpContext.Session.GetString("UserCity") ?? "");
                
                // TownId'yi ilçe adına çevir (şimdilik ID olarak göster, sonra lookup eklenebilir)
                Profile.District = address.TownId.HasValue 
                    ? GetDistrictName(address.TownId.Value) 
                    : (address.DistrictName ?? HttpContext.Session.GetString("UserDistrict") ?? "");
                
                Profile.PostalCode = address.PostalCode ?? HttpContext.Session.GetString("UserPostalCode") ?? "";
                Profile.AddressId = address.AddressId;
                Profile.AddressType = address.AddressType;
                Profile.CountryCode = address.CountryCode;
                Profile.CityCode = address.CityId?.ToString() ?? address.CityCode;
                Profile.DistrictCode = address.TownId?.ToString() ?? address.DistrictCode;
                Profile.NeighborhoodCode = address.NeighborhoodCode;
                Profile.Street = address.Street;
                Profile.BuildingNo = address.BuildingNo;
                Profile.ApartmentNo = address.ApartmentNo;
                
                _logger.LogInformation("PopulateProfileFromApiResponses: Profile.Address='{Address}', Profile.City='{City}', Profile.District='{District}'", 
                    Profile.Address, Profile.City, Profile.District);
            }
            else
            {
                _logger.LogWarning("PopulateProfileFromApiResponses: API'den adres bilgisi gelmedi, Session'dan alınıyor");
                // Session'dan değerleri al (statik değer kullanma!)
                Profile.Address = HttpContext.Session.GetString("UserAddress") ?? "";
                Profile.City = HttpContext.Session.GetString("UserCity") ?? "";
                Profile.District = HttpContext.Session.GetString("UserDistrict") ?? "";
                Profile.PostalCode = HttpContext.Session.GetString("UserPostalCode") ?? "";
            }

            // İş bilgileri
            if (jobInfo != null)
            {
                _logger.LogInformation("PopulateProfileFromApiResponses: API'den iş bilgisi alındı. OccupationName={OccupationName}, JobGroupName={JobGroupName}", 
                    jobInfo.OccupationName, jobInfo.JobGroupName);
                
                // API'den gelen gerçek alanları kullan
                Profile.Profession = jobInfo.OccupationName 
                    ?? jobInfo.JobGroupName 
                    ?? jobInfo.JobTitle 
                    ?? HttpContext.Session.GetString("UserJob") ?? "";
                    
                Profile.IncomeRange = jobInfo.IncomeRange ?? HttpContext.Session.GetString("UserIncome") ?? "";
                
                _logger.LogInformation("PopulateProfileFromApiResponses: Profile.Profession='{Profession}', Profile.IncomeRange='{IncomeRange}'", 
                    Profile.Profession, Profile.IncomeRange);
            }
            else
            {
                _logger.LogWarning("PopulateProfileFromApiResponses: API'den iş bilgisi gelmedi, Session'dan alınıyor");
                Profile.Profession = HttpContext.Session.GetString("UserJob") ?? "";
                Profile.IncomeRange = HttpContext.Session.GetString("UserIncome") ?? "";
            }

            // E-posta (statik değer kullanma!)
            Profile.Email = HttpContext.Session.GetString("UserEmail") ?? "";
        }
        
        // Şehir lookup tablosu (static - performans için)
        private static readonly Dictionary<int, string> CityLookup = new()
        {
            {1, "Adana"}, {2, "Adıyaman"}, {3, "Afyonkarahisar"}, {4, "Ağrı"}, {5, "Amasya"},
            {6, "Ankara"}, {7, "Antalya"}, {8, "Artvin"}, {9, "Aydın"}, {10, "Balıkesir"},
            {11, "Bilecik"}, {12, "Bingöl"}, {13, "Bitlis"}, {14, "Bolu"}, {15, "Burdur"},
            {16, "Bursa"}, {17, "Çanakkale"}, {18, "Çankırı"}, {19, "Çorum"}, {20, "Denizli"},
            {21, "Diyarbakır"}, {22, "Edirne"}, {23, "Elazığ"}, {24, "Erzincan"}, {25, "Erzurum"},
            {26, "Eskişehir"}, {27, "Gaziantep"}, {28, "Giresun"}, {29, "Gümüşhane"}, {30, "Hakkari"},
            {31, "Hatay"}, {32, "Isparta"}, {33, "Mersin"}, {34, "İstanbul"}, {35, "İzmir"},
            {36, "Kars"}, {37, "Kastamonu"}, {38, "Kayseri"}, {39, "Kırklareli"}, {40, "Kırşehir"},
            {41, "Kocaeli"}, {42, "Konya"}, {43, "Kütahya"}, {44, "Malatya"}, {45, "Manisa"},
            {46, "Kahramanmaraş"}, {47, "Mardin"}, {48, "Muğla"}, {49, "Muş"}, {50, "Nevşehir"},
            {51, "Niğde"}, {52, "Ordu"}, {53, "Rize"}, {54, "Sakarya"}, {55, "Samsun"},
            {56, "Siirt"}, {57, "Sinop"}, {58, "Sivas"}, {59, "Tekirdağ"}, {60, "Tokat"},
            {61, "Trabzon"}, {62, "Tunceli"}, {63, "Şanlıurfa"}, {64, "Uşak"}, {65, "Van"},
            {66, "Yozgat"}, {67, "Zonguldak"}, {68, "Aksaray"}, {69, "Bayburt"}, {70, "Karaman"},
            {71, "Kırıkkale"}, {72, "Batman"}, {73, "Şırnak"}, {74, "Bartın"}, {75, "Ardahan"},
            {76, "Iğdır"}, {77, "Yalova"}, {78, "Karabük"}, {79, "Kilis"}, {80, "Osmaniye"}, {81, "Düzce"}
        };
        
        // İlçe lookup tablosu (örnek ilçeler - İzmir için)
        private static readonly Dictionary<int, string> DistrictLookup = new()
        {
            // İzmir ilçeleri
            {1, "Aliağa"}, {2, "Balçova"}, {3, "Bayındır"}, {4, "Bayraklı"}, {5, "Bergama"},
            {6, "Beydağ"}, {7, "Bornova"}, {8, "Buca"}, {9, "Çeşme"}, {10, "Çiğli"},
            {11, "Dikili"}, {12, "Foça"}, {13, "Konak"}, {14, "Gaziemir"}, {15, "Güzelbahçe"},
            {16, "Karabağlar"}, {17, "Karaburun"}, {18, "Karşıyaka"}, {19, "Kemalpaşa"}, {20, "Kınık"},
            {21, "Kiraz"}, {22, "Menderes"}, {23, "Menemen"}, {24, "Narlıdere"}, {25, "Ödemiş"},
            {26, "Seferihisar"}, {27, "Selçuk"}, {28, "Tire"}, {29, "Torbalı"}, {30, "Urla"},
            // İstanbul ilçeleri
            {100, "Adalar"}, {101, "Arnavutköy"}, {102, "Ataşehir"}, {103, "Avcılar"}, {104, "Bağcılar"},
            {105, "Bahçelievler"}, {106, "Bakırköy"}, {107, "Başakşehir"}, {108, "Bayrampaşa"}, {109, "Beşiktaş"},
            {110, "Beykoz"}, {111, "Beylikdüzü"}, {112, "Beyoğlu"}, {113, "Büyükçekmece"}, {114, "Çatalca"},
            {115, "Çekmeköy"}, {116, "Esenler"}, {117, "Esenyurt"}, {118, "Eyüpsultan"}, {119, "Fatih"},
            {120, "Gaziosmanpaşa"}, {121, "Güngören"}, {122, "Kadıköy"}, {123, "Kağıthane"}, {124, "Kartal"},
            {125, "Küçükçekmece"}, {126, "Maltepe"}, {127, "Pendik"}, {128, "Sancaktepe"}, {129, "Sarıyer"},
            {130, "Silivri"}, {131, "Sultanbeyli"}, {132, "Sultangazi"}, {133, "Şile"}, {134, "Şişli"},
            {135, "Tuzla"}, {136, "Ümraniye"}, {137, "Üsküdar"}, {138, "Zeytinburnu"},
            // Ankara ilçeleri
            {200, "Altındağ"}, {201, "Akyurt"}, {202, "Ayaş"}, {203, "Bala"}, {204, "Beypazarı"},
            {205, "Çamlıdere"}, {206, "Çankaya"}, {207, "Çubuk"}, {208, "Elmadağ"}, {209, "Etimesgut"},
            {210, "Evren"}, {211, "Gölbaşı"}, {212, "Güdül"}, {213, "Haymana"}, {214, "Kalecik"},
            {215, "Kahramankazan"}, {216, "Keçiören"}, {217, "Kızılcahamam"}, {218, "Mamak"}, {219, "Nallıhan"},
            {220, "Polatlı"}, {221, "Pursaklar"}, {222, "Sincan"}, {223, "Şereflikoçhisar"}, {224, "Yenimahalle"}
        };
        
        // Şehir ID'sini şehir adına çevirir
        private string GetCityName(int cityId)
        {
            return CityLookup.TryGetValue(cityId, out var cityName) ? cityName : $"Şehir ({cityId})";
        }
        
        // Şehir adını şehir ID'sine çevirir (güncelleme için)
        private int? GetCityId(string cityName)
        {
            if (string.IsNullOrEmpty(cityName)) return null;
            
            var entry = CityLookup.FirstOrDefault(x => 
                x.Value.Equals(cityName, StringComparison.OrdinalIgnoreCase));
            
            return entry.Key != 0 ? entry.Key : null;
        }
        
        // İlçe ID'sini ilçe adına çevirir
        private string GetDistrictName(int districtId)
        {
            return DistrictLookup.TryGetValue(districtId, out var districtName) ? districtName : $"İlçe ({districtId})";
        }
        
        // İlçe adını ilçe ID'sine çevirir (güncelleme için)
        private int? GetDistrictId(string districtName)
        {
            if (string.IsNullOrEmpty(districtName)) return null;
            
            // "İlçe (13)" formatındaysa ID'yi çıkar
            if (districtName.StartsWith("İlçe (") && districtName.EndsWith(")"))
            {
                var idStr = districtName.Replace("İlçe (", "").Replace(")", "");
                if (int.TryParse(idStr, out var id)) return id;
            }
            
            var entry = DistrictLookup.FirstOrDefault(x => 
                x.Value.Equals(districtName, StringComparison.OrdinalIgnoreCase));
            
            return entry.Key != 0 ? entry.Key : null;
        }

        private void LoadDefaultProfile()
        {
            // Session'dan kullanıcı adını al
            var userName = HttpContext.Session.GetString("UserName");
            var nameParts = !string.IsNullOrEmpty(userName) ? userName.Split(' ', 2) : Array.Empty<string>();
            
            Profile = new UserProfileViewModel
            {
                // Session'dan al, yoksa boş bırak (statik değer kullanma!)
                FirstName = nameParts.Length > 0 ? nameParts[0] : HttpContext.Session.GetString("UserFirstName") ?? "",
                LastName = nameParts.Length > 1 ? nameParts[1] : HttpContext.Session.GetString("UserLastName") ?? "",
                TcKimlikNo = HttpContext.Session.GetString("Tckn") ?? "",
                Phone = HttpContext.Session.GetString("Gsm") ?? "",
                Email = HttpContext.Session.GetString("UserEmail") ?? "",
                BirthDate = null, // Statik tarih kullanma
                Address = HttpContext.Session.GetString("UserAddress") ?? "",
                City = HttpContext.Session.GetString("UserCity") ?? "",
                District = HttpContext.Session.GetString("UserDistrict") ?? "",
                PostalCode = HttpContext.Session.GetString("UserPostalCode") ?? "",
                Profession = HttpContext.Session.GetString("UserJob") ?? "",
                IncomeRange = HttpContext.Session.GetString("UserIncome") ?? ""
            };

            var customerId = HttpContext.Session.GetString("CustomerId");
            if (int.TryParse(customerId, out var cid))
            {
                Profile.CustomerId = cid;
            }
            
            _logger.LogInformation("LoadDefaultProfile: Varsayılan profil yüklendi (Session'dan). FirstName: {FirstName}, LastName: {LastName}", 
                Profile.FirstName, Profile.LastName);
        }

        // Profil Güncelleme POST metodu
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            try
            {
                _logger.LogInformation("=== PROFILE UPDATE START ===");
                
                // Request body'den JSON oku
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                _logger.LogInformation("OnPostUpdateProfileAsync: Request Body: {Body}", requestBody);
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return new JsonResult(new { success = false, error = "Güncelleme verisi bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }
                
                // JSON'u parse et
                var updateData = JsonSerializer.Deserialize<JsonElement>(requestBody);
                
                var customerId = HttpContext.Session.GetString("CustomerId");
                var tckn = HttpContext.Session.GetString("Tckn");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    return new JsonResult(new { success = false, error = "Müşteri ID bulunamadı. Lütfen tekrar giriş yapın." })
                    {
                        StatusCode = 400
                    };
                }

                _logger.LogInformation("OnPostUpdateProfileAsync: CustomerId={CustomerId}, TCKN={Tckn}", customerId, tckn);
                
                // Değerleri al
                var address = GetJsonString(updateData, "address");
                var city = GetJsonString(updateData, "city");
                var district = GetJsonString(updateData, "district");
                var profession = GetJsonString(updateData, "profession");
                var incomeRange = GetJsonString(updateData, "incomeRange");
                
                _logger.LogInformation("OnPostUpdateProfileAsync: Address={Address}, City={City}, District={District}", 
                    address, city, district);
                
                // Şehir adını CityId'ye çevir
                var cityId = GetCityId(city);
                var townId = GetDistrictId(district);
                
                _logger.LogInformation("OnPostUpdateProfileAsync: CityId={CityId}, TownId={TownId}", cityId, townId);
                
                var errors = new List<string>();
                var successes = new List<string>();

                // Adres güncelleme - API'nin beklediği format
                var addressRequest = new
                {
                    tckn = tckn,
                    customerId = int.Parse(customerId),
                    cityId = cityId ?? 35, // Varsayılan İzmir
                    townId = townId ?? 13, // Varsayılan ilçe
                    address = address,
                    source = 2 // Web kaynağı
                };

                _logger.LogInformation("OnPostUpdateProfileAsync: Address Request: {Request}", 
                    JsonSerializer.Serialize(addressRequest));

                var addressResponse = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_ADDRESS_CREATE, addressRequest, HttpContext);

                _logger.LogInformation("OnPostUpdateProfileAsync: Address Response - Status={Status}, Success={Success}, Error={Error}", 
                    addressResponse.Status, addressResponse.IsSuccess, addressResponse.Error);

                if (addressResponse.IsSuccess)
                {
                    successes.Add("Adres bilgileri güncellendi");
                    
                    // Session'ı güncelle
                    HttpContext.Session.SetString("UserAddress", address ?? "");
                    HttpContext.Session.SetString("UserCity", city ?? "");
                    HttpContext.Session.SetString("UserDistrict", district ?? "");
                }
                else
                {
                    errors.Add($"Adres güncellenemedi: {addressResponse.Error}");
                }

                // İş bilgisi güncelleme (eğer meslek veya gelir varsa)
                if (!string.IsNullOrEmpty(profession) || !string.IsNullOrEmpty(incomeRange))
                {
                    var jobRequest = new
                    {
                        customerId = int.Parse(customerId),
                        occupationName = profession,
                        jobGroupName = profession,
                        incomeRange = incomeRange
                    };

                    _logger.LogInformation("OnPostUpdateProfileAsync: Job Request: {Request}", 
                        JsonSerializer.Serialize(jobRequest));

                    var jobResponse = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_JOB_PROFILE, jobRequest, HttpContext);

                    _logger.LogInformation("OnPostUpdateProfileAsync: Job Response - Status={Status}, Success={Success}, Error={Error}", 
                        jobResponse.Status, jobResponse.IsSuccess, jobResponse.Error);

                    if (jobResponse.IsSuccess)
                    {
                        successes.Add("İş bilgileri güncellendi");
                        
                        // Session'ı güncelle
                        HttpContext.Session.SetString("UserJob", profession ?? "");
                        HttpContext.Session.SetString("UserIncome", incomeRange ?? "");
                    }
                    else
                    {
                        errors.Add($"İş bilgileri güncellenemedi: {jobResponse.Error}");
                    }
                }

                _logger.LogInformation("=== PROFILE UPDATE END ===");

                if (successes.Count > 0)
                {
                    var message = string.Join(". ", successes) + ".";
                    if (errors.Count > 0)
                    {
                        message += " Ancak bazı hatalar oluştu: " + string.Join(", ", errors);
                    }
                    
                    return new JsonResult(new { 
                        success = true, 
                        message = message,
                        updatedData = new {
                            address,
                            city,
                            district,
                            profession,
                            incomeRange
                        }
                    });
                }

                return new JsonResult(new { success = false, error = string.Join(", ", errors) })
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostUpdateProfileAsync: Beklenmeyen hata");
                return new JsonResult(new { success = false, error = $"Beklenmeyen bir hata oluştu: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }
        
        // JSON'dan string değer okuma helper'ı
        private string? GetJsonString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
            return null;
        }

        // Müşteri Adres Getir (AJAX için)
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetCustomerAddress()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(customerId))
                {
                    return new JsonResult(new { success = false, error = "Müşteri ID bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerAddressUrl(customerId), HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // Müşteri Adres Kaydet (AJAX için)
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostCustomerAddress()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_ADDRESS_CREATE, JsonSerializer.Deserialize<object>(body), HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // Müşteri İş Bilgisi Getir (AJAX için)
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetCustomerJobInfo()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(customerId))
                {
                    return new JsonResult(new { success = false, error = "Müşteri ID bulunamadı" })
                    {
                        StatusCode = 400
                    };
                }

                var response = await _apiClient.GetAsync<object>(ApiConfig.GetCustomerJobInfoUrl(customerId), HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // Müşteri İş Profili Kaydet (AJAX için)
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostCustomerJobProfile()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                var response = await _apiClient.PostAsync<object>(ApiConfig.CUSTOMER_JOB_PROFILE, JsonSerializer.Deserialize<object>(body), HttpContext);

                if (response.IsSuccess)
                {
                    return new JsonResult(new { success = true, data = response.Data });
                }

                return new JsonResult(new { success = false, error = response.Error })
                {
                    StatusCode = response.Status
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }
    }
}
