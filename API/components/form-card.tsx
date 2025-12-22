'use client';

import { ReactNode } from 'react';

interface FormCardProps {
  title?: string;
  children: ReactNode;
  isCompleted?: boolean;
}

export function FormCard({ title, children, isCompleted }: FormCardProps) {
  return (
    <div className={`bg-white border rounded-xl p-6 shadow-sm ${
      isCompleted ? 'border-green-500' : 'border-gray-200'
    }`}>
      {title && (
        <div className="flex items-center space-x-2 mb-4">
          {isCompleted && <span className="text-green-500 text-xl">✓</span>}
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
        </div>
      )}
      {children}
    </div>
  );
}

