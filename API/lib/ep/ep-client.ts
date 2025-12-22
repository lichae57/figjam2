export interface EpRequestOptions {
  method: 'GET' | 'POST';
  url: string;
  body?: unknown;
  token?: string;
}

export interface EpResponse<T = unknown> {
  status: number;
  data: T;
  elapsed: number;
  error?: string;
}

export async function callExternalEp<T = unknown>(
  options: EpRequestOptions
): Promise<EpResponse<T>> {
  const startTime = Date.now();
  
  try {
    const headers: Record<string, string> = {};
    
    if (options.method === 'POST') {
      headers['Content-Type'] = 'application/json';
    }
    
    if (options.token) {
      headers['Authorization'] = `Bearer ${options.token}`;
    }

    const fetchOptions: RequestInit = {
      method: options.method,
      headers,
      cache: 'no-store',
    };

    if (options.method === 'POST' && options.body) {
      fetchOptions.body = JSON.stringify(options.body);
    }

    console.log(`[EP Client] ${options.method} ${options.url}`);
    
    const response = await fetch(options.url, fetchOptions);
    const elapsed = Date.now() - startTime;
    
    let data: T;
    const contentType = response.headers.get('content-type');
    
    if (contentType && contentType.includes('application/json')) {
      data = await response.json();
    } else {
      const text = await response.text();
      data = text as T;
    }

    console.log(`[EP Client] ${options.method} ${options.url} - Status: ${response.status} - Elapsed: ${elapsed}ms`);

    if (!response.ok) {
      return {
        status: response.status,
        data,
        elapsed,
        error: `HTTP ${response.status}: ${response.statusText}`,
      };
    }

    return {
      status: response.status,
      data,
      elapsed,
    };
  } catch (error) {
    const elapsed = Date.now() - startTime;
    console.error(`[EP Client] ${options.method} ${options.url} - Error:`, error);
    
    return {
      status: 500,
      data: {} as T,
      elapsed,
      error: error instanceof Error ? error.message : 'Unknown error',
    };
  }
}

