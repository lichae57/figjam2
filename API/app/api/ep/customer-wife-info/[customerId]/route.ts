import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ customerId: string }> }
) {
  try {
    const { customerId } = await params;
    const EP_URL = `https://customers-api.azurewebsites.net/api/customer/wife-info/${customerId}?code=2b9e1uy3xw7MzQooWUvZwS3WsFjHsaffyE2XDlfCdJmYAzFuPszXkw==`;
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] customer-wife-info GET error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}
