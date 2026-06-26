export type ApiResult = {
  status: number;
  statusText: string;
  ok: boolean;
  url: string;
  data: unknown;
};

export type ApiConfig = {
  eventBaseUrl: string;
  ticketBaseUrl: string;
  token: string;
};

const trimTrailingSlash = (value: string) => value.replace(/\/+$/, '');

export async function sendRequest(
  baseUrl: string,
  path: string,
  token: string,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
  body?: unknown
): Promise<ApiResult> {
  const url = `${trimTrailingSlash(baseUrl)}${path}`;
  const headers: Record<string, string> = {
    Accept: 'application/json'
  };

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  if (token.trim()) {
    headers.Authorization = token.trim().startsWith('Bearer ')
      ? token.trim()
      : `Bearer ${token.trim()}`;
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body)
  });

  const contentType = response.headers.get('content-type') ?? '';
  let data: unknown = null;

  if (response.status !== 204) {
    if (contentType.includes('application/json')) {
      data = await response.json();
    } else {
      data = await response.text();
    }
  }

  return {
    status: response.status,
    statusText: response.statusText,
    ok: response.ok,
    url,
    data
  };
}
