import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL = 'https://api-idc.azurewebsites.net/api/generate-otp?code=NgHo3RJwMJ4rVsPtGLau40m_vykzGV24zBAZYJPbVQpIAzFurGPRTw==';

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
    console.error('[API] generate-otp error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

