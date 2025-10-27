import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Site, Pilot } from '@app/shared/schemas';

@Component({
  selector: 'mbc-route-info',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <h3>Route Information</h3>
    <div class="mbc-detail-grid">
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Origin:</span>
        <span class="mbc-detail-value">
          <a [routerLink]="['/sites', origin().id]" class="mbc-link">
            {{ origin().name }}
          </a>
        </span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Destination:</span>
        <span class="mbc-detail-value">
          <a [routerLink]="['/sites', destination().id]" class="mbc-link">
            {{ destination().name }}
          </a>
        </span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Pilot:</span>
        <span class="mbc-detail-value">
          <a [routerLink]="['/pilots', pilot().id]" class="mbc-link">
            {{ pilot().fullName }}
          </a>
        </span>
      </div>
    </div>
  `
})
export class RouteInfoComponent {
  origin = input.required<Site>();
  destination = input.required<Site>();
  pilot = input.required<Pilot>();
}

