import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { setToken, getToken } from '@/lib/session/token';

const EP_URL = 'https://api-idc.azurewebsites.net/api/verify-otp?code=NUqFOooe6OqRE8Nf5Se8Swlt5vUxjdMr3oJZes_-MioAAzFuP2nw2g==';

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

    // Token extraction logic
    if (result.status === 200 && result.data) {
      let token: string | undefined;
      
      // Check different possible token locations
      if (typeof result.data === 'object' && result.data !== null) {
        const dataObj = result.data as Record<string, unknown>;
        
        if (typeof dataObj.token === 'string') {
          token = dataObj.token;
        } else if (typeof dataObj.access_token === 'string') {
          token = dataObj.access_token;
        } else if (typeof dataObj.jwt === 'string') {
          token = dataObj.jwt;
        } else if (typeof dataObj.accessToken === 'string') {
          token = dataObj.accessToken;
        }
      }
      
      if (token) {
        await setToken(token);
        console.log('[API] verify-otp: Token set successfully');
      }
    }

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] verify-otp error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

