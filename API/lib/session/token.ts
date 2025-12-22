import { cookies } from 'next/headers';
import { API_CONFIG } from '@/lib/config';

const TOKEN_COOKIE_NAME = 'idc_token';

export async function setToken(token: string): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.set(TOKEN_COOKIE_NAME, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: 60 * 60 * 24, // 24 saat
    path: '/',
  });
}

export async function getToken(request?: { headers: Headers | HeadersInit }): Promise<string> {
  // Önce request header'ından token al (figjam2 entegrasyonu için)
  if (request?.headers) {
    try {
      const headers = request.headers instanceof Headers 
        ? request.headers 
        : new Headers(request.headers);
      const authHeader = headers.get('authorization');
      if (authHeader && authHeader.startsWith('Bearer ')) {
        return authHeader.substring(7);
      }
    } catch (error) {
      // Header okunamazsa devam et
    }
  }
  
  // Sonra cookie'den token al
  try {
    const cookieStore = await cookies();
    const cookieToken = cookieStore.get(TOKEN_COOKIE_NAME)?.value;
    
    if (cookieToken) {
      return cookieToken;
    }
  } catch (error) {
    // Cookie okunamazsa devam et
  }
  
  // Son olarak default token'i kullan
  return API_CONFIG.DEFAULT_TOKEN;
}

export async function clearToken(): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.delete(TOKEN_COOKIE_NAME);
}

