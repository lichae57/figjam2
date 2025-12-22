'use client';

import { useState } from 'react';

interface JsonViewerProps {
  data: unknown;
  title?: string;
}

export function JsonViewer({ data, title }: JsonViewerProps) {
  const [copied, setCopied] = useState(false);
  
  let displayText: string;
  
  try {
    if (typeof data === 'string') {
      // Try to parse if it's a JSON string
      try {
        const parsed = JSON.parse(data);
        displayText = JSON.stringify(parsed, null, 2);
      } catch {
        displayText = data;
      }
    } else {
      displayText = JSON.stringify(data, null, 2);
    }
  } catch {
    displayText = String(data);
  }

  const handleCopy = () => {
    navigator.clipboard.writeText(displayText);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="w-full">
      {title && (
        <div className="flex items-center justify-between mb-2">
          <div className="text-xs font-semibold text-gray-500 uppercase tracking-wider">{title}</div>
          <button
            onClick={handleCopy}
            className={`flex items-center justify-center w-7 h-7 rounded transition-all ${
              copied 
                ? 'bg-green-600 text-white' 
                : 'bg-gray-700 hover:bg-gray-600 text-gray-200'
            }`}
            title="Kopyala"
          >
            {copied ? (
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
              </svg>
            ) : (
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                <path d="M8 3a1 1 0 011-1h2a1 1 0 110 2H9a1 1 0 01-1-1z" />
                <path d="M6 3a2 2 0 00-2 2v11a2 2 0 002 2h8a2 2 0 002-2V5a2 2 0 00-2-2 3 3 0 01-3 3H9a3 3 0 01-3-3z" />
              </svg>
            )}
          </button>
        </div>
      )}
      <pre className="bg-gray-800 text-gray-100 border border-gray-700 rounded-md p-3 overflow-x-auto text-xs font-mono max-h-[400px] overflow-y-auto">
        {displayText}
      </pre>
    </div>
  );
}

