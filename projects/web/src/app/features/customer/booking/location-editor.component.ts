import { Component, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Location } from '@app/shared/schemas';

@Component({
  selector: 'mbc-location-editor',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule],
  template: `
    <mat-form-field appearance="outline" class="coordinate-field">
      <mat-label>X Coordinate</mat-label>
      <input
        matInput
        type="number"
        [(ngModel)]="x"
        min="0"
        max="255"
        step="1"
      />
    </mat-form-field>

    <mat-form-field appearance="outline" class="coordinate-field">
      <mat-label>Y Coordinate</mat-label>
      <input
        matInput
        type="number"
        [(ngModel)]="y"
        min="0"
        max="255"
        step="1"
      />
    </mat-form-field>
  `,
})
export class LocationEditorComponent {
  value = model<Location | null>(null);

  get x(): number {
    return this.value()?.x ?? 0;
  }

  set x(val: number) {
    this.updateMaybe({ x: val });
  }

  get y(): number {
    return this.value()?.y ?? 0;
  }

  set y(val: number) {
    this.updateMaybe({ y: val });
  }

  private updateMaybe(partial: Partial<Location>): void {
    const current = this.value();
    const x = partial.x ?? current?.x ?? 0;
    const y = partial.y ?? current?.y ?? 0;
    if (this.isValid(x, y)) {
      this.value.set({ x, y });
    }
  }

  private isValid(x: number, y: number): boolean {
    return x >= 0 && x <= 255 && y >= 0 && y <= 255;
  }
}

