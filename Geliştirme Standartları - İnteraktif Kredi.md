**İnteraktif Kredi - Geliştirme Standartları** **\(v1.1\) **

Proje Tipi: C\# Razor Pages Web Uygulaması Teknoloji Yığını: .NET Core / 8\+, SCSS \(Özel Tasarım\), jQuery Tarih: 19.12.2025 

Özet: Bu döküman; performans odaklı, harici CSS kütüphanesi barındırmayan, snake\_case yapısını benimseyen ve finansal veri güvenliğini esas alan geliştirme kural arını içerir. 

**1. Klasör ve Dosya Yapısı \(Folder Structure\)** Projenin derlenen \(source\) ve sunulan \(public\) dosyaları kesin çizgilerle ayrılmalıdır. 

Okunabilirliği artırmak adına aşağıdaki ağaç yapısına sadık kalınacaktır: Plaintext 

None

ProjectRoot/ 

├── Pages/ \# Razor Sayfaları \(.cshtml\) 

│ ├── Shared/ \# Layout, Partial View ve Component'ler 

│ ├── index.cshtml \# Anasayfa 

│ └── ... 

├── Styles/ \# Ham SCSS Dosyaları \(Derlenmemiş\) 

│ ├── abstracts/ \# Değişkenler, Mixinler \(Çıktı üretmez\) 

│ │ ├── \_variables.scss 

│ │ └── \_mixins.scss 

│ ├── base/ \# Reset, Typography, Genel HTML ayarları 

│ ├── components/ \# Butonlar, Kartlar, Modallar 

│ │ ├── \_buttons.scss 

│ │ └── \_forms.scss 

│ ├── pages/ \# Sayfaya özel stiller 

│ │ └── \_kredi\_basvuru.scss 

│ └── main.scss \# Tüm importların yapıldığı ana dosya 

├── wwwroot/ \# İstemciye \(Client\) giden statik dosyalar 

│ ├── css/ \# Derlenmiş ve Minify edilmiş 

CSS 

│ ├── js/ \# Derlenmiş JS dosyaları 

│ └── img/ \# Görseller 

├── appsettings.json 

└── Program.cs 

****

**2. İsimlendirme Kuralları \(Naming Conventions\)** Projede **Front-end** \(HTML/CSS/JS\) tarafında snake\_case, **Back-end** \(C\#\) tarafında ise dilin standardı olan PascalCase kul anılacaktır. 

**Alan** 

**Format** 

**Örnek** 

**Açıklama** 

**CSS Class / ID** 

snake\_case 

.credit\_amount\_input 

HTML okunabilirliği için zorunlu. 

**JS Değişkenleri** 

snake\_case 

let total\_interest = 0; 

CSS ile uyum sağlaması için. 

**JS Fonksiyonları** 

snake\_case 

function calculate\_payment\(\) 

Genel JS standardı. 

**Resim Dosyaları** 

snake\_case 

bank\_logo\_white.png 

Dosya sistemi uyumu için. 

**Razor Dosyaları** 

PascalCase 

ApplicationForm.cshtml 

.NET standardı. 

**C\# Property** 

PascalCase 

public decimal LoanAmount 

Backend standardı. 

****

**3. SCSS ve CSS Mimarisi **

Harici bir CSS Framework \(Bootstrap, Tailwind vb.\) **kullanılmayacaktır**. Tasarım tamamen özel eştirilebilir ve hafif tutulacaktır. 

**3.1. Class İsimlendirme Metodolojisi** BEM \(Block, Element, Modifier\) yapısının snake\_case uyarlaması kul anılacaktır. 

● **Blok:** .loan\_card 

● **Eleman \(\_\_\):** .loan\_card\_\_header \(İki alt çizgi\) 

● **Durum \(--\):** .loan\_card--approved \(İki tire\) **Örnek SCSS Kod Bloğu: **

SCSS 

None

// Styles/components/\_buttons.scss 

.btn\_primary \{ 

background-color: $brand\_color; 

color: $white; 

padding: 12px 24px; 

border-radius: 8px; 

border: none; 

cursor: pointer; 



// Durum: Mouse üzerine gelince 

&:hover \{ 

background-color: darken\($brand\_color, 10%\); 

\} 

// Varyasyon: Geniş buton 

&--full\_width \{ 

width: 100%; 

display: block; 

\} 

\} 

****

**3.2. Yasaklar **

● HTML etiketine doğrudan stil yazmak \(div \{ ... \}\) yasaktır \(Reset css hariç\). 

● \!important kul anımı kesinlikle yasaktır. 

● Derinlemesine iç içe yazım \(Nesting\) 3 seviyeyi geçmemelidir. 

**4. JavaScript ve jQuery Kullanımı **

Sayfa yüklenme hızını etkilememek adına jQuery performanslı kul anılmalıdır. 

**4.1. Selector Caching \(Seçici Önbellekleme\)** DOM taraması maliyetli bir işlemdir. Bir elemente birden fazla kez erişilecekse, mutlaka bir değişkene atanmalıdır. 

**Doğru Kullanım:** 

JavaScript

// DOM sadece bir kez taranır 

var $apply\_btn = $\('\#apply\_btn'\); $apply\_btn.prop\('disabled', true\); 

$apply\_btn.on\('click', function\(\) \{ 

send\_application\(\); 

\}\); 

****

**4.2. Olay Yönetimi \(Event Delegation\)** Dinamik olarak sonradan eklenen elementler için document seviyesinde dinleme yapılmalıdır. 

JavaScript

$\(document\).on\('click', '.dynamic\_option', function\(\) \{ 

// İşlemler... 

\}\); 

****

**4.3. Form Güvenliği ve UX \(ÖNEMLİ\)** İnteraktif kredi başvurularında mükerrer kaydı ve hatalı girişi önlemek zorunludur. 

● **Double Submit Prevention:** Form submit edildiği anda buton kilitlenmelidir. 

JavaScript



$\('\#loan\_form'\).on\('submit', function\(\) \{ 

var $btn = $\(this\).find\('\#submit\_btn'\); $btn.prop\('disabled', true\).text\('İşleniyor...'\); 

\}\); 



● **Input Masking:** Para birimi ve telefon alanlarında kul anıcı yazarken formatlama yapılmalıdır \(10000 -> 10.000 ₺\). Bunun için hafif jQuery eklentileri veya özel fonksiyonlar kul anılmalıdır. 

**5. C\# Razor Pages \(Backend\) Kuralları **

● **Logic Ayrımı:** .cshtml \(View\) dosyalarında veritabanı sorgusu \(DbContext\) çalıştırılamaz. Tüm mantık .cshtml.cs \(PageModel\) tarafında olmalıdır. 

● **Tag Helpers:** <input asp-for="LoanAmount" class="form\_input" /> yapısı standarttır. 

**6. Performans, Kütüphane Yönetimi ve Yükleme** **Stratejileri **

Projenin teknik başarısı, **Sayfa Açılış Hızı \(Page Speed\)** ve **SEO Puanı** ile ölçülecektir. 

**6.1. CSS Kütüphane Politikası **

● **Harici Kütüphane:** CSS tarafında framework \(Bootstrap vb.\) kul anımı yasaktır. 

● **Özel Tasarım:** Sadece kul anılan stil er SCSS ile yazılır, "Unused CSS" oluşumu engel enir. 

**6.2. JavaScript Kütüphane Seçimi **

● **Öncelik:** İşlevsel ihtiyaçlar öncelikle **Vanilla JS** veya mevcut **jQuery** fonksiyonları ile çözülmelidir. 

● **Zorunlu Haller:** Kütüphane zorunluysa \(Örn: Karmaşık bir Slider\), muadil eri arasındaki en hafif \("lightweight"\) olan seçilmelidir. 

**6.3. Akıllı Yükleme Stratejisi \(Lazy Loading\) **

● **Global Yüklemeden Kaçınma:** Sadece belirli bir sayfada kul anılan scriptler \_Layout.cshtml dosyasına eklenmez. İlgili sayfaya eklenir. 

● **On-Demand Loading:** Sayfanın alt kısmında kalan veya modal içinde çalışan ağır kütüphaneler, sayfa açılışında yüklenmez. Kul anıcı o alana geldiğinde \(Intersection Observer\) yüklenir. 

**7. Finansal Veri ve Güvenlik Standartları \(Kritik\)** Kredi projesi olması sebebiyle aşağıdaki kural ar esnetilemez: **7.1. Veri Tipleri ve Hesaplama **

● **Decimal Zorunluluğu:** Parasal değerler \(Kredi tutarı, faiz, taksit\) tutulurken float veya double kesinlikle kul anılmaz. Kesinlik kaybını önlemek için C\# tarafında decimal, veritabanında decimal\(18,2\) vb. kul anılmalıdır. 

**7.2. Veri Gizliliği \(Logging\) **

● **Maskeleme:** Log mekanizmalarına kul anıcının TCKN, Telefon No veya Gelir bilgisi açık \(plain-text\) yazılmamalıdır. Hata takibi için gerekiyorsa maskelenmelidir \(Örn: 12\*\*\*\*\*\*\*90\). 

**7.3. Güvenlik \(XSS\) **

● **Html.Raw Yasağı:** Kul anıcıdan gelen hiçbir veri, güvenlik onayı olmadan 

@Html.Raw\(\) ile ekrana basılamaz. 

**8. Kalite Kontrol ve Sıkça Yapılan Hatalar Listesi** Geliştiriciler, kodlarını "Code Review" aşamasına göndermeden önce aşağıdaki maddeleri kontrol etmekle yükümlüdür. 

● \[ \] **Hatalı İsimlendirme:** CSS class'larında camelCase \(örn: loanAmount\) kul anıldı mı? -> **HATA** \(loan\_amount olmalı\). 

● \[ \] **Performans:** jQuery seçicisi döngü içinde tekrar tekrar çağrıldı mı? -> **HATA** \(Değişkene atanmalı\). 

● \[ \] **Veri Tipi:** Para hesaplamalarında double kul anıldı mı? -> **HATA** \(decimal olmalı\). 

● \[ \] **Mimari:** .cshtml içinde SQL sorgusu var mı? -> **HATA** \(PageModel'e taşınmalı\). 

● \[ \] **UX:** Form gönderilirken buton kilitlenmiyor mu? -> **HATA** \(Çift kayıt riski\). 

● \[ \] **Clean Code:** Canlıya çıkacak kodda console.log veya debugger unutuldu mu? -> **HATA**. 




# Document Outline

+ İnteraktif Kredi - Geliştirme Standartları \(v1.1\)   
	+ 1. Klasör ve Dosya Yapısı \(Folder Structure\)  
	+   
	+ 2. İsimlendirme Kuralları \(Naming Conventions\)  
	+   
	+ 3. SCSS ve CSS Mimarisi   
		+ 3.1. Class İsimlendirme Metodolojisi  
		+   
		+ 3.2. Yasaklar  

	+ 4. JavaScript ve jQuery Kullanımı   
		+ 4.1. Selector Caching \(Seçici Önbellekleme\)  
		+   
		+ 4.2. Olay Yönetimi \(Event Delegation\)  
		+   
		+ 4.3. Form Güvenliği ve UX \(ÖNEMLİ\)  

	+ 5. C\# Razor Pages \(Backend\) Kuralları  
	+ 6. Performans, Kütüphane Yönetimi ve Yükleme Stratejileri   
		+ 6.1. CSS Kütüphane Politikası  
		+ 6.2. JavaScript Kütüphane Seçimi  
		+ 6.3. Akıllı Yükleme Stratejisi \(Lazy Loading\)  

	+ 7. Finansal Veri ve Güvenlik Standartları \(Kritik\)   
		+ 7.1. Veri Tipleri ve Hesaplama  
		+ 7.2. Veri Gizliliği \(Logging\)  
		+ 7.3. Güvenlik \(XSS\)  

	+ 8. Kalite Kontrol ve Sıkça Yapılan Hatalar Listesi



