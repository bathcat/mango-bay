import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'mbc-schedule-info',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h3>Schedule</h3>
    <div class="mbc-detail-grid">
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Scheduled For:</span>
        <span class="mbc-detail-value">{{ scheduledFor() | date:'fullDate' }}</span>
      </div>
      @if (completedOn()) {
        <div class="mbc-detail-item">
          <span class="mbc-detail-label">Completed On:</span>
          <span class="mbc-detail-value">{{ completedOn() | date:'medium' }}</span>
        </div>
      }
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Created:</span>
        <span class="mbc-detail-value">{{ createdAt() | date:'medium' }}</span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Last Updated:</span>
        <span class="mbc-detail-value">{{ updatedAt() | date:'medium' }}</span>
      </div>
    </div>
  `
})
export class ScheduleInfoComponent {
  scheduledFor = input.required<Date>();
  completedOn = input<Date | null | undefined>();
  createdAt = input.required<Date>();
  updatedAt = input.required<Date>();
}

