'use client';

import { ReactNode } from 'react';

interface StepCardProps {
  title: string;
  stepNumber: number;
  method: 'GET' | 'POST';
  isCompleted: boolean;
  isLocked: boolean;
  children: ReactNode;
}

export function StepCard({
  title,
  stepNumber,
  method,
  isCompleted,
  isLocked,
  children,
}: StepCardProps) {
  return (
    <div
      className={`border rounded-lg p-6 ${
        isCompleted
          ? 'border-green-300 bg-green-50'
          : isLocked
          ? 'border-gray-200 bg-gray-50 opacity-60'
          : 'border-blue-300 bg-blue-50'
      }`}
    >
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-3">
          <div
            className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
              isCompleted
                ? 'bg-green-500 text-white'
                : isLocked
                ? 'bg-gray-300 text-gray-600'
                : 'bg-blue-500 text-white'
            }`}
          >
            {isCompleted ? '✓' : stepNumber}
          </div>
          <div>
            <h3 className="font-semibold text-gray-800">{title}</h3>
            <div className="flex items-center space-x-2 mt-1">
              <span
                className={`text-xs font-mono px-2 py-0.5 rounded ${
                  method === 'GET'
                    ? 'bg-blue-100 text-blue-700'
                    : 'bg-purple-100 text-purple-700'
                }`}
              >
                {method}
              </span>
              {isLocked && (
                <span className="text-xs text-gray-500">🔒 Kilitli</span>
              )}
            </div>
          </div>
        </div>
      </div>
      
      {isLocked ? (
        <div className="text-sm text-gray-600 italic">
          Önceki adımları tamamlayın
        </div>
      ) : (
        <div>{children}</div>
      )}
    </div>
  );
}

