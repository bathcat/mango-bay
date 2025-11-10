import { Directive, Input, TemplateRef, ViewContainerRef, OnDestroy, ComponentRef } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { Loadable } from './loadable.types';
import { LoadingComponent } from './loading.component';
import { ErrorStateComponent } from './error-state.component';

interface LoadableContext<T> {
  $implicit: T;
  mbcLoadable: T;
}

@Directive({
  selector: '[mbcLoadable]',
  standalone: true
})
export class LoadableDirective<T> implements OnDestroy {
  private subscription?: Subscription;
  private loadingComponentRef?: ComponentRef<LoadingComponent>;
  private errorComponentRef?: ComponentRef<ErrorStateComponent>;

  @Input() errorMessage?: string;
  @Input() errorSubtitle?: string;

  constructor(
    private viewContainer: ViewContainerRef,
    private templateRef: TemplateRef<LoadableContext<T>>
  ) {}

  @Input()
  set mbcLoadable(observable: Observable<Loadable<T>>) {
    this.subscription?.unsubscribe();
    this.viewContainer.clear();
    this.loadingComponentRef = undefined;
    this.errorComponentRef = undefined;

    this.subscription = observable.subscribe(state => {
      this.viewContainer.clear();
      this.loadingComponentRef = undefined;
      this.errorComponentRef = undefined;

      switch (state.status) {
        case 'loading':
          this.loadingComponentRef = this.viewContainer.createComponent(LoadingComponent);
          break;

        case 'error':
          this.errorComponentRef = this.viewContainer.createComponent(ErrorStateComponent);
          if (this.errorMessage) {
            this.errorComponentRef.instance.message = this.errorMessage;
          }
          if (this.errorSubtitle) {
            this.errorComponentRef.instance.subtitle = this.errorSubtitle;
          } else {
            this.errorComponentRef.instance.subtitle = this.extractErrorMessage(state.error);
          }
          break;

        case 'loaded':
          this.viewContainer.createEmbeddedView(this.templateRef, {
            $implicit: state.value,
            mbcLoadable: state.value
          });
          break;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private extractErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    if (error?.message) {
      return error.message;
    }
    if (error?.error?.message) {
      return error.error.message;
    }
    if (error?.statusText) {
      return error.statusText;
    }
    return 'An unexpected error occurred';
  }

  static ngTemplateContextGuard<T>(
    directive: LoadableDirective<T>,
    context: unknown
  ): context is LoadableContext<T> {
    return true;
  }
}

