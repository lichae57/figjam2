import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://customers-api.azurewebsites.net/api/customer/finance-assets?code=rp3u-kwXRt5U2bXDf-1sgsIcYwPcbzl1XVniXw510b7SAzFui9yHVw==';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'POST',
      url: EP_URL,
      body,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] customer-finance-assets POST error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}
