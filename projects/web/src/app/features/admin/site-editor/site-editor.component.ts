import { Component, input, output, effect } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Site, CreateOrUpdateSiteRequest, Location, SiteStatus } from '@app/shared/schemas';
import { LocationEditorComponent } from '@app/features/customer/booking/location-editor.component';
import { SiteStatusPickerComponent } from '@app/features/admin/site-status-picker.component';
import { EditConfirmDialogComponent } from '@app/shared/ui/edit-confirm-dialog.component';

export interface SiteEditorData {
  mode: 'create' | 'edit';
  initialData?: Site;
  isLoading: boolean;
  error?: string;
}

@Component({
  selector: 'mbc-site-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    LocationEditorComponent,
    SiteStatusPickerComponent,
    EditConfirmDialogComponent,
  ],
  templateUrl: './site-editor.component.html',
  styleUrl: './site-editor.component.scss',
})
export class SiteEditorComponent {
  data = input.required<SiteEditorData>();
  submit = output<CreateOrUpdateSiteRequest>();
  cancel = output<void>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      notes: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(500)]],
      island: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      location: [null as Location | null, Validators.required],
      status: [null as SiteStatus | null, Validators.required],
    });

    effect(() => {
      const dataValue = this.data();
      if (dataValue.initialData) {
        this.form.patchValue({
          name: dataValue.initialData.name,
          notes: dataValue.initialData.notes,
          island: dataValue.initialData.island,
          address: dataValue.initialData.address,
          location: dataValue.initialData.location,
          status: dataValue.initialData.status,
        });
      }
    });
  }


  onLocationChange(location: Location | null): void {
    this.form.patchValue({ location });
  }

  onStatusChange(status: SiteStatus | null): void {
    this.form.patchValue({ status });
  }

  get canProceed(): boolean {
    return this.form.valid;
  }

  onSubmit(): void {
    if (this.canProceed) {
      const request: CreateOrUpdateSiteRequest = {
        name: this.form.value.name,
        notes: this.form.value.notes,
        island: this.form.value.island,
        address: this.form.value.address,
        location: this.form.value.location,
        status: this.form.value.status,
      };
      this.submit.emit(request);
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  getErrorMessage(field: 'name' | 'notes' | 'island' | 'address'): string {
    const control = this.form.get(field);
    if (!control || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return 'This field is required';
    }
    if (control.errors['minlength']) {
      return 'This field cannot be empty';
    }
    if (control.errors['maxlength']) {
      const max = control.errors['maxlength'].requiredLength;
      return `Cannot exceed ${max} characters`;
    }
    return '';
  }
}

