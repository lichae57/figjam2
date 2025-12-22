import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  try {
    const { id } = await params;
    const EP_URL = `https://api-idc.azurewebsites.net/api/kvkk/text/${id}?code=5OaiAxOi6mmwXV4gPTcKiHc9plIMg3s6Kcer667-4OK1AzFulZ-Mkw==`;
    const token = await getToken({ headers: request.headers });
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] kvkk-text error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

