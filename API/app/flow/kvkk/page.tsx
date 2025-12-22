'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { useFlowStore } from '@/lib/store/flow-state';

export default function KvkkPage() {
  const router = useRouter();
  const { kvkkId, setKvkkId, setStepResult, steps, isStepCompleted } = useFlowStore();
  const [localKvkkId, setLocalKvkkId] = useState(kvkkId);
  const [isLoadingText, setIsLoadingText] = useState(false);
  const [isLoadingOnay, setIsLoadingOnay] = useState(false);
  const [kvkkText, setKvkkText] = useState('');
  const [activeTab, setActiveTab] = useState<'text' | 'onay'>('text');

  const stepResultText = steps['kvkk-text'];
  const stepResultOnay = steps['kvkk-onay'];
  const isCompleted = stepResultOnay?.status === 'success';
  const isLocked = false; // Artık bağımlılık yok

  const handleFetchText = async () => {
    setIsLoadingText(true);
    setStepResult('kvkk-text', { status: 'loading' });

    try {
      const response = await fetch(`/api/ep/kvkk-text/${localKvkkId}`);
      const result = await response.json();

      if (result.error) {
        setStepResult('kvkk-text', {
          status: 'error',
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        if (result.data && typeof result.data === 'object') {
          const dataObj = result.data as Record<string, unknown>;
          if (typeof dataObj.text === 'string') {
            setKvkkText(dataObj.text);
          } else if (typeof dataObj.content === 'string') {
            setKvkkText(dataObj.content);
          }
        }

        setKvkkId(localKvkkId);
        setStepResult('kvkk-text', {
          status: 'success',
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('kvkk-text', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingText(false);
    }
  };

  const handleApprove = async () => {
    setIsLoadingOnay(true);
    setStepResult('kvkk-onay', { status: 'loading' });

    try {
      const requestBody = {
        kvkkId: localKvkkId,
        approved: true,
        timestamp: new Date().toISOString(),
      };

      const response = await fetch('/api/ep/kvkk-onay', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();

      if (result.error) {
        setStepResult('kvkk-onay', {
          status: 'error',
          request: requestBody,
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('kvkk-onay', {
          status: 'success',
          request: requestBody,
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('kvkk-onay', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingOnay(false);
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
            <h1 className="text-2xl font-bold text-gray-900">KVKK</h1>
            <div className="flex items-center space-x-2 mt-2">
              <span className="text-sm text-gray-500">Adım 2-3/12</span>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-gray-200">
          <button
            onClick={() => setActiveTab('text')}
            className={`flex-1 px-4 py-3 text-sm font-semibold transition-colors ${
              activeTab === 'text'
                ? 'text-gray-900 border-b-2 border-gray-900'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            KVKK Metni (GET)
          </button>
          <button
            onClick={() => setActiveTab('onay')}
            className={`flex-1 px-4 py-3 text-sm font-semibold transition-colors ${
              activeTab === 'onay'
                ? 'text-gray-900 border-b-2 border-gray-900'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            KVKK Onay (POST)
          </button>
        </div>

        {/* Form Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {activeTab === 'text' ? (
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  KVKK ID
                </label>
                <input
                  type="number"
                  value={localKvkkId}
                  onChange={(e) => setLocalKvkkId(Number(e.target.value))}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gray-800 focus:border-transparent transition-all"
                  disabled={isLoadingText}
                />
              </div>

              <button
                onClick={handleFetchText}
                disabled={isLoadingText}
                className="w-full bg-gray-800 text-white py-3 px-4 rounded-lg hover:bg-gray-900 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors font-semibold"
              >
                {isLoadingText ? 'Getiriliyor...' : 'KVKK Metnini Getir'}
              </button>

              {kvkkText && (
                <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 max-h-64 overflow-y-auto">
                  <div className="text-sm text-gray-800 whitespace-pre-wrap">{kvkkText}</div>
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-5">
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <p className="text-sm text-blue-900">
                  KVKK metnini okuyup onayladığınızı beyan ediyorsunuz.
                </p>
              </div>

              <button
                onClick={handleApprove}
                disabled={isLoadingOnay || stepResultText?.status !== 'success'}
                className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors font-semibold"
              >
                {isLoadingOnay ? 'Kaydediliyor...' : 'KVKK\'yı Onayla'}
              </button>

              {stepResultText?.status !== 'success' && (
                <p className="text-sm text-gray-500 text-center">
                  Önce KVKK metnini getirin
                </p>
              )}
            </div>
          )}

          {isCompleted && (
            <button
              onClick={() => router.push('/flow/otp')}
              className="w-full mt-6 bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 transition-colors font-semibold"
            >
              Sonraki Adım: OTP →
            </button>
          )}
        </div>
      </div>

      {/* Sağ Panel - Response */}
      <div className="flex-1 flex flex-col bg-gray-50">
        <div className="border-b border-gray-200 bg-white px-8 py-6">
          <h2 className="text-xl font-bold text-gray-800">
            {activeTab === 'text' ? 'KVKK Metni - Sonuç' : 'KVKK Onay - Sonuç'}
          </h2>
          <p className="text-sm text-gray-500 mt-1">Request ve Response detayları</p>
        </div>
        <div className="flex-1 overflow-y-auto p-8">
          <RequestResponsePanel
            request={activeTab === 'text' ? stepResultText?.request : stepResultOnay?.request}
            response={activeTab === 'text' ? stepResultText?.response : stepResultOnay?.response}
            error={activeTab === 'text' ? stepResultText?.error : stepResultOnay?.error}
            elapsed={activeTab === 'text' ? stepResultText?.elapsed : stepResultOnay?.elapsed}
            status={activeTab === 'text' ? (stepResultText?.status || 'idle') : (stepResultOnay?.status || 'idle')}
          />
        </div>
      </div>
    </div>
  );
}
