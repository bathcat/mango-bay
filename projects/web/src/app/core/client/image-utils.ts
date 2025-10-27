import { Observable, of, throwError } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { ErrorService } from '../errors/error.service';
import { ErrorInfo } from '../errors/error.models';

/**
 * Converts a Blob to a base64 data URL
 */
export function asDataUrl(blob: Blob): Observable<string> {
  return new Observable<string>(subscriber => {
    const reader = new FileReader();
    
    reader.onload = () => {
      subscriber.next(reader.result as string);
      subscriber.complete();
    };
    
    reader.onerror = () => {
      subscriber.error(new Error('Failed to convert image to data URL'));
    };
    
    reader.readAsDataURL(blob);
  });
}

/**
 * Handles image loading errors with consistent error reporting
 */
export function handleImageError(
  error: any, 
  operation: string, 
  endpoint: string, 
  errorService: ErrorService
): Observable<null> {
  if (error.status === 404) {
    return of(null);
  }
  
  if (error.status) {
    const errorInfo = ErrorInfo.fromHttpError(error, operation, endpoint);
    errorService.report(errorInfo);
  } else if (error.name === 'HttpErrorResponse' || error.message?.includes('Http failure')) {
    const errorInfo = ErrorInfo.fromNetworkError(error, operation, endpoint);
    errorService.report(errorInfo);
  } else {
    const errorInfo = ErrorInfo.fromUnknown(error, operation, endpoint);
    errorService.report(errorInfo);
  }
  
  return throwError(() => error);
}
