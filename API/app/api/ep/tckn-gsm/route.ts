import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://customers-api.azurewebsites.net/api/customer/tckn-gsm?code=gww5m66SOHBjQ9LY58dM5Gulq2giauLokvvIX4ylR405AzFu7CUbIA==';

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
    console.error('[API] tckn-gsm error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

