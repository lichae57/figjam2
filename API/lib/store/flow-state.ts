'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { API_CONFIG } from '@/lib/config';

export interface StepResult {
  status: 'idle' | 'loading' | 'success' | 'error';
  request?: unknown;
  response?: unknown;
  error?: string;
  elapsed?: number;
}

export interface FlowState {
  // User inputs
  tckn: string;
  gsm: string;
  customerId: number | null;
  kvkkId: number;
  otpCode: string;
  dealerId: number;
  
  // Flow data
  reportIds: string[];
  selectedReportId: string;
  token: string;
  
  // Step results
  steps: Record<string, StepResult>;
  
  // Actions
  setTckn: (tckn: string) => void;
  setGsm: (gsm: string) => void;
  setCustomerId: (id: number | null) => void;
  setKvkkId: (id: number) => void;
  setOtpCode: (code: string) => void;
  setDealerId: (id: number) => void;
  setReportIds: (ids: string[]) => void;
  setSelectedReportId: (id: string) => void;
  setToken: (token: string) => void;
  setStepResult: (stepKey: string, result: StepResult) => void;
  resetFlow: () => void;
  isStepCompleted: (stepKey: string) => boolean;
}

export const useFlowStore = create<FlowState>()(
  persist(
    (set, get) => ({
      // Initial state
      tckn: '12345678901',
      gsm: '5551112233',
      customerId: 1000849,
      kvkkId: 2,
      otpCode: '123456',
      dealerId: 1081,
      reportIds: [],
      selectedReportId: '',
      token: API_CONFIG.DEFAULT_TOKEN,
      steps: {},
      
      // Actions
      setTckn: (tckn) => set({ tckn }),
      setGsm: (gsm) => set({ gsm }),
      setCustomerId: (id) => set({ customerId: id }),
      setKvkkId: (id) => set({ kvkkId: id }),
      setOtpCode: (code) => set({ otpCode: code }),
      setDealerId: (id) => set({ dealerId: id }),
      setReportIds: (ids) => set({ reportIds: ids }),
      setSelectedReportId: (id) => set({ selectedReportId: id }),
      setToken: (token) => set({ token }),
      
      setStepResult: (stepKey, result) =>
        set((state) => ({
          steps: {
            ...state.steps,
            [stepKey]: result,
          },
        })),
      
      resetFlow: () =>
        set({
          tckn: '12345678901',
          gsm: '5551112233',
          customerId: 1000849,
          kvkkId: 2,
          otpCode: '123456',
          dealerId: 1081,
          reportIds: [],
          selectedReportId: '',
          token: API_CONFIG.DEFAULT_TOKEN,
          steps: {},
        }),
      
      isStepCompleted: (stepKey) => {
        const step = get().steps[stepKey];
        return step?.status === 'success';
      },
    }),
    {
      name: 'idc-flow-storage',
      partialize: (state) => ({
        tckn: state.tckn,
        gsm: state.gsm,
        customerId: state.customerId,
        kvkkId: state.kvkkId,
        otpCode: state.otpCode,
        dealerId: state.dealerId,
        reportIds: state.reportIds,
        selectedReportId: state.selectedReportId,
        token: state.token,
        steps: state.steps, // Request/response verileri de kaydediliyor
      }),
    }
  )
);

