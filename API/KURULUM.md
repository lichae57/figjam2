# 🚀 İnteraktif Kredi API Test Paneli - Kurulum Rehberi

## 📋 Gereksinimler

- **Node.js** 18+ (önerilir: v20)
- **pnpm** (veya npm/yarn)

## 🔧 Kurulum Adımları

### 1️⃣ Projeyi Aç
```bash
# Zip dosyasını çıkart ve klasöre gir
cd WebSube
```

### 2️⃣ Bağımlılıkları Yükle
```bash
pnpm install
# veya
npm install
# veya
yarn install
```

### 3️⃣ Development Server'ı Başlat
```bash
pnpm dev
# veya
npm run dev
```

### 4️⃣ Tarayıcıda Aç
```
http://localhost:3000
```

## ✅ Başarılı Kurulum Kontrol

Eğer tarayıcıda şunları görüyorsanız kurulum başarılı:
- ✅ Sol tarafta 12 endpoint listesi
- ✅ Sağ tarafta "İnteraktif Kredi API Dokümantasyonu" yazısı
- ✅ Bearer Token: `fe7vSdh1Qq...` görünüyor

## 🎯 Nasıl Kullanılır?

1. **Sol menüden** bir endpoint seç (örn: "TCKN & GSM Doğrulama")
2. **Sağ panelde** form otomatik açılır (örnek veriler dolu gelir)
3. **"Test Et"** butonuna bas
4. **Sonuçları** aynı sayfada sağda gör

## 🔐 Token Bilgisi

- Tüm endpoint'ler otomatik olarak Bearer token ile çağrılır
- Default token: `fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s`
- Token değiştirmek için: `lib/config.ts` dosyasını düzenle

## 📝 Örnek Veriler (Formlar Dolu Gelir)

- **TCKN:** 12345678901
- **GSM:** 5551112233
- **Customer ID:** 1000849
- **Dealer ID:** 1081
- **OTP:** 123456
- **KVKK ID:** 2

## 🐛 Sorun Giderme

### Port 3000 kullanımda hatası
```bash
# Farklı port kullan
pnpm dev -- -p 3001
```

### Bağımlılık yükleme hatası
```bash
# Cache'i temizle ve tekrar dene
rm -rf node_modules package-lock.json
pnpm install
```

### Next.js build hatası
```bash
# .next klasörünü sil ve tekrar başlat
rm -rf .next
pnpm dev
```

## 📦 Production Build

```bash
# Build oluştur
pnpm build

# Production'da çalıştır
pnpm start
```

## 🏗️ Proje Yapısı

```
WebSube/
├── app/
│   ├── api/ep/          # API proxy endpoints (12 adet)
│   ├── flow/            # Flow sayfaları (opsiyonel)
│   ├── page.tsx         # Ana sayfa (API test arayüzü)
│   └── layout.tsx       # Root layout
├── components/          # UI bileşenleri
├── lib/
│   ├── config.ts        # Token yapılandırması
│   ├── ep/              # API client
│   ├── store/           # State management
│   └── validations.ts   # Zod schemas
├── package.json
└── README.md
```

## 🌐 Endpoint'ler

Toplam **12 endpoint** test edilebilir:

### customers-api.azurewebsites.net
1. TCKN & GSM Doğrulama
2. Müşteri Adres
3. Maaş Bilgisi
4. İş Bilgisi
5. İş Profili

### api-idc.azurewebsites.net
6. KVKK Metni
7. KVKK Onay
8. OTP Üret
9. OTP SMS Gönder
10. OTP Doğrula
11. Rapor Listesi
12. Rapor Detayı

## 💡 İpuçları

- Endpoint'lerin üzerine gelince **"Kopyala"** butonu çıkar
- Her endpoint **bağımsız** test edilebilir (sıra gerekmez)
- Formlar **örnek verilerle dolu** gelir
- Request/Response **JSON formatında** gösterilir
- Her test sonrası **süre bilgisi** (ms) görüntülenir

## 📞 Destek

Sorun yaşarsan:
1. `node_modules` ve `.next` klasörlerini sil, tekrar yükle
2. Node.js versiyonunu kontrol et (18+ olmalı)
3. Port 3000'in boş olduğundan emin ol

---

**Not:** Bu panel sadece test amaçlıdır. Production kullanımı için ek güvenlik önlemleri alınmalıdır.

🎉 **Başarılar!**

