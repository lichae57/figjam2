import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://customers-api.azurewebsites.net/api/customer/wife-info?code=LCslw1w526GGpzHiv3UU8M_jI2XzSDZzCMh5DcKx2MkfAzFuOs-XaQ==';

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
    console.error('[API] customer-wife-info POST error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}
