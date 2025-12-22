'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { StepCard } from '@/components/step-card';
import { RequestResponsePanel } from '@/components/request-response-panel';
import { useFlowStore } from '@/lib/store/flow-state';

export default function ProfilePage() {
  const router = useRouter();
  const { customerId, dealerId, setCustomerId, setDealerId, setStepResult, steps, isStepCompleted } = useFlowStore();
  const [localCustomerId, setLocalCustomerId] = useState(customerId || 1000849);
  const [localDealerId, setLocalDealerId] = useState(dealerId || 1081);
  const [isLoadingAddress, setIsLoadingAddress] = useState(false);
  const [isLoadingSalary, setIsLoadingSalary] = useState(false);
  const [isLoadingJobInfo, setIsLoadingJobInfo] = useState(false);
  const [isLoadingJobProfile, setIsLoadingJobProfile] = useState(false);
  const [jobProfileData, setJobProfileData] = useState(JSON.stringify({
    customerId: 1000849, // Varsayılan ID (veya localCustomerId ile eşleşen)
    customerWork: 5,
    jobGroupId: 3,
    workingYears: 5,
    workingMonth: 2,
    titleCompany: "ABC Teknoloji A.Ş.",
    companyPosition: "Kıdemli Yazılım Geliştirici"
  }, null, 2));

  const stepResultAddress = steps['customer-address'];
  const stepResultSalary = steps['customer-salary'];
  const stepResultJobInfo = steps['customer-job-info'];
  const stepResultJobProfile = steps['customer-job-profile'];
  const isCompleted = stepResultJobProfile?.status === 'success';
  const isLocked = false; // Artık bağımlılık yok

  const handleFetchAddress = async () => {
    if (!localCustomerId) return;

    setIsLoadingAddress(true);
    setStepResult('customer-address', { status: 'loading' });
    setCustomerId(localCustomerId);

    try {
      const response = await fetch(`/api/ep/customer-address/${localCustomerId}`);
      const result = await response.json();

      if (result.error) {
        setStepResult('customer-address', {
          status: 'error',
          request: { customerId: localCustomerId },
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('customer-address', {
          status: 'success',
          request: { customerId: localCustomerId },
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('customer-address', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingAddress(false);
    }
  };

  const handleFetchSalary = async () => {
    if (!localCustomerId) return;

    setIsLoadingSalary(true);
    setStepResult('customer-salary', { status: 'loading' });
    setCustomerId(localCustomerId);
    setDealerId(localDealerId);

    try {
      const response = await fetch(`/api/ep/customer-salary/${localCustomerId}/${localDealerId}`);
      const result = await response.json();

      if (result.error) {
        setStepResult('customer-salary', {
          status: 'error',
          request: { customerId: localCustomerId, dealerId: localDealerId },
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('customer-salary', {
          status: 'success',
          request: { customerId: localCustomerId, dealerId: localDealerId },
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('customer-salary', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingSalary(false);
    }
  };

  const handleFetchJobInfo = async () => {
    if (!localCustomerId) return;

    setIsLoadingJobInfo(true);
    setStepResult('customer-job-info', { status: 'loading' });
    setCustomerId(localCustomerId);

    try {
      const response = await fetch(`/api/ep/customer-job-info/${localCustomerId}`);
      const result = await response.json();

      if (result.error) {
        setStepResult('customer-job-info', {
          status: 'error',
          request: { customerId: localCustomerId },
          response: result.data,
          error: result.error,
          elapsed: result.elapsed,
        });
      } else {
        setStepResult('customer-job-info', {
          status: 'success',
          request: { customerId: localCustomerId },
          response: result.data,
          elapsed: result.elapsed,
        });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Bilinmeyen hata';
      setStepResult('customer-job-info', {
        status: 'error',
        error: errorMsg,
      });
    } finally {
      setIsLoadingJobInfo(false);
    }
  };

  const handleUpsertJobProfile = async () => {
    if (!localCustomerId) return;

    setIsLoadingJobProfile(true);
    setStepResult('customer-job-profile', { status: 'loading' });
    setCustomerId(localCustomerId);

    try {
      let requestBody: unknown;
      try {
        requestBody = JSON.parse(jobProfileData);
      } catch {
        // JSON parse edilmezse varsayılan yapı
        requestBody = {
          customerId: localCustomerId,
          customerWork: 5,
          jobGroupId: 3,
          workingYears: 5,
          workingMonth: 2,
          titleCompany: "ABC Teknoloji A.Ş.",
          companyPosition: "Kıdemli Yazılım Geliştirici"
        };
      }

      const response = await fetch('/api/ep/customer-job-profile', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(requestBody),
      });

      const result = await response.json();
      // ... (Kalan kısım aynı)

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

            <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-2">
              <div className="flex items-center space-x-4">
                <label className="text-sm font-medium text-gray-700 w-32">
                  Customer ID:
                </label>
                <input
                  type="number"
                  value={localCustomerId}
                  onChange={(e) => setLocalCustomerId(Number(e.target.value))}
                  className="flex-1 px-3 py-1 border border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                  placeholder="Customer ID girin"
                />
              </div>
              <div className="flex items-center space-x-4">
                <label className="text-sm font-medium text-gray-700 w-32">
                  Dealer ID:
                </label>
                <input
                  type="number"
                  value={localDealerId}
                  onChange={(e) => setLocalDealerId(Number(e.target.value))}
                  className="flex-1 px-3 py-1 border border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <StepCard
              title="Müşteri Adres"
              stepNumber={9}
              method="GET"
              isCompleted={stepResultAddress?.status === 'success'}
              isLocked={isLocked}
            >
              <div className="space-y-4">
                <button
                  onClick={handleFetchAddress}
                  disabled={isLoadingAddress || !localCustomerId}
                  className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isLoadingAddress ? 'Getiriliyor...' : 'Adres Bilgisini Getir'}
                </button>

                <RequestResponsePanel
                  request={stepResultAddress?.request}
                  response={stepResultAddress?.response}
                  error={stepResultAddress?.error}
                  elapsed={stepResultAddress?.elapsed}
                  status={stepResultAddress?.status || 'idle'}
                />
              </div>
            </StepCard>

            <StepCard
              title="Maaş Bilgisi"
              stepNumber={10}
              method="GET"
              isCompleted={stepResultSalary?.status === 'success'}
              isLocked={isLocked}
            >
              <div className="space-y-4">
                <button
                  onClick={handleFetchSalary}
                  disabled={isLoadingSalary || !localCustomerId}
                  className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isLoadingSalary ? 'Getiriliyor...' : 'Maaş Bilgisini Getir'}
                </button>

                <RequestResponsePanel
                  request={stepResultSalary?.request}
                  response={stepResultSalary?.response}
                  error={stepResultSalary?.error}
                  elapsed={stepResultSalary?.elapsed}
                  status={stepResultSalary?.status || 'idle'}
                />
              </div>
            </StepCard>

            <StepCard
              title="İş Bilgisi"
              stepNumber={11}
              method="GET"
              isCompleted={stepResultJobInfo?.status === 'success'}
              isLocked={isLocked}
            >
              <div className="space-y-4">
                <button
                  onClick={handleFetchJobInfo}
                  disabled={isLoadingJobInfo || !localCustomerId}
                  className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isLoadingJobInfo ? 'Getiriliyor...' : 'İş Bilgisini Getir'}
                </button>

                <RequestResponsePanel
                  request={stepResultJobInfo?.request}
                  response={stepResultJobInfo?.response}
                  error={stepResultJobInfo?.error}
                  elapsed={stepResultJobInfo?.elapsed}
                  status={stepResultJobInfo?.status || 'idle'}
                />
              </div>
            </StepCard>

            <StepCard
              title="İş Profili Kaydet/Güncelle"
              stepNumber={12}
              method="POST"
              isCompleted={isCompleted}
              isLocked={isLocked}
            >
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    JSON Data (İsteğe göre düzenleyin)
                  </label>
                  <textarea
                    value={jobProfileData}
                    onChange={(e) => setJobProfileData(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono text-sm"
                    rows={6}
                    placeholder='{"customerId": 123, "jobTitle": "Engineer"}'
                    disabled={isLoadingJobProfile}
                  />
                </div>

                <button
                  onClick={handleUpsertJobProfile}
                  disabled={isLoadingJobProfile || !localCustomerId}
                  className="w-full bg-green-600 text-white py-2 px-4 rounded-lg hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isLoadingJobProfile ? 'Kaydediliyor...' : 'İş Profilini Kaydet'}
                </button>

                <RequestResponsePanel
                  request={stepResultJobProfile?.request}
                  response={stepResultJobProfile?.response}
                  error={stepResultJobProfile?.error}
                  elapsed={stepResultJobProfile?.elapsed}
                  status={stepResultJobProfile?.status || 'idle'}
                />
              </div>

              {isCompleted && (
                <div className="mt-6 bg-green-50 border border-green-200 rounded-lg p-4">
                  <div className="text-green-800 font-medium text-center">
                    🎉 Tüm adımlar tamamlandı!
                  </div>
                  <button
                    onClick={() => router.push('/')}
                    className="w-full mt-4 bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    Ana Sayfaya Dön
                  </button>
                </div>
              )}
            </StepCard>
          </div>
        </div>
      );
    }

