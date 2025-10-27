import { Component, input, output, effect, inject, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { CreditCardInfo, CreditCardInfoSchema } from '@app/shared/schemas';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'mbc-credit-card-info',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
],
  templateUrl: './credit-card-info.component.html',
  styleUrl: './credit-card-info.component.scss',
})
export class CreditCardInfoComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();

  value = input<CreditCardInfo>();
  valueChange = output<CreditCardInfo>();

  cardForm = this.fb.group({
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{4}\s\d{4}\s\d{4}\s\d{4}$/)]],
    expiration: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/), this.futureDateValidator]],
    cvc: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
    cardholderName: ['', [Validators.required, Validators.minLength(3)]],
  }, { validators: [this.zodValidator] });

  constructor() {
    effect(() => {
      const initialValue = this.value();
      if (initialValue) {
        this.cardForm.patchValue({
          ...initialValue,
          cardNumber: this.formatCardNumber(initialValue.cardNumber),
        }, { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.cardForm.statusChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.cardForm.valid) {
          const formValue = this.cardForm.value;
          this.valueChange.emit({
            cardNumber: formValue.cardNumber!.replace(/\s/g, ''),
            expiration: formValue.expiration!,
            cvc: formValue.cvc!,
            cardholderName: formValue.cardholderName!,
          });
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onCardNumberInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const formatted = this.formatCardNumber(input.value.replace(/\s/g, ''));
    this.cardForm.patchValue({ cardNumber: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  onExpirationInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const formatted = this.formatExpiration(input.value.replace(/\D/g, ''));
    this.cardForm.patchValue({ expiration: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  onCvcInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.replace(/\D/g, '').substring(0, 4);
    this.cardForm.patchValue({ cvc: value }, { emitEvent: false });
    input.value = value;
  }

  private formatCardNumber(value: string): string {
    const cleaned = value.substring(0, 16);
    return cleaned.match(/.{1,4}/g)?.join(' ') || cleaned;
  }

  private formatExpiration(value: string): string {
    if (value.length >= 2) {
      return value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    return value;
  }

  private futureDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }

    const match = control.value.match(/^(\d{2})\/(\d{2})$/);
    if (!match) {
      return null;
    }

    const month = parseInt(match[1], 10);
    const year = 2000 + parseInt(match[2], 10);

    const expiration = new Date(year, month - 1);
    const now = new Date();
    now.setDate(1);

    if (expiration < now) {
      return { expiredCard: true };
    }

    return null;
  }

  private zodValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    
    if (!value.cardNumber || !value.expiration || !value.cvc || !value.cardholderName) {
      return null;
    }

    const creditCardData = {
      cardNumber: value.cardNumber.replace(/\s/g, ''),
      expiration: value.expiration,
      cvc: value.cvc,
      cardholderName: value.cardholderName,
    };

    const result = CreditCardInfoSchema.safeParse(creditCardData);
    
    if (!result.success) {
      return { zodValidation: result.error.errors };
    }

    return null;
  }

  getCardNumberError(): string {
    const control = this.cardForm.get('cardNumber');
    if (control?.hasError('required')) {
      return 'Card number is required';
    }
    if (control?.hasError('pattern')) {
      return 'Card number must be 16 digits';
    }
    return '';
  }

  getExpirationError(): string {
    const control = this.cardForm.get('expiration');
    if (control?.hasError('required')) {
      return 'Expiration date is required';
    }
    if (control?.hasError('pattern')) {
      return 'Format: MM/YY';
    }
    if (control?.hasError('expiredCard')) {
      return 'Card has expired';
    }
    return '';
  }

  getCvcError(): string {
    const control = this.cardForm.get('cvc');
    if (control?.hasError('required')) {
      return 'CVC is required';
    }
    if (control?.hasError('pattern')) {
      return 'CVC must be 3-4 digits';
    }
    return '';
  }

  getCardholderNameError(): string {
    const control = this.cardForm.get('cardholderName');
    if (control?.hasError('required')) {
      return 'Cardholder name is required';
    }
    if (control?.hasError('minlength')) {
      return 'Name must be at least 3 characters';
    }
    return '';
  }
}
