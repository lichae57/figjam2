'use client';

import { useRouter } from 'next/navigation';
import Image from 'next/image';
import { ReactNode } from 'react';

interface FlowLayoutProps {
  children: ReactNode;
  title: string;
  stepNumber: number;
  method: 'GET' | 'POST';
}

export function FlowLayout({ children, title, stepNumber, method }: FlowLayoutProps) {
  const router = useRouter();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-5xl mx-auto px-8 py-6">
          <div className="flex items-center justify-between mb-4">
            <button
              onClick={() => router.push('/')}
              className="text-gray-600 hover:text-gray-900 flex items-center space-x-2 transition-colors"
            >
              <span className="text-xl">←</span>
              <span className="font-medium">Ana Sayfa</span>
            </button>
            <Image 
              src="https://interaktifkredi.com.tr/images/InteraktifKrediNewLogo.png" 
              alt="İnteraktif Kredi" 
              width={180}
              height={48}
              className="h-12 w-auto object-contain"
            />
            <div className="text-sm text-gray-500">
              Adım {stepNumber}/12
            </div>
          </div>
          <div className="mt-4">
            <h1 className="text-3xl font-bold text-gray-900">{title}</h1>
            <div className="mt-2">
              <span
                className={`inline-flex items-center px-3 py-1 rounded-md text-xs font-semibold ${
                  method === 'GET'
                    ? 'bg-blue-100 text-blue-700'
                    : 'bg-purple-100 text-purple-700'
                }`}
              >
                {method}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-5xl mx-auto px-8 py-8">
        {children}
      </div>
    </div>
  );
}

