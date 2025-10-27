import { Component, input, output, effect, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavigationService } from '@app/core/routing/navigation.service';

@Component({
  selector: 'mbc-search-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
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
          placeholder="Search docs"
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
export class SearchInputComponent {
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  
  private navigationService = inject(NavigationService);
  
  initialQuery = input<string>('');
  query = this.initialQuery();

  constructor() {
    effect(() => {
      this.query = this.initialQuery();
    });
  }

  onSearch() {
    const trimmedQuery = this.query.trim();
    if (trimmedQuery) {
      this.navigationService.navigateToSearch(trimmedQuery);
      this.query = '';
      this.searchInput.nativeElement.blur();
    }
  }
}
