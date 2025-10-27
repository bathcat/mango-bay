export interface ErrorContext {
  operation: string;
  endpoint?: string;
  requestId?: string;
  timestamp: Date;
  userAgent?: string;
}

export interface ErrorInfo {
  type: 'network' | 'validation' | 'business' | 'server';
  originalError: any;
  context: ErrorContext;
  userMessage: string;
  technicalDetails: string;
}

export class ErrorInfo {
  static fromNetworkError(error: any, operation: string, endpoint?: string): ErrorInfo {
    return {
      type: 'network',
      originalError: error,
      context: {
        operation,
        endpoint,
        timestamp: new Date(),
        userAgent: navigator.userAgent,
      },
      userMessage: 'Unable to connect to the server. Please check your internet connection.',
      technicalDetails: `Network error during ${operation}: ${error.message || error}`,
    };
  }

  static fromHttpError(error: any, operation: string, endpoint?: string): ErrorInfo {
    const status = error.status || error.statusCode || 'unknown';
    const statusText = error.statusText || error.message || 'Unknown error';
    
    let type: ErrorInfo['type'] = 'server';
    let userMessage = 'An unexpected error occurred. Please try again.';
    
    if (status >= 400 && status < 500) {
      type = 'business';
      if (status === 401) {
        userMessage = 'You are not authorized. Please sign in again.';
      } else if (status === 403) {
        userMessage = 'You do not have permission to perform this action.';
      } else if (status === 404) {
        userMessage = 'The requested resource was not found.';
      } else {
        userMessage = 'Invalid request. Please check your input and try again.';
      }
    }

    const problemDetails = error.error || error.body;
    const detail = problemDetails?.detail || problemDetails?.title || statusText;

    return {
      type,
      originalError: error,
      context: {
        operation,
        endpoint,
        timestamp: new Date(),
        userAgent: navigator.userAgent,
      },
      userMessage,
      technicalDetails: `HTTP ${status}: ${detail}`,
    };
  }

  static fromZodValidation(error: any, operation: string, endpoint?: string): ErrorInfo {
    const zodError = error.issues ? error : error.error;
    const issues = zodError.issues || [];
    
    const issueMessages = issues.map((issue: any) => 
      `${issue.path.join('.')}: ${issue.message}`
    ).join('; ');

    return {
      type: 'validation',
      originalError: error,
      context: {
        operation,
        endpoint,
        timestamp: new Date(),
        userAgent: navigator.userAgent,
      },
      userMessage: 'Data validation failed. Please check your input.',
      technicalDetails: `Validation error during ${operation}: ${issueMessages}`,
    };
  }

  static fromUnknown(error: any, operation: string, endpoint?: string): ErrorInfo {
    return {
      type: 'server',
      originalError: error,
      context: {
        operation,
        endpoint,
        timestamp: new Date(),
        userAgent: navigator.userAgent,
      },
      userMessage: 'An unexpected error occurred. Please try again.',
      technicalDetails: `Unknown error during ${operation}: ${error.message || error}`,
    };
  }
}