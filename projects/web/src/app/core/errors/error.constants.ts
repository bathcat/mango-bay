export const ERROR_CONSTANTS = {
  TOAST_AUTO_DISMISS_MS: 10000,
  TECHNICAL_DETAILS_MAX_LINES: 3,
  TECHNICAL_DETAILS_MAX_CHARS: 200,
} as const;

export const ERROR_TYPE_LABELS = {
  network: 'Network Error',
  validation: 'Validation Error', 
  business: 'Request Error',
  server: 'Server Error',
} as const;

export const ERROR_TYPE_COLORS = {
  network: '#ff9800',
  validation: '#f44336',
  business: '#2196f3',
  server: '#9c27b0',
} as const;
