# İnteraktif Kredi Panel

Next.js 14 (App Router) ile oluşturulmuş İnteraktif Kredi API entegrasyon demo paneli.

## 🎯 Özellikler

- ✅ 12 farklı endpoint entegrasyonu
- ✅ Server-side API proxy katmanı (CORS ve güvenlik için)
- ✅ Token yönetimi (httpOnly cookie)
- ✅ Adım adım flow yönetimi (state machine)
- ✅ Request/Response görüntüleme
- ✅ Hata yönetimi ve kullanıcı bildirimleri
- ✅ Timing bilgisi (ms)
- ✅ TypeScript + Zod validasyonu
- ✅ Tailwind CSS ile modern UI
- ✅ Zustand ile state management

## 📋 Gereksinimler

- Node.js 18+
- pnpm (veya npm/yarn)

## 🚀 Kurulum

```bash
# Bağımlılıkları yükle
pnpm install

# Geliştirme sunucusunu başlat
pnpm dev
```

Tarayıcınızda [http://localhost:3000](http://localhost:3000) adresini açın.

## 📁 Proje Yapısı

```
├── app/
│   ├── api/ep/                  # API route handlers (12 endpoint)
│   │   ├── tckn-gsm/
│   │   ├── kvkk-text/[id]/
│   │   ├── kvkk-onay/
│   │   ├── generate-otp/
│   │   ├── send-otp-sms/
│   │   ├── verify-otp/
│   │   ├── dummy-report-list/
│   │   ├── report-detail/
│   │   ├── customer-address/[customerId]/
│   │   ├── customer-salary/[customerId]/[dealerId]/
│   │   ├── customer-job-info/[customerId]/
│   │   └── customer-job-profile/
│   ├── flow/                    # Flow sayfaları
│   │   ├── customer/
│   │   ├── kvkk/
│   │   ├── otp/
│   │   ├── reports/
│   │   └── profile/
│   ├── layout.tsx
│   ├── page.tsx                 # Ana dashboard
│   └── globals.css
├── components/                  # UI bileşenleri
│   ├── step-card.tsx
│   ├── json-viewer.tsx
│   └── request-response-panel.tsx
├── lib/
│   ├── ep/
│   │   └── ep-client.ts        # External API client
│   ├── session/
│   │   └── token.ts            # Token yönetimi (httpOnly cookie)
│   ├── store/
│   │   └── flow-state.ts       # Zustand store
│   └── validations.ts          # Zod schemas
└── README.md
```

## 🔄 Flow Adımları

1. **TCKN & GSM Doğrulama** (POST)
2. **KVKK Metni Getir** (GET)
3. **KVKK Onay Kaydet** (POST)
4. **OTP Üret** (POST)
5. **OTP SMS Gönder** (POST)
6. **OTP Doğrula** (POST) - JWT token üretir
7. **Dummy Rapor Listesi** (GET) - Token gerektirir
8. **Rapor Detayı** (GET) - Token gerektirir
9. **Müşteri Adres** (GET) - Token gerektirir
10. **Maaş Bilgisi** (GET) - Token gerektirir
11. **İş Bilgisi** (GET) - Token gerektirir
12. **İş Profili Kaydet** (POST) - Token gerektirir

## 🔐 Token Yönetimi

- **Default Token**: Tüm endpoint'lerde otomatik olarak `fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s` tokeni kullanılır
- Token, `lib/config.ts` dosyasında `DEFAULT_TOKEN` olarak tanımlıdır
- OTP doğrulama başarılı olduğunda alınan token, default token'ın üzerine yazılır
- Tüm endpoint'ler otomatik olarak Bearer token ile çağrılır
- Token UI'da maskeli olarak gösterilir (güvenlik için)
- Token httpOnly cookie olarak server-side saklanır

## 🎨 UI/UX Özellikleri

- **Dashboard**: Tüm adımların genel görünümü ve ilerleme çubuğu
- **Step Cards**: Her adım için ayrı card görünümü
- **Lock/Unlock**: Önceki adım tamamlanmadan sonraki adım açılmaz
- **Request/Response Viewer**: JSON pretty-print ile gösterim
- **Error Handling**: Kullanıcı dostu hata mesajları
- **Loading States**: Her işlem için loading göstergesi
- **Timing Info**: Her request için süre bilgisi (ms)

## 🛠️ Teknolojiler

- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript (Strict mode)
- **Styling**: Tailwind CSS
- **State Management**: Zustand (with persist)
- **Validation**: Zod
- **API Client**: Fetch API (server-side)

## 🔧 Yapılandırma

### Environment Variables

Bu projede environment variable gerekmez. Tüm endpoint URL'leri kod içinde sabit olarak tanımlanmıştır.

### Default Değerler

- **Bearer Token**: `fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s` (tüm endpoint'lerde kullanılır)
- **KVKK ID**: 2 (değiştirilebilir)
- **Dealer ID**: 1081 (değiştirilebilir)
- **Customer ID**: TCKN-GSM doğrulama sonucundan alınır veya manuel girilir

**Not**: Token'i değiştirmek için `lib/config.ts` dosyasındaki `DEFAULT_TOKEN` değerini güncelleyin.

## 📝 Kullanım

1. Ana sayfada flow adımlarını göreceksiniz
2. **Tüm endpoint'ler otomatik olarak Bearer token ile çağrılır** (`fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s`)
3. İlk adımdan başlayarak sırayla ilerleyin:
   - Gerekli alanları doldurun
   - "Çalıştır" butonuna tıklayın
   - Request ve Response'u inceleyin
   - Başarılı olursa sonraki adıma geçin
4. OTP doğrulama başarılıysa, yeni token eski token'ın üzerine yazılır
5. Tüm adımları sırayla tamamlayın

### Token Değiştirme

Token'i değiştirmek için `lib/config.ts` dosyasını düzenleyin:

```typescript
export const API_CONFIG = {
  DEFAULT_TOKEN: 'BURAYA_YENİ_TOKEN',
  USE_DEFAULT_TOKEN: true,
} as const;
```

## 🐛 Hata Ayıklama

### Server Console

Tüm API çağrıları server console'da loglanır:
- Endpoint URL
- HTTP Status
- Elapsed time (ms)

### Client State

Zustand dev tools ile client-side state incelenebilir. State localStorage'da persist edilir.

### Response Viewer

Her adımın response'u UI'da JSON formatında gösterilir. Hata durumlarında detaylı hata mesajları görüntülenir.

## 🚦 Build & Deploy

```bash
# Production build
pnpm build

# Production sunucusu
pnpm start
```

## 📄 Lisans

Bu proje demo amaçlı oluşturulmuştur.

## 🤝 Katkıda Bulunma

Bu bir demo projedir ve aktif geliştirme beklenmemektedir.

## 📞 İletişim

Sorularınız için issue açabilirsiniz.

---

**Not**: Bu panel sadece test amaçlıdır. Production kullanımı için ek güvenlik önlemleri alınmalıdır.

