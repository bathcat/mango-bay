import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'mbc-cargo-details',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h3>Cargo Details</h3>
    <div class="mbc-detail-grid">
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Description:</span>
        <span class="mbc-detail-value">{{ cargoDescription() }}</span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Weight:</span>
        <span class="mbc-detail-value">{{ cargoWeightKg() }} kg</span>
      </div>
    </div>
  `
})
export class CargoDetailsComponent {
  cargoDescription = input.required<string>();
  cargoWeightKg = input.required<number>();
}

