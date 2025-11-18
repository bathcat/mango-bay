import { Component, output, viewChild } from '@angular/core';
import { MatMenu, MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { FillButtonComponent } from '../../../../shared/ui/fill-button.component';

const testCreditCards = [
  { 
    type: 'Good', 
    cardNumber: '1200000000000000',
    expiration: '12/29',
    cvc: '123',
    cardholderName: 'Joe Bloggs'
  },
  { 
    type: 'NSF', 
    cardNumber: '0000000000000000',
    expiration: '12/29',
    cvc: '987',
    cardholderName: 'Jon Doe'
  }
];

@Component({
  selector: 'mbc-test-credit-cards-menu',
  standalone: true,
  imports: [MatMenuModule, MatButtonModule, MatIconModule, MatChipsModule, FillButtonComponent],
  template: `
<mat-menu #menu="matMenu" class="test-credit-cards-menu">
  <div class="test-credit-cards-content">
    <h3 class="test-credit-cards-title">Test Credit Cards</h3>
    <table class="test-credit-cards-table">
      <thead>
        <tr>
          <th>Type</th>
          <th>Number</th>
          <th>Fill</th>
        </tr>
      </thead>
      <tbody>
        @for (card of cards; track card.cardNumber) {
          <tr>
            <td>
              <mat-chip-set>
                <mat-chip [class]="'card-' + card.type.toLowerCase()">
                  {{ card.type }}
                </mat-chip>
              </mat-chip-set>
            </td>
            <td>
              <span class="credential-text">{{ formatCardNumber(card.cardNumber) }}</span>
            </td>
            <td>
              <mbc-fill-button
                ariaLabel="Fill card details"
                tooltip="Fill card details"
                (click)="emitFill(card)">
              </mbc-fill-button>
            </td>
          </tr>
        }
      </tbody>
    </table>
  </div>
</mat-menu>
  `
})
export class TestCreditCardsMenuComponent {
  fill = output<{ cardNumber: string; expiration: string; cvc: string; cardholderName: string }>();
  menu = viewChild<MatMenu>('menu');

  cards = testCreditCards;

  formatCardNumber(cardNumber: string): string {
    return cardNumber.match(/.{1,4}/g)?.join(' ') || cardNumber;
  }

  emitFill(card: typeof testCreditCards[0]): void {
    this.fill.emit({
      cardNumber: card.cardNumber,
      expiration: card.expiration,
      cvc: card.cvc,
      cardholderName: card.cardholderName
    });
  }
}

