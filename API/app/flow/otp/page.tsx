'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { useFlowStore } from '@/lib/store/flow-state';
import { otpCodeSchema } from '@/lib/validations';

export default function OtpPage() {
  const router = useRouter();
  const { gsm, otpCode, setOtpCode, setToken, setStepResult, steps, isStepCompleted } = useFlowStore();
  const [localOtpCode, setLocalOtpCode] = useState(otpCode || '123456');
  const [isLoadingGenerate, setIsLoadingGenerate] = useState(false);
  const [isLoadingSend, setIsLoadingSend] = useState(false);
  const [isLoadingVerify, setIsLoadingVerify] = useState(false);
  const [error, setError] = useState('');
  const [activeStep, setActiveStep] = useState<'generate' | 'send' | 'verify'>('generate');

  const stepResultGenerate = steps['generate-otp'];
  const stepResultSend = steps['send-otp-sms'];
  const stepResultVerify = steps['verify-otp'];
  const isCompleted = stepResultVerify?.status === 'success';
  const isLocked = false; // Artık bağımlılık yok

  const handleGenerateOtp = async () => {
    setIsLoadingGenerate(true);
    setStepResult('generate-otp', { status: 'loading' });

    try {
      const requestBody = { gsm, purpose: 'verification' };
      const response = await fetch('/api/ep/generate-otp', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();

      if (result.error) {
        setStepResult('generate-otp', {
          status: 'error',
          request: requestBody,
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('generate-otp', {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('generate-otp', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingGenerate(false);
    }
  };

  const handleSendSms = async () => {
    setIsLoadingSend(true);
    setStepResult('send-otp-sms', { status: 'loading' });

    try {
      const requestBody = { gsm, messageType: 'otp' };
      const response = await fetch('/api/ep/send-otp-sms', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();

      if (result.error) {
        setStepResult('send-otp-sms', {
          status: 'error',
          request: requestBody,
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('send-otp-sms', {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('send-otp-sms', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingSend(false);
    }
  };

  const handleVerifyOtp = async () => {
    setError('');
    
    const validation = otpCodeSchema.safeParse(localOtpCode);
    if (!validation.success) {
      setError(validation.error.errors[0].message);
      return;
    }

    setIsLoadingVerify(true);
    setStepResult('verify-otp', { status: 'loading' });

    try {
      const requestBody = { gsm, otpCode: localOtpCode };
      const response = await fetch('/api/ep/verify-otp', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();

      if (result.error) {
        setStepResult('verify-otp', {
          status: 'error',
          request: requestBody,
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
        setError(result.error);
      } else {
        if (result.data && typeof result.data === 'object') {
          const dataObj = result.data as Record<string, unknown>;
          const token = 
            (typeof dataObj.token === 'string' ? dataObj.token : undefined) ||
            (typeof dataObj.access_token === 'string' ? dataObj.access_token : undefined) ||
            (typeof dataObj.jwt === 'string' ? dataObj.jwt : undefined) ||
            (typeof dataObj.accessToken === 'string' ? dataObj.accessToken : undefined);
          
          if (token) {
            setToken(token);
          }
        }

        setOtpCode(localOtpCode);
        setStepResult('verify-otp', {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('verify-otp', {
        status: 'error',
        error: errorMsg,
      });
      setError(errorMsg);
    } finally {
      setIsLoadingVerify(false);
    }
  };

  if (isLocked) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-100">
        <div className="text-center">
          <div className="text-6xl mb-4">🔒</div>
          <h2 className="text-2xl font-bold text-gray-800 mb-2">Bu Adım Kilitli</h2>
          <p className="text-gray-600 mb-6">Önceki adımları tamamlayın</p>
          <button
            onClick={() => router.push('/')}
            className="px-6 py-3 bg-gray-800 text-white rounded-lg hover:bg-gray-900 transition-colors"
          >
            Ana Sayfaya Dön
          </button>
        </div>
      </div>
    );
  }

  const currentStepResult = 
    activeStep === 'generate' ? stepResultGenerate :
    activeStep === 'send' ? stepResultSend :
    stepResultVerify;

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sol Panel */}
      <div className="w-[500px] bg-white flex flex-col border-r border-gray-200">
        <div className="p-6 border-b border-gray-200">
          <button
            onClick={() => router.push('/')}
            className="text-gray-600 hover:text-gray-900 flex items-center space-x-2 transition-colors mb-4"
          >
            <span className="text-xl">←</span>
            <span className="font-medium">Ana Sayfa</span>
          </button>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">OTP İşlemleri</h1>
            <div className="text-sm text-gray-500 mt-1">GSM: {gsm}</div>
          </div>
        </div>

        <div className="flex border-b border-gray-200 text-xs">
          <button
            onClick={() => setActiveStep('generate')}
            className={`flex-1 px-3 py-2 font-semibold transition-colors ${
              activeStep === 'generate' ? 'text-gray-900 border-b-2 border-gray-900' : 'text-gray-500'
            }`}
          >
            1. Üret
          </button>
          <button
            onClick={() => setActiveStep('send')}
            className={`flex-1 px-3 py-2 font-semibold transition-colors ${
              activeStep === 'send' ? 'text-gray-900 border-b-2 border-gray-900' : 'text-gray-500'
            }`}
          >
            2. Gönder
          </button>
          <button
            onClick={() => setActiveStep('verify')}
            className={`flex-1 px-3 py-2 font-semibold transition-colors ${
              activeStep === 'verify' ? 'text-gray-900 border-b-2 border-gray-900' : 'text-gray-500'
            }`}
          >
            3. Doğrula
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6 space-y-5">
          {activeStep === 'generate' && (
            <button
              onClick={handleGenerateOtp}
              disabled={isLoadingGenerate}
              className="w-full bg-gray-800 text-white py-3 px-4 rounded-lg hover:bg-gray-900 disabled:bg-gray-400 transition-colors font-semibold"
            >
              {isLoadingGenerate ? 'Üretiliyor...' : 'OTP Üret'}
            </button>
          )}

          {activeStep === 'send' && (
            <button
              onClick={handleSendSms}
              disabled={isLoadingSend || stepResultGenerate?.status !== 'success'}
              className="w-full bg-gray-800 text-white py-3 px-4 rounded-lg hover:bg-gray-900 disabled:bg-gray-400 transition-colors font-semibold"
            >
              {isLoadingSend ? 'Gönderiliyor...' : 'SMS Gönder'}
            </button>
          )}

          {activeStep === 'verify' && (
            <>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  OTP Kodu <span className="text-gray-400 font-normal">(6 hane)</span>
                </label>
                <input
                  type="text"
                  value={localOtpCode}
                  onChange={(e) => setLocalOtpCode(e.target.value)}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gray-800 transition-all"
                  placeholder="123456"
                  maxLength={6}
                  disabled={isLoadingVerify}
                />
              </div>

              {error && (
                <div className="bg-red-50 border-l-4 border-red-500 rounded-lg p-4 text-sm text-red-700">
                  {error}
                </div>
              )}

              <button
                onClick={handleVerifyOtp}
                disabled={isLoadingVerify || stepResultSend?.status !== 'success'}
                className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 disabled:bg-gray-400 transition-colors font-semibold"
              >
                {isLoadingVerify ? 'Doğrulanıyor...' : 'OTP Doğrula'}
              </button>
            </>
          )}

          {isCompleted && (
            <button
              onClick={() => router.push('/flow/reports')}
              className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 transition-colors font-semibold"
            >
              Sonraki Adım: Raporlar →
            </button>
          )}
        </div>
      </div>

      {/* Sağ Panel */}
      <div className="flex-1 flex flex-col bg-gray-50">
        <div className="border-b border-gray-200 bg-white px-8 py-6">
          <h2 className="text-xl font-bold text-gray-800">
            {activeStep === 'generate' ? 'OTP Üret - Sonuç' :
             activeStep === 'send' ? 'SMS Gönder - Sonuç' :
             'OTP Doğrula - Sonuç'}
          </h2>
        </div>
        <div className="flex-1 overflow-y-auto p-8">
          <RequestResponsePanel
            request={currentStepResult?.request}
            response={currentStepResult?.response}
            error={currentStepResult?.error}
            elapsed={currentStepResult?.elapsed}
            status={currentStepResult?.status || 'idle'}
          />
        </div>
      </div>
    </div>
  );
}
