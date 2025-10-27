import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

const RICH_TEXT_AREA_CONSTANTS = {
  MAX_LENGTH: 5000,
  EDITOR_HEIGHT_PX: 200,
  EDITOR_LINE_HEIGHT_PX: 1.5,
  TOOLBAR_CONFIG: [
    ['bold', 'italic'],
    ['link'],
    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
  ] as any,
};

@Component({
  selector: 'mbc-rich-text-area',
  standalone: true,
  imports: [CommonModule, FormsModule, QuillModule, MatSlideToggleModule],
  template: `
    @if (isWysiwygMode()) {
      <quill-editor
        [ngModel]="model()"
        (ngModelChange)="onContentChange($event)"
        [modules]="quillConfig"
        [maxLength]="constants.MAX_LENGTH">
      </quill-editor>
    } @else {
      <textarea
        class="raw-html-editor"
        [value]="model()"
        (input)="onContentChange($any($event.target).value)"
        [maxLength]="constants.MAX_LENGTH"
        placeholder="Enter HTML content...">
      </textarea>
    }
    
    <div class="editor-controls">
      <mat-slide-toggle
        [checked]="isWysiwygMode()"
        (change)="isWysiwygMode.set($event.checked)"
        color="primary">
        Wysiwyg
      </mat-slide-toggle>
      <div class="character-count">
        {{ currentLength }} / {{ constants.MAX_LENGTH }} characters
      </div>
    </div>
  `,
})
export class RichTextAreaComponent {
  public readonly constants = RICH_TEXT_AREA_CONSTANTS;
  
  model = input<string>('');
  modelChange = output<string>();
  
  isWysiwygMode = signal(true);

  public readonly quillConfig = {
    toolbar: this.constants.TOOLBAR_CONFIG,
  };

  onContentChange(value: string): void {
    this.modelChange.emit(value || '');
  }

  get currentLength(): number {
    return this.model()?.length || 0;
  }

  get remainingLength(): number {
    return this.constants.MAX_LENGTH - this.currentLength;
  }

  get isAtLimit(): boolean {
    return this.currentLength >= this.constants.MAX_LENGTH;
  }
}
