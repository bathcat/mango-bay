import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DeliveryStatusComponent } from '../../../customer/deliveries/delivery-status.component';
import { DeliveryDetailsStateService } from '@app/features/customer/deliveries/delivery-details-state.service';
import { CompleteDeliveryComponent } from '../complete-delivery/complete-delivery.component';
import { PaymentInfoComponent } from '../../../../shared/delivery/payment-info.component';
import { RouteInfoComponent } from '../../../../shared/delivery/route-info.component';
import { CargoDetailsComponent } from '../../../../shared/delivery/cargo-details.component';
import { ScheduleInfoComponent } from '../../../../shared/delivery/schedule-info.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ReviewCardComponent } from '@app/shared/delivery/review/review-card.component';

@Component({
  selector: 'mbc-assignment-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatDialogModule,
    DeliveryStatusComponent,
    ReviewCardComponent,
    PaymentInfoComponent,
    RouteInfoComponent,
    CargoDetailsComponent,
    ScheduleInfoComponent,
    LoadableDirective,
  ],
  providers: [DeliveryDetailsStateService],
  templateUrl: './assignment-detail.component.html',
})
export class AssignmentDetailComponent {
  readonly state = inject(DeliveryDetailsStateService);
  private dialog = inject(MatDialog);

  openCompleteDeliveryDialog(deliveryId: string): void {
    const dialogRef = this.dialog.open(CompleteDeliveryComponent, {
      data: { deliveryId },
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.state.refresh();
      }
    });
  }
}
