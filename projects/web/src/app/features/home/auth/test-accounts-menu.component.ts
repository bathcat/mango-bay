import { Component, output, viewChild } from '@angular/core';
import { MatMenu, MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { FillButtonComponent } from '../../../shared/ui/fill-button.component';

const password = 'One2three';

const testAccounts = [
  { role: 'Customer', email: 'jbloggs@goomail.com', password },
  { role: 'Pilot', email: 'baloo@mangobaycargo.com', password },
  { role: 'Administrator', email: 'admin@mangobaycargo.com', password }
];

@Component({
  selector: 'mbc-test-accounts-menu',
  standalone: true,
  imports: [MatMenuModule, MatButtonModule, MatIconModule, MatChipsModule, FillButtonComponent],
  template: `
<mat-menu #menu="matMenu" class="test-accounts-menu">
  <div class="test-accounts-content">
    <h3 class="test-accounts-title">Test Accounts</h3>
    <table class="test-accounts-table">
      <thead>
        <tr>
          <th>Role</th>
          <th>Email</th>
          <th>Password</th>
          <th>Fill</th>
        </tr>
      </thead>
      <tbody>
        @for (account of accounts; track account.email) {
          <tr>
            <td>
              <mat-chip-set>
                <mat-chip [class]="'role-' + account.role.toLowerCase()">
                  {{ account.role }}
                </mat-chip>
              </mat-chip-set>
            </td>
            <td>
              <span class="credential-text">{{ account.email }}</span>
            </td>
            <td>
              <span class="credential-text">{{ account.password }}</span>
            </td>
            <td>
              <mbc-fill-button
                ariaLabel="Fill credentials"
                tooltip="Fill credentials"
                (click)="emitFill(account.email, account.password)">
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
export class TestAccountsMenuComponent {
  fill = output<{ email: string; password: string }>();
  menu = viewChild<MatMenu>('menu');

  accounts = testAccounts;

  emitFill(email: string, password: string): void {
    this.fill.emit({ email, password });
  }
}


