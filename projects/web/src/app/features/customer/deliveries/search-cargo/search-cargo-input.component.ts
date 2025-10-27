import { Component, output, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'mbc-search-cargo-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <div class="mbc-search-container">
      <div class="mbc-search-input-wrapper">
        <input
          #searchInput
          [(ngModel)]="query"
          (keyup.enter)="onSearch()"
          placeholder="Search deliveries"
          class="mbc-search-input"
        />
      </div>
      <button
        mat-icon-button
        color="primary"
        (click)="onSearch()"
        [disabled]="!query.trim()"
        class="mbc-search-button"
      >
        <mat-icon>search</mat-icon>
      </button>
    </div>
  `,
})
export class SearchCargoInputComponent {
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  
  query = '';
  search = output<string>();

  onSearch() {
    const trimmedQuery = this.query.trim();
    if (trimmedQuery) {
      this.search.emit(trimmedQuery);
      this.query = '';
      this.searchInput.nativeElement.blur();
    }
  }
}

