/**
 * Utility functions - client-safe
 */

export function maskToken(token: string): string {
  if (!token || token.length < 20) return '***';
  return `${token.substring(0, 10)}...${token.substring(token.length - 10)}`;
}

