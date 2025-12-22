import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

export async function GET(request: NextRequest) {
  try {
    const searchParams = request.nextUrl.searchParams;
    const reportId = searchParams.get('reportId');
    
    if (!reportId) {
      return NextResponse.json(
        { error: 'reportId query parameter is required' },
        { status: 400 }
      );
    }
    
    const EP_URL = `https://api-idc.azurewebsites.net/api/GetReportDetail?code=ghMl1aeJa-tKFvo9hkAn3Cnzgkbf_sc3ZYg7tWvPLfLhAzFuAm67hQ==&reportId=${reportId}`;
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] report-detail error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

