'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import { useFlowStore } from '@/lib/store/flow-state';
import { maskToken } from '@/lib/utils';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { tcknSchema, gsmSchema, otpCodeSchema } from '@/lib/validations';

export default function HomePage() {
  const {
    token,
    steps,
    setStepResult,
    tckn, gsm, setTckn, setGsm, setCustomerId,
    kvkkId, setKvkkId,
    otpCode, setOtpCode, setToken: setStoreToken,
    customerId, setCustomerId: setStoredCustomerId,
    reportIds, selectedReportId, setReportIds, setSelectedReportId
  } = useFlowStore();

  const [selectedEndpoint, setSelectedEndpoint] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [copied, setCopied] = useState(false);
  const [copiedToken, setCopiedToken] = useState(false);
  const [copiedEndpointUrl, setCopiedEndpointUrl] = useState<string | null>(null);
  const [curlCommand, setCurlCommand] = useState<string>('');
  const [copiedCurl, setCopiedCurl] = useState(false);

  // Form states
  const [localTckn, setLocalTckn] = useState('12345678901');
  const [localGsm, setLocalGsm] = useState('5551112233');
  const [localKvkkId, setLocalKvkkId] = useState(2);
  const [localOtpCode, setLocalOtpCode] = useState('123456');
  const [localCustomerId, setLocalCustomerId] = useState(1000849);
  const [localReportId, setLocalReportId] = useState('');
  
  // GÜNCELLENEN KISIM: customer-job-profile varsayılan JSON yapısı
  const [localJobProfile, setLocalJobProfile] = useState(JSON.stringify({
    customerId: 1000849,
    customerWork: 5,
    jobGroupId: 3,
    workingYears: 5,
    workingMonth: 2,
    titleCompany: "ABC Teknoloji A.Ş.",
    companyPosition: "Kıdemli Yazılım Geliştirici"
  }, null, 2));

  const [localAddressSave, setLocalAddressSave] = useState(JSON.stringify({
    customerId: 1,
    adress: "test",
    cityId: 34,
    townId: 12,
    source: 2
  }, null, 2));
  const [wifeInfoForm, setWifeInfoForm] = useState({
    maritalStatus: true,
    workWife: true,
    wifeSalaryAmount: 65000
  });
  const [localFinanceAssets, setLocalFinanceAssets] = useState(JSON.stringify({
    customerId: 1,
    workSector: 3,
    salaryBank: "Ziraat",
    salaryAmount: 50000,
    carStatus: true,
    houseStatus: false
  }, null, 2));
  const [kvkkText, setKvkkText] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Clear curl command when endpoint changes
  useEffect(() => {
    setCurlCommand('');
    setError('');
  }, [selectedEndpoint]);

  const endpoints = [
    {
      key: 'tckn-gsm',
      title: 'TCKN & GSM Doğrulama',
      method: 'POST',
      description: 'Müşteri TCKN ve GSM numarası doğrulama. Response\'da customerId üretilir.',
      url: 'https://customers-api.azurewebsites.net/api/customer/tckn-gsm?code=gww5m66SOHBjQ9LY58dM5Gulq2giauLokvvIX4ylR405AzFu7CUbIA=='
    },
    {
      key: 'kvkk-text',
      title: 'KVKK Metni Getir',
      method: 'GET',
      description: 'Servis ID\'ye göre metinsel adlarını ve metinleri getirir',
      url: 'https://api-idc.azurewebsites.net/api/kvkk/text/{id}?code=5OaiAxOi6mmwXV4gPTcKiHc9plIMg3s6Kcer667-4OK1AzFulZ-Mkw=='
    },
    {
      key: 'kvkk-onay',
      title: 'KVKK Onay Kaydet',
      method: 'POST',
      description: 'Müşteri KVKK onayını kaydet (Create_KvkkOnay)',
      url: 'https://api-idc.azurewebsites.net/api/kvkk/onay?code=VYa4cMvucKPZrC9eQ9ZuXKixMPhzb3M4URamT57nMXtkAzFuUL-6og=='
    },
    {
      key: 'generate-otp',
      title: 'OTP Üret',
      method: 'POST',
      description: 'Tek kullanımlık şifre oluştur (GENERATEOTP)',
      url: 'https://api-idc.azurewebsites.net/api/generate-otp?code=NgHo3RJwMJ4rVsPtGLau40m_vykzGV24zBAZYJPbVQpIAzFurGPRTw=='
    },
    {
      key: 'send-otp-sms',
      title: 'OTP SMS Gönder',
      method: 'POST',
      description: 'OTP kodunu SMS ile gönder (SENDOTPSMS)',
      url: 'https://api-idc.azurewebsites.net/api/send-otp-sms?code=fsfjOxq_5dhmOKxp4Q6ffi5ZuxdmGO2cnwGi2IBwRzSjAzFu9FrxaQ=='
    },
    {
      key: 'verify-otp',
      title: 'OTP Doğrula',
      method: 'POST',
      description: 'OTP kodunu doğrula. JWT token üretir ve oturum başlatır (VERIFYOTP)',
      url: 'https://api-idc.azurewebsites.net/api/verify-otp?code=NUqFOooe6OqRE8Nf5Se8Swlt5vUxjdMr3oJZes_-MioAAzFuP2nw2g=='
    },
    {
      key: 'dummy-report-list',
      title: 'Dummy Rapor Listesi',
      method: 'GET',
      description: '3 adet reportId döner (GetDummyReportListFunction)',
      url: 'https://api-idc.azurewebsites.net/api/dummy/report-list?code=09wQ_IdxHgsj4oPYrxSrQMbedNddK56Q60lsZPpS5CckAzFu3c0m1A=='
    },
    {
      key: 'report-detail',
      title: 'Rapor Detayı',
      method: 'GET',
      description: '3 rapordan isteneni döndürür (GetReportDetail)',
      url: 'https://api-idc.azurewebsites.net/api/GetReportDetail?code=ghMl1aeJa-tKFvo9hkAn3Cnzgkbf_sc3ZYg7tWvPLfLhAzFuAm67hQ=='
    },
    {
      key: 'customer-address',
      title: 'Müşteri Adres Bilgisi',
      method: 'GET',
      description: 'Müşteri adres bilgilerini getirir',
      url: 'https://customers-api.azurewebsites.net/api/customer/address/{customerId}?code=NXF0SIFM-2G5ZN0uPQEEmrEmuzN7bIrCU3o36MWvlipqAzFuO4ojGQ=='
    },
    {
      key: 'customer-job-info',
      title: 'İş Bilgisi',
      method: 'GET',
      description: 'Müşteri iş bilgilerini getirir (GetCustJobInformation)',
      url: 'https://customers-api.azurewebsites.net/api/customer/job-info/new/{customerId}?code=RPR5Pwi9E_TDXZb1f27lPNrk7jMF67Et58elqdNsilWNAzFup83XlQ=='
    },
    {
      key: 'customer-job-profile',
      title: 'İş Profili Kaydet/Güncelle',
      method: 'POST',
      description: 'İş profili kaydet/güncelle. Nullable alanlar, gönderilmeyenler aynı kalır (SaveCustomerJobProfileFunction)',
      url: 'https://customers-api.azurewebsites.net/api/customer/job-profile?code=vod9U_H064FUeWaxERAxzC3xzh7SVMtnWhSFfOByA-okAzFumNNJUg=='
    },
    {
      key: 'customer-address-save',
      title: 'Adres Kaydet/Güncelle',
      method: 'POST',
      description: 'Müşteri adres bilgilerini kaydet/güncelle (SaveCustomerAddressFunction)',
      url: 'https://customers-api.azurewebsites.net/api/customer/address?code=HiKhittOb6dIjhw7YqA-W1W51iKtQ6D1UoHmKQl91rYSAzFuOurQTQ=='
    },
    {
      key: 'customer-wife-info',
      title: 'Eş/Gelir/Çalışma Durumu Bilgisi',
      method: 'GET',
      description: 'Müşteri eş bilgisi, gelir ve çalışma durumu bilgilerini getirir',
      url: 'https://customers-api.azurewebsites.net/api/customer/wife-info/{customerId}?code=2b9e1uy3xw7MzQooWUvZwS3WsFjHsaffyE2XDlfCdJmYAzFuPszXkw=='
    },
    {
      key: 'customer-wife-info-save',
      title: 'Eş Bilgisi Kaydet/Güncelle',
      method: 'POST',
      description: 'Müşteri eş bilgilerini kaydet/güncelle (SaveCustomerWifeInformationFunction)',
      url: 'https://customers-api.azurewebsites.net/api/customer/wife-info/{customerId}?code=2b9e1uy3xw7MzQooWUvZwS3WsFjHsaffyE2XDlfCdJmYAzFuPszXkw=='
    },
    {
      key: 'customer-finance-assets',
      title: 'Finansal Durum/Varlık Bilgisi',
      method: 'GET',
      description: 'Müşteri finansal durum ve varlık bilgilerini getirir',
      url: 'https://customers-api.azurewebsites.net/api/customer/finance-assets/{customerId:long}?code=iFcccGkEcm1mhd2TQjNY4tk6cKT90e68wgOpvTU-46RNAzFuf9r8tw=='
    },
    {
      key: 'customer-finance-assets-save',
      title: 'Finansal Durum/Varlık Bilgisi Kaydet',
      method: 'POST',
      description: 'Müşteri finansal durum ve varlık bilgilerini kaydet/güncelle (SaveCustomerFinanceAndAssetsFunction)',
      url: 'https://customers-api.azurewebsites.net/api/customer/finance-assets?code=rp3u-kwXRt5U2bXDf-1sgsIcYwPcbzl1XVniXw510b7SAzFui9yHVw=='
    },
  ];

  const filteredEndpoints = endpoints.filter(ep =>
    ep.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    ep.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
    ep.url.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const currentEndpoint = endpoints.find(e => e.key === selectedEndpoint);
  const stepResult = selectedEndpoint ? steps[selectedEndpoint] : null;

  const getMethodColor = (method: string) => {
    switch (method) {
      case 'GET': return 'bg-blue-100 text-blue-700 border-blue-300';
      case 'POST': return 'bg-green-100 text-green-700 border-green-300';
      default: return 'bg-gray-100 text-gray-700 border-gray-300';
    }
  };

  const generateCurlCommand = (localUrl: string, method: string, requestBody: any) => {
    let fullUrl = currentEndpoint?.url || '';

    // Replace dynamic parameters in URL for both GET and POST
    fullUrl = fullUrl.replace(/\{customerId[^}]*\}/g, String(localCustomerId));
    fullUrl = fullUrl.replace(/\{id[^}]*\}/g, String(localKvkkId));

    // Handle reportId query parameter for report-detail
    if (selectedEndpoint === 'report-detail' && localReportId) {
      if (!fullUrl.includes('reportId=')) {
        fullUrl = fullUrl + `&reportId=${localReportId}`;
      }
    }

    // Build curl command with proper formatting
    let curlParts = [];
    curlParts.push(`curl -X ${method}`);
    curlParts.push(`  '${fullUrl}'`);

    if (method === 'POST' && requestBody) {
      curlParts.push(`  -H 'Content-Type: application/json'`);
      curlParts.push(`  -d '${JSON.stringify(requestBody)}'`);
    }

    return curlParts.join(' \\\n');
  };

  const handleTest = async () => {
    if (!selectedEndpoint) return;

    setError('');
    setIsLoading(true);
    setStepResult(selectedEndpoint, { status: 'loading' });

    try {
      let requestBody: any = null;
      let url = '';
      let method = currentEndpoint?.method || 'GET';

      switch (selectedEndpoint) {
        case 'tckn-gsm':
          const tcknValidation = tcknSchema.safeParse(localTckn);
          const gsmValidation = gsmSchema.safeParse(localGsm);
          if (!tcknValidation.success) {
            setError(tcknValidation.error.errors[0].message);
            setIsLoading(false);
            return;
          }
          if (!gsmValidation.success) {
            setError(gsmValidation.error.errors[0].message);
            setIsLoading(false);
            return;
          }
          requestBody = { TCKN: localTckn, GSM: localGsm };
          url = '/api/ep/tckn-gsm';
          break;

        case 'kvkk-text':
          url = `/api/ep/kvkk-text/${localKvkkId}`;
          method = 'GET';
          break;

        case 'kvkk-onay':
          requestBody = { kvkkId: localKvkkId, customerId: localCustomerId, isOk: true };
          url = '/api/ep/kvkk-onay';
          break;

        case 'generate-otp':
          requestBody = { tckn: localTckn, gsm: localGsm, utmId: "5" };
          url = '/api/ep/generate-otp';
          break;

        case 'send-otp-sms':
          requestBody = { gsm: localGsm, otpCode: localOtpCode };
          url = '/api/ep/send-otp-sms';
          break;

        case 'verify-otp':
          const otpValidation = otpCodeSchema.safeParse(localOtpCode);
          if (!otpValidation.success) {
            setError(otpValidation.error.errors[0].message);
            setIsLoading(false);
            return;
          }
          requestBody = { otpCode: localOtpCode };
          url = '/api/ep/verify-otp';
          break;

        case 'dummy-report-list':
          url = '/api/ep/dummy-report-list';
          method = 'GET';
          break;

        case 'report-detail':
          url = `/api/ep/report-detail?reportId=${localReportId}`;
          method = 'GET';
          break;

        case 'customer-address':
          url = `/api/ep/customer-address/${localCustomerId}`;
          method = 'GET';
          break;

        case 'customer-job-info':
          url = `/api/ep/customer-job-info/${localCustomerId}`;
          method = 'GET';
          break;

        case 'customer-job-profile':
          try {
            requestBody = JSON.parse(localJobProfile);
          } catch {
            setError('Geçersiz JSON formatı');
            setIsLoading(false);
            return;
          }
          url = '/api/ep/customer-job-profile';
          break;

        case 'customer-address-save':
          try {
            requestBody = JSON.parse(localAddressSave);
          } catch {
            setError('Geçersiz JSON formatı');
            setIsLoading(false);
            return;
          }
          url = '/api/ep/customer-address/' + (requestBody.customerId || localCustomerId);
          break;


        case 'customer-wife-info':
          url = `/api/ep/customer-wife-info/${localCustomerId}`;
          method = 'GET';
          break;

        case 'customer-wife-info-save':
          // JSON parse yerine form state'inden veriyi alıyoruz
          requestBody = {
            customerId: localCustomerId, // Mevcut global customerId state'i
            maritalStatus: wifeInfoForm.maritalStatus,
            workWife: wifeInfoForm.workWife,
            wifeSalaryAmount: Number(wifeInfoForm.wifeSalaryAmount)
          };
          // Not: endpoint URL'i route.ts'deki yapıya uygun olmalı (GET ile aynı path ise)
          // Eğer route.ts'de path farklıysa '/api/ep/customer-wife-info' olarak kalsın
          url = '/api/ep/customer-wife-info';
          break;

        case 'customer-finance-assets':
          url = `/api/ep/customer-finance-assets/${localCustomerId}`;
          method = 'GET';
          break;

        case 'customer-finance-assets-save':
          try {
            requestBody = JSON.parse(localFinanceAssets);
          } catch {
            setError('Geçersiz JSON formatı');
            setIsLoading(false);
            return;
          }
          url = '/api/ep/customer-finance-assets';
          break;

        default:
          setError('Bilinmeyen endpoint');
          setIsLoading(false);
          return;
      }

      // Generate cURL command
      const curl = generateCurlCommand(url, method, requestBody);
      setCurlCommand(curl);

      const fetchOptions: RequestInit = {
        method,
        headers: method === 'POST' ? { 'Content-Type': 'application/json' } : {},
      };

      if (method === 'POST' && requestBody) {
        fetchOptions.body = JSON.stringify(requestBody);
      }

      const response = await fetch(url, fetchOptions);
      const result = await response.json();

      if (result.error) {
        setStepResult(selectedEndpoint, {
          status: 'error',
          request: requestBody,
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
        setError(result.error);
      } else {
        // Handle special cases
        if (selectedEndpoint === 'kvkk-text' && result.data) {
          const dataObj = result.data as Record<string, unknown>;
          if (typeof dataObj.text === 'string') setKvkkText(dataObj.text);
          else if (typeof dataObj.content === 'string') setKvkkText(dataObj.content);
        }

        if (selectedEndpoint === 'verify-otp' && result.data) {
          const dataObj = result.data as Record<string, unknown>;
          const token =
            (typeof dataObj.token === 'string' ? dataObj.token : undefined) ||
            (typeof dataObj.access_token === 'string' ? dataObj.access_token : undefined) ||
            (typeof dataObj.jwt === 'string' ? dataObj.jwt : undefined);
          if (token) setStoreToken(token);
        }

        if (selectedEndpoint === 'dummy-report-list' && Array.isArray(result.data)) {
          const ids = result.data.map((item: any) => String(item.id || item.reportId || item));
          setReportIds(ids.filter(Boolean));
        }

        setStepResult(selectedEndpoint, {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult(selectedEndpoint, {
        status: 'error',
        error: errorMsg,
      });
      setError(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  const renderForm = () => {
    if (!selectedEndpoint) return null;

    switch (selectedEndpoint) {
      case 'tckn-gsm':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                TCKN <span className="text-gray-400 font-normal">(11 hane)</span>
              </label>
              <input
                type="text"
                value={localTckn}
                onChange={(e) => setLocalTckn(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                placeholder="12345678901"
                maxLength={11}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                GSM <span className="text-gray-400 font-normal">(10-11 hane)</span>
              </label>
              <input
                type="text"
                value={localGsm}
                onChange={(e) => setLocalGsm(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                placeholder="5551234567"
                maxLength={11}
              />
            </div>
          </div>
        );

      case 'kvkk-text':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">KVKK ID</label>
              <input
                type="number"
                value={localKvkkId}
                onChange={(e) => setLocalKvkkId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
            {kvkkText && (
              <div className="bg-gray-50 border border-gray-200 rounded-md p-3 max-h-32 overflow-y-auto">
                <div className="text-xs text-gray-800 whitespace-pre-wrap">{kvkkText}</div>
              </div>
            )}
          </div>
        );

      case 'kvkk-onay':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">KVKK ID</label>
              <input
                type="number"
                value={localKvkkId}
                onChange={(e) => setLocalKvkkId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Customer ID</label>
              <input
                type="number"
                value={localCustomerId}
                onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
            <div className="bg-blue-50 border border-blue-200 rounded-md p-3">
              <p className="text-xs text-blue-900">
                isOk: true olarak gönderilecek
              </p>
            </div>
          </div>
        );

      case 'generate-otp':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">TCKN</label>
              <input
                type="text"
                value={localTckn}
                onChange={(e) => setLocalTckn(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                maxLength={11}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">GSM</label>
              <input
                type="text"
                value={localGsm}
                onChange={(e) => setLocalGsm(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                maxLength={11}
              />
            </div>
            <div className="bg-gray-50 border border-gray-200 rounded-md p-2">
              <p className="text-xs text-gray-600">
                utmId: "5" (default)
              </p>
            </div>
          </div>
        );

      case 'send-otp-sms':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">GSM</label>
              <input
                type="text"
                value={localGsm}
                onChange={(e) => setLocalGsm(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                maxLength={11}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">OTP Code</label>
              <input
                type="text"
                value={localOtpCode}
                onChange={(e) => setLocalOtpCode(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                placeholder="123456"
                maxLength={6}
              />
            </div>
          </div>
        );

      case 'verify-otp':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                OTP Kodu <span className="text-gray-400 font-normal">(6 hane)</span>
              </label>
              <input
                type="text"
                value={localOtpCode}
                onChange={(e) => setLocalOtpCode(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                placeholder="123456"
                maxLength={6}
              />
            </div>
          </div>
        );

      case 'report-detail':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Report ID</label>
              <input
                type="text"
                value={localReportId}
                onChange={(e) => setLocalReportId(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
                placeholder="Report ID girin"
              />
            </div>
          </div>
        );

      case 'customer-address':
      case 'customer-job-info':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Customer ID</label>
              <input
                type="number"
                value={localCustomerId}
                onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
          </div>
        );

      case 'customer-job-profile':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                JSON Data
              </label>
              <textarea
                value={localJobProfile}
                onChange={(e) => setLocalJobProfile(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent font-mono text-xs"
                rows={8}
              />
            </div>
          </div>
        );

      case 'customer-address-save':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                JSON Data
              </label>
              <textarea
                value={localAddressSave}
                onChange={(e) => setLocalAddressSave(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent font-mono text-xs"
                rows={6}
              />
            </div>
            <div className="bg-blue-50 border border-blue-200 rounded-md p-2">
              <p className="text-xs text-blue-900">
                source: 2 = kişi ekler, 1 = danışmanlık ekler
              </p>
            </div>
          </div>
        );

      case 'customer-wife-info':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Customer ID</label>
              <input
                type="number"
                value={localCustomerId}
                onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
          </div>
        );

      case 'customer-wife-info-save':
        return (
          <div className="space-y-3">
            {/* Customer ID */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Customer ID</label>
              <input
                type="number"
                value={localCustomerId}
                onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>

            {/* Medeni Durum */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Medeni Durum</label>
              <select
                value={wifeInfoForm.maritalStatus ? "true" : "false"}
                onChange={(e) => setWifeInfoForm(prev => ({ ...prev, maritalStatus: e.target.value === "true" }))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              >
                <option value="true">Evli</option>
                <option value="false">Bekar</option>
              </select>
            </div>

            {/* Eş Çalışma Durumu */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Eş Çalışma Durumu</label>
              <select
                value={wifeInfoForm.workWife ? "true" : "false"}
                onChange={(e) => setWifeInfoForm(prev => ({ ...prev, workWife: e.target.value === "true" }))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              >
                <option value="true">Çalışıyor</option>
                <option value="false">Çalışmıyor</option>
              </select>
            </div>

            {/* Eş Gelir */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Eş Maaş Tutarı</label>
              <input
                type="number"
                value={wifeInfoForm.wifeSalaryAmount}
                onChange={(e) => setWifeInfoForm(prev => ({ ...prev, wifeSalaryAmount: Number(e.target.value) }))}
                disabled={!wifeInfoForm.workWife} // Eş çalışmıyorsa girişi engelle
                className={`w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent ${!wifeInfoForm.workWife ? 'bg-gray-100 text-gray-400' : ''}`}
              />
            </div>

            {/* Bilgi Kutusu */}
            <div className="bg-blue-50 border border-blue-200 rounded-md p-2 mt-2">
              <p className="text-xs text-blue-900">
                Gönderilecek JSON:<br />
                {JSON.stringify({
                  customerId: localCustomerId,
                  maritalStatus: wifeInfoForm.maritalStatus,
                  workWife: wifeInfoForm.workWife,
                  wifeSalaryAmount: Number(wifeInfoForm.wifeSalaryAmount)
                }, null, 2)}
              </p>
            </div>
          </div>
        );

      case 'customer-finance-assets':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Customer ID</label>
              <input
                type="number"
                value={localCustomerId}
                onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent"
              />
            </div>
          </div>
        );

      case 'customer-finance-assets-save':
        return (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                JSON Data
              </label>
              <textarea
                value={localFinanceAssets}
                onChange={(e) => setLocalFinanceAssets(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-gray-800 focus:border-transparent font-mono text-xs"
                rows={8}
              />
            </div>
          </div>
        );

      case 'dummy-report-list':
        return <div className="text-gray-500 text-xs">Bu endpoint için parametre gerekmemektedir.</div>;

      default:
        return <div className="text-gray-500 text-xs">Bu endpoint için parametre gerekmemektedir.</div>;
    }
  };

  const truncatePath = (url: string, maxLength: number = 60) => {
    if (url.length <= maxLength) return url;
    const start = url.substring(0, 30);
    const end = url.substring(url.length - 30);
    return `${start}...${end}`;
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sol Panel */}
      <div className="w-96 bg-white flex flex-col border-r border-gray-200">
        <div className="p-6 border-b border-gray-200 flex items-center justify-center">
          <Image
            src="https://interaktifkredi.com.tr/images/InteraktifKrediNewLogo.png"
            alt="İnteraktif Kredi"
            width={240}
            height={64}
            className="h-16 w-auto object-contain"
            priority
          />
        </div>

        {token && (
          <div className="px-6 py-3 bg-gray-50 border-b border-gray-200">
            <div className="flex items-center justify-between mb-1">
              <div className="text-xs text-gray-500">Bearer Token</div>
              <button
                onClick={() => {
                  navigator.clipboard.writeText(token);
                  setCopiedToken(true);
                  setTimeout(() => setCopiedToken(false), 2000);
                }}
                className={`px-2 py-1 rounded text-xs font-medium transition-all ${copiedToken
                  ? 'bg-green-600 text-white'
                  : 'bg-gray-800 hover:bg-black text-white'
                  }`}
                title="Token'ı kopyala"
              >
                {copiedToken ? '✓ Kopyalandı' : 'Kopyala'}
              </button>
            </div>
            <code className="text-xs text-gray-800 font-mono break-all">
              {maskToken(token)}
            </code>
          </div>
        )}

        <div className="p-4 border-b border-gray-200">
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Endpoint ara..."
            className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-gray-800 focus:border-transparent"
          />
        </div>

        <div className="flex-1 overflow-y-auto">
          {filteredEndpoints.map((endpoint) => {
            const isActive = selectedEndpoint === endpoint.key;
            const stepData = steps[endpoint.key];
            const hasData = stepData?.status === 'success' || stepData?.status === 'error';

            return (
              <button
                key={endpoint.key}
                onClick={() => setSelectedEndpoint(endpoint.key)}
                className={`w-full text-left px-6 py-4 border-b border-gray-100 transition-all ${isActive ? 'bg-gray-50 border-l-4 border-l-gray-900' : 'hover:bg-gray-50'
                  }`}
              >
                <div className="flex items-start space-x-3">
                  <span className={`px-2 py-1 text-xs font-bold rounded border flex-shrink-0 ${getMethodColor(endpoint.method)}`}>
                    {endpoint.method}
                  </span>
                  <div className="flex-1 min-w-0">
                    <div className="font-semibold text-gray-900 text-sm mb-1 flex items-center space-x-2">
                      <span>{endpoint.title}</span>
                      {hasData && (
                        <span className={`w-2 h-2 rounded-full flex-shrink-0 ${stepData.status === 'success' ? 'bg-green-500' : 'bg-red-500'
                          }`} />
                      )}
                    </div>
                    <div className="text-xs text-gray-500 mb-1.5">{endpoint.description}</div>
                    <div className="flex items-center space-x-2">
                      <code className="text-xs text-gray-400 font-mono leading-relaxed overflow-hidden text-ellipsis whitespace-nowrap block flex-1">
                        {truncatePath(endpoint.url)}
                      </code>
                      <div
                        onClick={(e) => {
                          e.stopPropagation();
                          navigator.clipboard.writeText(endpoint.url);
                          setCopiedEndpointUrl(endpoint.key);
                          setTimeout(() => setCopiedEndpointUrl(null), 2000);
                        }}
                        className={`flex-shrink-0 px-2 py-0.5 rounded text-xs font-medium transition-all cursor-pointer ${copiedEndpointUrl === endpoint.key
                          ? 'bg-green-600 text-white'
                          : 'bg-gray-700 hover:bg-gray-900 text-white'
                          }`}
                        title="URL'i kopyala"
                      >
                        {copiedEndpointUrl === endpoint.key ? '✓' : 'Kopyala'}
                      </div>
                    </div>
                  </div>
                </div>
              </button>
            );
          })}
        </div>

        <div className="p-4 border-t border-gray-200 bg-gray-50">
          <div className="text-xs text-gray-600">
            <div className="flex justify-between">
              <span>Toplam Endpoint:</span>
              <span className="font-semibold">{endpoints.length}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Sağ Panel */}
      <div className="flex-1 flex flex-col bg-white">
        {selectedEndpoint && currentEndpoint ? (
          <>
            <div className="border-b border-gray-200 bg-white px-6 py-4">
              <div className="flex items-center space-x-2.5 mb-2">
                <span className={`px-2.5 py-0.5 text-xs font-bold rounded border ${getMethodColor(currentEndpoint.method)}`}>
                  {currentEndpoint.method}
                </span>
                <h2 className="text-xl font-bold text-gray-900">{currentEndpoint.title}</h2>
              </div>
              <p className="text-sm text-gray-600 mb-2.5">{currentEndpoint.description}</p>
              <div className="bg-gray-50 rounded-md p-2.5 border border-gray-200 flex items-center justify-between">
                <code className="text-xs font-mono text-gray-800 flex-1 mr-2 break-all">{currentEndpoint.url}</code>
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(currentEndpoint.url);
                    setCopied(true);
                    setTimeout(() => setCopied(false), 2000);
                  }}
                  className={`flex-shrink-0 px-2.5 py-1.5 rounded text-xs font-medium transition-all ${copied
                    ? 'bg-green-600 text-white'
                    : 'bg-gray-800 hover:bg-black text-white'
                    }`}
                  title="URL'i kopyala"
                >
                  {copied ? '✓ Kopyalandı' : 'Kopyala'}
                </button>
              </div>
            </div>

            <div className="flex-1 overflow-hidden p-6">
              <div className="grid grid-cols-2 gap-6 h-full">
                {/* Sol Kolon - Form */}
                <div className="flex flex-col overflow-y-auto pr-2">
                  <h3 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wider">Parametreler</h3>

                  <div className="mb-4">
                    {renderForm()}
                  </div>

                  {error && (
                    <div className="mb-4 bg-red-50 border-l-4 border-red-500 rounded-md p-3 text-xs text-red-700">
                      {error}
                    </div>
                  )}

                  <button
                    onClick={handleTest}
                    disabled={isLoading}
                    className="w-full bg-blue-600 text-white py-2.5 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors text-sm font-semibold shadow-sm"
                  >
                    {isLoading ? 'Test Ediliyor...' : 'Test Et'}
                  </button>

                  {curlCommand && (
                    <div className="mt-4 border border-gray-200 rounded-lg overflow-hidden shadow-sm">
                      <div className="bg-gradient-to-r from-gray-50 to-gray-100 border-b border-gray-200 px-4 py-2.5 flex items-center justify-between">
                        <div className="flex items-center space-x-2">
                          <svg className="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 9l3 3-3 3m5 0h3M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                          <span className="text-xs font-bold text-gray-700 uppercase tracking-wide">cURL Komutu</span>
                        </div>
                        <button
                          onClick={() => {
                            navigator.clipboard.writeText(curlCommand);
                            setCopiedCurl(true);
                            setTimeout(() => setCopiedCurl(false), 2000);
                          }}
                          className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-all ${copiedCurl
                            ? 'bg-green-600 text-white'
                            : 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-300 shadow-sm'
                            }`}
                          title="cURL komutunu kopyala"
                        >
                          {copiedCurl ? (
                            <>
                              <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                              </svg>
                              <span>Kopyalandı!</span>
                            </>
                          ) : (
                            <>
                              <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                <path d="M8 3a1 1 0 011-1h2a1 1 0 110 2H9a1 1 0 01-1-1z" />
                                <path d="M6 3a2 2 0 00-2 2v11a2 2 0 002 2h8a2 2 0 002-2V5a2 2 0 00-2-2 3 3 0 01-3 3H9a3 3 0 01-3-3z" />
                              </svg>
                              <span>Kopyala</span>
                            </>
                          )}
                        </button>
                      </div>
                      <div className="bg-gray-900 p-4">
                        <pre className="text-gray-100 overflow-x-auto text-xs font-mono leading-relaxed">
                          <code className="language-bash">{curlCommand}</code>
                        </pre>
                      </div>
                    </div>
                  )}
                </div>

                {/* Sağ Kolon - Sonuç */}
                <div className="flex flex-col border-l border-gray-200 pl-6 overflow-y-auto">
                  <h3 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wider">Sonuç</h3>
                  <div>
                    <RequestResponsePanel
                      request={stepResult?.request}
                      response={stepResult?.response}
                      error={stepResult?.error}
                      elapsed={stepResult?.elapsed}
                      status={stepResult?.status || 'idle'}
                    />
                  </div>
                </div>
              </div>
            </div>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center">
            <div className="text-center max-w-md">
              <h3 className="text-2xl font-bold text-gray-900 mb-3">
                İnteraktif Kredi API Dokümantasyonu
              </h3>
              <p className="text-gray-600 mb-6">
                Sol menüden bir endpoint seçerek test edin
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}