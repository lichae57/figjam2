import { NextRequest, NextResponse } from 'next/server';
import { callExternalEp } from '@/lib/ep/ep-client';
import { getToken } from '@/lib/session/token';

const EP_URL_POST = 'https://customers-api.azurewebsites.net/api/customer/address?code=HiKhittOb6dIjhw7YqA-W1W51iKtQ6D1UoHmKQl91rYSAzFuOurQTQ==';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ customerId: string }> }
) {
  try {
    const { customerId } = await params;
    const EP_URL = `https://customers-api.azurewebsites.net/api/customer/addressfull/${customerId}?code=rNlR8hjUVN2GeCrR0Ac7px6kKA-pSeI-swMngYss39FAAzFuULEzbQ==`;
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'GET',
      url: EP_URL,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] customer-address GET error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const token = await getToken();
    
    const result = await callExternalEp({
      method: 'POST',
      url: EP_URL_POST,
      body,
      token,
    });

    return NextResponse.json(result, { status: result.status });
  } catch (error) {
    console.error('[API] customer-address POST error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

