import { Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ErrorInfo } from './error.models';

@Injectable({
  providedIn: 'root',
})
export class ErrorService implements OnDestroy {
  private readonly errorSubject = new Subject<ErrorInfo>();
  
  public readonly error$: Observable<ErrorInfo> = this.errorSubject.asObservable();

  constructor() {}

  public report(error: ErrorInfo): void {
    this.log(error);
    this.errorSubject.next(error);
  }

  private log(error: ErrorInfo): void {
    console.group(`[ErrorService] ${error.type.toUpperCase()} ERROR`);
    console.error('User Message:', error.userMessage);
    console.error('Technical Details:', error.technicalDetails);
    console.error('Original Error:', error.originalError);
    console.error('Context:', error.context);
    console.groupEnd();
  }

  ngOnDestroy(): void {
    this.errorSubject.complete();
  }
}
