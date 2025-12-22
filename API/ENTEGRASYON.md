# Next.js API - figjam2 Entegrasyonu

## Kurulum

### 1. Next.js API Projesini Başlat

```bash
cd C:\Users\Nihan\Documents\API
npm install
npm run dev
```

Next.js API `http://localhost:3000` adresinde çalışacak.

### 2. figjam2 Projesini Başlat

```bash
cd C:\Users\Nihan\Documents\figjam2
dotnet run
```

figjam2 projesi `http://localhost:5229` adresinde çalışacak.

## Entegrasyon Detayları

### API Gateway Yapısı

figjam2 projesi artık doğrudan Azure API'lerine değil, Next.js API Gateway'e bağlanıyor:

```
figjam2 (localhost:5229) 
    ↓
Next.js API Gateway (localhost:3000/api/ep)
    ↓
Azure API'leri (customers-api.azurewebsites.net, api-idc.azurewebsites.net)
```

### Endpoint Mapping

| figjam2 Endpoint | Next.js API Gateway | Azure API |
|-----------------|---------------------|-----------|
| `ApiConfig.TCKN_GSM` | `/api/ep/tckn-gsm` | `customers-api.azurewebsites.net/api/customer/tckn-gsm` |
| `ApiConfig.KVKK_TEXT` | `/api/ep/kvkk-text/{id}` | `api-idc.azurewebsites.net/api/kvkk/text/{id}` |
| `ApiConfig.KVKK_ONAY` | `/api/ep/kvkk-onay` | `api-idc.azurewebsites.net/api/kvkk/onay` |
| `ApiConfig.GENERATE_OTP` | `/api/ep/generate-otp` | `api-idc.azurewebsites.net/api/generate-otp` |
| `ApiConfig.SEND_OTP_SMS` | `/api/ep/send-otp-sms` | `api-idc.azurewebsites.net/api/send-otp-sms` |
| `ApiConfig.VERIFY_OTP` | `/api/ep/verify-otp` | `api-idc.azurewebsites.net/api/verify-otp` |

### Token Yönetimi

- figjam2, token'ı `Authorization: Bearer {token}` header'ı ile Next.js API'ye gönderir
- Next.js API, token'ı header'dan alır veya cookie'den okur
- Token yoksa default token kullanılır: `fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s`

### CORS Ayarları

Next.js API Gateway'de CORS middleware eklendi:
- `Access-Control-Allow-Origin: *`
- `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
- `Access-Control-Allow-Headers: Content-Type, Authorization, X-Requested-With`

### Response Formatı

Next.js API Gateway şu formatta response döndürür:

```json
{
  "status": 200,
  "data": { ... },
  "elapsed": 123,
  "error": null
}
```

figjam2'deki `ApiClient` bu formatı otomatik olarak parse eder.

## Test

1. Next.js API'yi başlat: `cd API && npm run dev`
2. figjam2'yi başlat: `cd figjam2 && dotnet run`
3. Login sayfasına git: `http://localhost:5229/Login`
4. TCKN ve telefon numarası gir, KVKK onayla, SMS kodu gönder

## Sorun Giderme

### Next.js API çalışmıyor

- Port 3000'in kullanılabilir olduğundan emin olun
- `npm install` komutunu çalıştırın
- `npm run dev` ile başlatın

### figjam2 Next.js API'ye bağlanamıyor

- Next.js API'nin `http://localhost:3000` adresinde çalıştığından emin olun
- `ApiConfig.cs` dosyasındaki `NEXTJS_API_BASE` değerini kontrol edin
- CORS ayarlarının doğru olduğundan emin olun

### Token sorunları

- Token'ın `Authorization: Bearer {token}` header'ı ile gönderildiğinden emin olun
- Next.js API loglarını kontrol edin
- figjam2 session'ında token'ın kaydedildiğinden emin olun

