'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { StepCard } from '@/components/step-card';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { useFlowStore } from '@/lib/store/flow-state';

export default function ReportsPage() {
  const router = useRouter();
  const { reportIds, selectedReportId, setReportIds, setSelectedReportId, setStepResult, steps, isStepCompleted } = useFlowStore();
  const [isLoadingList, setIsLoadingList] = useState(false);
  const [isLoadingDetail, setIsLoadingDetail] = useState(false);
  const [localSelectedReportId, setLocalSelectedReportId] = useState(selectedReportId);

  const stepResultList = steps['dummy-report-list'];
  const stepResultDetail = steps['report-detail'];
  const isCompleted = stepResultDetail?.status === 'success';
  const isLocked = false; // Artık bağımlılık yok

  useEffect(() => {
    if (reportIds.length > 0 && !localSelectedReportId) {
      setLocalSelectedReportId(reportIds[0]);
    }
  }, [reportIds, localSelectedReportId]);

  const handleFetchList = async () => {
    setIsLoadingList(true);
    setStepResult('dummy-report-list', { status: 'loading' });

    try {
      const response = await fetch('/api/ep/dummy-report-list');
      const result = await response.json();

      if (result.error) {
        setStepResult('dummy-report-list', {
          status: 'error',
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        // Extract report IDs from response
        let extractedIds: string[] = [];
        if (Array.isArray(result.data)) {
          extractedIds = result.data.map((item: unknown) => {
            if (typeof item === 'string') return item;
            if (typeof item === 'object' && item !== null) {
              const obj = item as Record<string, unknown>;
              return String(obj.id || obj.reportId || obj.report_id || '');
            }
            return String(item);
          }).filter(Boolean);
        } else if (result.data && typeof result.data === 'object') {
          const dataObj = result.data as Record<string, unknown>;
          if (Array.isArray(dataObj.reportIds)) {
            extractedIds = dataObj.reportIds.map(String);
          } else if (Array.isArray(dataObj.reports)) {
            extractedIds = dataObj.reports.map((r: unknown) => {
              if (typeof r === 'object' && r !== null) {
                const obj = r as Record<string, unknown>;
                return String(obj.id || obj.reportId || '');
              }
              return String(r);
            }).filter(Boolean);
          }
        }

        if (extractedIds.length > 0) {
          setReportIds(extractedIds);
          setLocalSelectedReportId(extractedIds[0]);
        }

        setStepResult('dummy-report-list', {
          status: 'success',
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('dummy-report-list', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingList(false);
    }
  };

  const handleFetchDetail = async () => {
    if (!localSelectedReportId) return;

    setIsLoadingDetail(true);
    setStepResult('report-detail', { status: 'loading' });

    try {
      const response = await fetch(`/api/ep/report-detail?reportId=${encodeURIComponent(localSelectedReportId)}`);
      const result = await response.json();

      if (result.error) {
        setStepResult('report-detail', {
          status: 'error',
          request: { reportId: localSelectedReportId },
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setSelectedReportId(localSelectedReportId);
        setStepResult('report-detail', {
          status: 'success',
          request: { reportId: localSelectedReportId },
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('report-detail', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingDetail(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="max-w-4xl mx-auto space-y-6">
        <div>
          <button
            onClick={() => router.push('/')}
            className="text-blue-600 hover:text-blue-800 flex items-center space-x-2"
          >
            <span>←</span>
            <span>Ana Sayfaya Dön</span>
          </button>
        </div>

        <StepCard
          title="Dummy Rapor Listesi"
          stepNumber={7}
          method="GET"
          isCompleted={stepResultList?.status === 'success'}
          isLocked={isLocked}
        >
          <div className="space-y-4">
            <button
              onClick={handleFetchList}
              disabled={isLoadingList}
              className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
            >
              {isLoadingList ? 'Getiriliyor...' : 'Rapor Listesini Getir'}
            </button>

            {reportIds.length > 0 && (
              <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                <div className="text-sm font-medium text-gray-700 mb-2">
                  Bulunan Rapor ID'leri:
                </div>
                <div className="flex flex-wrap gap-2">
                  {reportIds.map((id) => (
                    <span
                      key={id}
                      className="bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-mono"
                    >
                      {id}
                    </span>
                  ))}
                </div>
              </div>
            )}

            <RequestResponsePanel
              response={stepResultList?.response}
              error={stepResultList?.error}
              elapsed={stepResultList?.elapsed}
              status={stepResultList?.status || 'idle'}
            />
          </div>
        </StepCard>

        <StepCard
          title="Rapor Detayı"
          stepNumber={8}
          method="GET"
          isCompleted={isCompleted}
          isLocked={isLocked || stepResultList?.status !== 'success'}
        >
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rapor ID Seçin
              </label>
              {reportIds.length > 0 ? (
                <select
                  value={localSelectedReportId}
                  onChange={(e) => setLocalSelectedReportId(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isLoadingDetail}
                >
                  {reportIds.map((id) => (
                    <option key={id} value={id}>
                      {id}
                    </option>
                  ))}
                </select>
              ) : (
                <input
                  type="text"
                  value={localSelectedReportId}
                  onChange={(e) => setLocalSelectedReportId(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Rapor ID girin"
                  disabled={isLoadingDetail}
                />
              )}
            </div>

            <button
              onClick={handleFetchDetail}
              disabled={isLoadingDetail || !localSelectedReportId}
              className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
            >
              {isLoadingDetail ? 'Getiriliyor...' : 'Rapor Detayını Getir'}
            </button>

            <RequestResponsePanel
              request={stepResultDetail?.request}
              response={stepResultDetail?.response}
              error={stepResultDetail?.error}
              elapsed={stepResultDetail?.elapsed}
              status={stepResultDetail?.status || 'idle'}
            />
          </div>

          {isCompleted && (
            <div className="mt-6">
              <button
                onClick={() => router.push('/flow/profile')}
                className="w-full bg-green-600 text-white py-2 px-4 rounded-lg hover:bg-green-700 transition-colors"
              >
                Sonraki Adım: Profil Bilgileri →
              </button>
            </div>
          )}
        </StepCard>
      </div>
    </div>
  );
}

