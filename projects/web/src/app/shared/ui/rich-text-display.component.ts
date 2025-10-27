import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'mbc-rich-text-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [innerHTML]="content()"></div>
  `,
})
export class RichTextDisplayComponent {
  content = input<string>('');
}
