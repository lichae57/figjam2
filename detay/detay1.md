UI Tasarım Referans Analizi ve Uygulama Planı Bu belge, ekteki görselde
yer alan kullanıcı arayüzü (UI) detaylarını referans alarak yeni bir
sayfa oluşturmak için hazırlanmıştır.

1\. Genel Tasarım Dili Stil: Modern, temiz ve minimal \"SaaS\" estetiği.

Renk Paleti: \* Arka Plan: Saf beyaz veya çok açık gri (#F9FAFB).

Vurgu Rengi: Canlı mor/indigo tonları (Örn: #6366F1).

Metin: Başlıklar için koyu antrasit (#111827), gövde metni için orta gri
(#4B5563).

Kenarlıklar: İnce (1px), düşük kontrastlı gri çizgiler. border-radius
değerleri yumuşak (genellikle 8px veya 12px).

2\. Yapısal Bileşenler A. Kart Yapıları (Cards) Gölge: Çok hafif,
yayılmış \"soft shadow\" (Örn: 0 4px 6px -1px rgb(0 0 0 / 0.1)).

Padding: Elemanlar arasında geniş nefes alma alanları (whitespace).

Header Kısmı: Kart başlığı sol tarafta kalın fontla, aksiyon butonları
sağ tarafta yer alıyor.

B. Veri Tabloları ve Listeler Hücre Yapısı: Dikey çizgiler yerine yatay
ayırıcı çizgiler tercih edilmiş.

Badge (Etiket) Kullanımı: Durum belirteçleri (Örn: \"Active\",
\"Pending\") için yuvarlatılmış köşeli, hafif arka plan rengine sahip
küçük etiketler.

İkonlar: İnce çizgili (outline) lineer ikon setleri (Lucide React veya
Heroicons benzeri).

C. Tipografi Font Ailesi: Sans-serif (Inter, Geist veya Roboto).

Hiyerarşi: Başlıklar font-semibold, alt metinler daha küçük text-sm ve
düşük opaklıkta.

3\. İnteraktif Öğeler Butonlar: \* Primary: Dolu renk, beyaz metin.

Secondary: Beyaz arka plan, ince gri çerçeve.

Input Alanları: Odaklanıldığında (focus) vurgu rengiyle parlayan ince
kenarlıklar.

4\. Cursor İçin Talimatlar (Prompt Önerisi) \"Ekteki görseldeki tasarım
dilini kullanarak bir \[SAYFA ADI\] oluşturmanı istiyorum. Özellikle
kart yapılarını, yazı tipi hiyerarşisini ve buton stillerini birebir
uygula. Tailwind CSS kullanarak modern, temiz ve profesyonel bir görünüm
elde et. Renk paleti olarak Indigo/Violet tonlarını baz al.\"
