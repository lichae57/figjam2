# İnteraktif Kredi - Web Şube 2.0

İnteraktif Kredi firması için geliştirilmiş, yüksek performanslı ve güvenli **Web Şube 2.0** uygulamasıdır. Proje, harici CSS kütüphanelerinden (Bootstrap, Tailwind vb.) bağımsız, tamamen özel tasarım mimarisiyle sıfırdan .NET 8 Razor Pages üzerinde kurgulanmıştır. 

Kullanıcıların **TCKN ve Cep Telefonu** bilgileriyle giriş yapabildiği, entegre API üzerinden **OTP (Tek Kullanımlık Şifre)** SMS doğrulaması sağlayan güvenli bir akışa sahiptir.

---

## 🚀 Temel Özellikler

* **Güvenli Giriş Sistemi:** TCKN ve Cep Telefonu ile doğrulama.
* **SMS OTP Entegrasyonu:** Hazır API bağlantısı üzerinden dinamik SMS kodu üretimi ve doğrulaması.
* **Performans Odaklı UI:** Harici CSS kütüphanesi kullanılmadan, tamamen SCSS ile yazılmış özel, hafif tasarım.
* **Gelişmiş Form Yönetimi:** Mükerrer tıklamaları (Double Submit) önleyen, otomatik veri formatlama (Input Masking) özellikli güvenli form yapısı.

---

## 🛠 Teknoloji Yığını

* **Backend:** C# / .NET 8 (Razor Pages)
* **Frontend:** HTML5, Özel SCSS Mimarisi, Vanilla JS & jQuery (Performans odaklı)
* **API İletişimi:** RESTful API entegrasyonu

---

## 📂 Klasör ve Mimari Yapısı

Proje, okunabilirliği artırmak ve sürdürülebilirliği sağlamak adına kaynak (`source`) ve sunulan (`public`) dosyalar olarak kesin çizgilerle ayrılmıştır:

```text
ProjectRoot/ 
├── Pages/                 # Razor Sayfaları (.cshtml) 
│   ├── Shared/            # Layout, Partial View ve Component'ler 
│   ├── index.cshtml       # Anasayfa 
│   └── ... 
├── Styles/                # Ham SCSS Dosyaları (Derlenmemiş) 
│   ├── abstracts/         # Değişkenler, Mixinler (Çıktı üretmez) 
│   ├── base/              # Reset, Typography, Genel HTML ayarları 
│   ├── components/        # Butonlar, Kartlar, Modallar 
│   ├── pages/             # Sayfaya özel stiller 
│   └── main.scss          # Tüm importların yapıldığı ana dosya 
├── wwwroot/               # İstemciye (Client) giden statik dosyalar 
│   ├── css/               # Derlenmiş ve Minify edilmiş CSS 
│   ├── js/                # Derlenmiş JS dosyaları 
│   └── img/               # Görseller 
├── appsettings.json 
└── Program.cs
