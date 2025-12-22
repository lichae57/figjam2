import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ customerId: string }> }
) {
  try {
    const { customerId } = await params;
    const EP_URL = `https://customers-api.azurewebsites.net/api/customer/job-infonew/${customerId}?code=RPR5Pwi9E_TDXZb1f27lPNrk7jMF67Et58elqdNsilWNAzFup83XlQ==`;
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] customer-job-info error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

