import { FormGroup, AbstractControl } from '@angular/forms';
import { CostEstimate, CreditCardInfo } from '@app/shared/schemas';

export function validateDifferentLocations(group: FormGroup): { [key: string]: any } | null {
  const originId = group.get('originId')?.value;
  const destinationId = group.get('destinationId')?.value;
  
  if (originId && destinationId && originId === destinationId) {
    return { sameLocation: true };
  }
  
  return null;
}

export function validateFutureDate(group: FormGroup): { [key: string]: any } | null {
  const scheduledFor = group.get('scheduledFor')?.value;
  
  if (scheduledFor) {
    const selectedDate = new Date(scheduledFor);
    selectedDate.setHours(0, 0, 0, 0);
    
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(0, 0, 0, 0);
    
    if (selectedDate < tomorrow) {
      return { pastDate: true };
    }
  }
  
  return null;
}

export function getFieldError(control: AbstractControl | null, fieldName: string): string {
  if (control?.hasError('required')) {
    return `${getFieldLabel(fieldName)} is required`;
  }
  if (control?.hasError('minlength')) {
    return `${getFieldLabel(fieldName)} must be at least ${control.errors?.['minlength'].requiredLength} characters`;
  }
  if (control?.hasError('min')) {
    return `${getFieldLabel(fieldName)} must be at least ${control.errors?.['min'].min}`;
  }
  if (control?.hasError('max')) {
    return `${getFieldLabel(fieldName)} cannot exceed ${control.errors?.['max'].max}`;
  }
  return '';
}

export function getFieldLabel(fieldName: string): string {
  const labels: { [key: string]: string } = {
    originId: 'Origin',
    destinationId: 'Destination',
    pilotId: 'Pilot',
    cargoDescription: 'Cargo description',
    cargoWeightKg: 'Weight',
    scheduledFor: 'Scheduled date',
  };
  return labels[fieldName] || fieldName;
}

export function convertExpirationToIsoDate(expiration: string): string {
  const [month, year] = expiration.split('/');
  const fullYear = 2000 + parseInt(year, 10);
  const monthNum = parseInt(month, 10);
  
  const lastDayOfMonth = new Date(fullYear, monthNum, 0);
  return lastDayOfMonth.toISOString().split('T')[0];
}

export function getMinDate(): Date {
  const minDate = new Date();
  minDate.setDate(minDate.getDate() + 1);
  minDate.setHours(0, 0, 0, 0);
  return minDate;
}

export function canProceedToEstimate(form: FormGroup): boolean {
  return form.valid && !form.hasError('sameLocation') && !form.hasError('pastDate');
}

export function canProceedToBooking(
  creditCardInfo: CreditCardInfo | null,
  costEstimate: CostEstimate | null
): boolean {
  return creditCardInfo !== null && costEstimate !== null;
}

export function hasSameOriginDestination(form: FormGroup): boolean {
  return form.hasError('sameLocation');
}

export function hasInvalidDate(form: FormGroup): boolean {
  return form.hasError('pastDate');
}

