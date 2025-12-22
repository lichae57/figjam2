'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { useFlowStore } from '@/lib/store/flow-state';
import { tcknSchema, gsmSchema } from '@/lib/validations';

export default function CustomerPage() {
  const router = useRouter();
  const { tckn, gsm, setTckn, setGsm, setCustomerId, setStepResult, steps } = useFlowStore();
  const [localTckn, setLocalTckn] = useState(tckn || '12345678901');
  const [localGsm, setLocalGsm] = useState(gsm || '5551112233');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const stepResult = steps['tckn-gsm'];
  const isCompleted = stepResult?.status === 'success';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    const tcknValidation = tcknSchema.safeParse(localTckn);
    const gsmValidation = gsmSchema.safeParse(localGsm);
    
    if (!tcknValidation.success) {
      setError(tcknValidation.error.errors[0].message);
      return;
    }
    
    if (!gsmValidation.success) {
      setError(gsmValidation.error.errors[0].message);
      return;
    }

    setIsLoading(true);
    setStepResult('tckn-gsm', { status: 'loading' });

    try {
      const requestBody = {
        tckn: localTckn,
        gsm: localGsm,
      };

      const response = await fetch('/api/ep/tckn-gsm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();

      if (result.error) {
        setStepResult('tckn-gsm', {
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
          if (typeof dataObj.customerId === 'number') {
            setCustomerId(dataObj.customerId);
          } else if (typeof dataObj.id === 'number') {
            setCustomerId(dataObj.id);
          }
        }

        setTckn(localTckn);
        setGsm(localGsm);
        
        setStepResult('tckn-gsm', {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('tckn-gsm', {
        status: 'error',
        error: errorMsg,
      });
      setError(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sol Panel - Form */}
      <div className="w-[500px] bg-white flex flex-col border-r border-gray-200">
        {/* Header */}
        <div className="p-6 border-b border-gray-200">
          <button
            onClick={() => router.push('/')}
            className="text-gray-600 hover:text-gray-900 flex items-center space-x-2 transition-colors mb-4"
          >
            <span className="text-xl">←</span>
            <span className="font-medium">Ana Sayfa</span>
          </button>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">TCKN & GSM Doğrulama</h1>
            <div className="flex items-center space-x-2 mt-2">
              <span className="inline-flex items-center px-3 py-1 rounded-md text-xs font-semibold bg-purple-100 text-purple-700">
                POST
              </span>
              <span className="text-sm text-gray-500">Adım 1/12</span>
            </div>
          </div>
        </div>

        {/* Form Content */}
        <div className="flex-1 overflow-y-auto p-6">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                TCKN <span className="text-gray-400 font-normal">(11 hane)</span>
              </label>
              <input
                type="text"
                value={localTckn}
                onChange={(e) => setLocalTckn(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gray-800 focus:border-transparent transition-all"
                placeholder="12345678901"
                maxLength={11}
                disabled={isLoading}
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                GSM <span className="text-gray-400 font-normal">(10-11 hane)</span>
              </label>
              <input
                type="text"
                value={localGsm}
                onChange={(e) => setLocalGsm(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gray-800 focus:border-transparent transition-all"
                placeholder="5551234567"
                maxLength={11}
                disabled={isLoading}
              />
            </div>

            {error && (
              <div className="bg-red-50 border-l-4 border-red-500 rounded-lg p-4 text-sm text-red-700">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gray-800 text-white py-3 px-4 rounded-lg hover:bg-gray-900 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors font-semibold"
            >
              {isLoading ? 'Gönderiliyor...' : 'Doğrula'}
            </button>

            {isCompleted && (
              <button
                type="button"
                onClick={() => router.push('/flow/kvkk')}
                className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 transition-colors font-semibold"
              >
                Sonraki Adım: KVKK →
              </button>
            )}
          </form>
        </div>
      </div>

      {/* Sağ Panel - Response */}
      <div className="flex-1 flex flex-col bg-gray-50">
        <div className="border-b border-gray-200 bg-white px-8 py-6">
          <h2 className="text-xl font-bold text-gray-800">Sonuç</h2>
          <p className="text-sm text-gray-500 mt-1">Request ve Response detayları</p>
        </div>
        <div className="flex-1 overflow-y-auto p-8">
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
  );
}
