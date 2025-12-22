import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://api-idc.azurewebsites.net/api/dummy/report-list?code=09wQ_IdxHgsj4oPYrxSrQMbedNddK56Q60lsZPpS5CckAzFu3c0m1A==';

export async function GET(request: NextRequest) {
  try {
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] dummy-report-list error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

