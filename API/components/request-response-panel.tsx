'use client';

import { useState } from 'react';
import { JsonViewer } from './json-viewer';

interface RequestResponsePanelProps {
  request?: unknown;
  response?: unknown;
  error?: string;
  elapsed?: number;
  status?: 'idle' | 'loading' | 'success' | 'error';
}

export function RequestResponsePanel({
  request,
  response,
  error,
  elapsed,
  status = 'idle',
}: RequestResponsePanelProps) {
  const [copiedError, setCopiedError] = useState(false);

  const handleCopyError = () => {
    if (error) {
      navigator.clipboard.writeText(error);
      setCopiedError(true);
      setTimeout(() => setCopiedError(false), 2000);
    }
  };
  if (status === 'idle') {
    return (
      <div className="flex items-center justify-center h-48 bg-gray-50 border-2 border-dashed border-gray-300 rounded-md">
        <div className="text-center">
          <div className="text-3xl mb-2">⚡</div>
          <div className="text-gray-500 text-sm font-medium">Henüz çalıştırılmadı</div>
          <div className="text-gray-400 text-xs mt-1">"Test Et" butonuna tıklayın</div>
        </div>
      </div>
    );
  }

  if (status === 'loading') {
    return (
      <div className="flex items-center justify-center h-48 bg-gray-50 border-2 border-gray-300 rounded-md">
        <div className="text-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-4 border-gray-800 mx-auto mb-3"></div>
          <span className="text-gray-700 text-sm font-medium">Yükleniyor...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Status Bar */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          {status === 'success' ? (
            <div className="flex items-center space-x-1.5 text-green-700 bg-green-50 px-3 py-1.5 rounded-md border border-green-200">
              <span className="text-sm">✓</span>
              <span className="text-xs font-semibold">Başarılı</span>
            </div>
          ) : (
            <div className="flex items-center space-x-1.5 text-red-700 bg-red-50 px-3 py-1.5 rounded-md border border-red-200">
              <span className="text-sm">✗</span>
              <span className="text-xs font-semibold">Hatalı</span>
            </div>
          )}
        </div>
        
        {elapsed !== undefined && (
          <div className="flex items-center space-x-1.5 text-gray-700 bg-gray-100 px-3 py-1.5 rounded-md border border-gray-300">
            <span className="text-xs">⏱️</span>
            <span className="text-xs font-semibold">{elapsed}ms</span>
          </div>
        )}
      </div>

      {error && (
        <div className="bg-red-50 border-l-4 border-red-500 rounded-md p-3">
          <div className="flex items-center justify-between mb-1.5">
            <div className="flex items-center space-x-1.5">
              <span className="text-sm">⚠️</span>
              <div className="text-xs font-bold text-red-900">Hata Mesajı</div>
            </div>
            <button
              onClick={handleCopyError}
              className={`flex items-center justify-center w-6 h-6 rounded transition-all ${
                copiedError 
                  ? 'bg-green-600 text-white' 
                  : 'bg-red-700 hover:bg-red-800 text-white'
              }`}
              title="Hatayı kopyala"
            >
              {copiedError ? (
                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
              ) : (
                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                  <path d="M8 3a1 1 0 011-1h2a1 1 0 110 2H9a1 1 0 01-1-1z" />
                  <path d="M6 3a2 2 0 00-2 2v11a2 2 0 002 2h8a2 2 0 002-2V5a2 2 0 00-2-2 3 3 0 01-3 3H9a3 3 0 01-3-3z" />
                </svg>
              )}
            </button>
          </div>
          <div className="text-xs text-red-800 font-mono">{error}</div>
        </div>
      )}

      {/* Request/Response */}
      <div className="space-y-4">
        {request && (
          <div>
            <JsonViewer data={request} title="Request" />
          </div>
        )}

        {response && (
          <div>
            <JsonViewer data={response} title="Response" />
          </div>
        )}
      </div>
    </div>
  );
}

