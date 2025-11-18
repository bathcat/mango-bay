import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'mbc-rich-text-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [innerHTML]="trustedHtml()"></div>
  `,
})
export class RichTextDisplayComponent {
  content = input<string>('');

  trustedHtml = computed<SafeHtml>(() => {
    return this.sanitizer.bypassSecurityTrustHtml(this.content());
  });

  constructor(private sanitizer: DomSanitizer) {}
}
