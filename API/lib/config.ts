/**
 * API Configuration
 * Sabit token ve diğer yapılandırma ayarları
 */

export const API_CONFIG = {
  // Default Bearer token - tüm endpoint'lerde kullanılacak
  DEFAULT_TOKEN: 'fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s',
  
  // Token kullanımı stratejisi:
  // - Eğer verify-otp'den token alındıysa, onu kullan
  // - Yoksa, DEFAULT_TOKEN'i kullan
  USE_DEFAULT_TOKEN: true,
} as const;

