import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://api-idc.azurewebsites.net/api/send-otp-sms?code=fsfjOxq_5dhmOKxp4Q6ffi5ZuxdmGO2cnwGi2IBwRzSjAzFu9FrxaQ==';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const token = await getToken({ headers: request.headers });
    
    const result = await callExternalEp({
      method: 'POST',
      url: EP_URL,
      body,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] send-otp-sms error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

