import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DeliveryStatusComponent } from '../delivery-status.component';
import { DeliveryDetailsStateService } from '@app/features/customer/deliveries/delivery-details-state.service';
import { CreateReviewComponent } from '../create-review.component';
import { EditReviewComponent } from '../edit-review.component';
import { PaymentInfoComponent } from '../../../../shared/delivery/payment-info.component';
import { RouteInfoComponent } from '../../../../shared/delivery/route-info.component';
import { CargoDetailsComponent } from '../../../../shared/delivery/cargo-details.component';
import { ScheduleInfoComponent } from '../../../../shared/delivery/schedule-info.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ReviewCardComponent } from '@app/shared/delivery/review/review-card.component';

@Component({
  selector: 'mbc-delivery-detail',
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
  templateUrl: './delivery-detail.component.html',
})
export class DeliveryDetailComponent {
  readonly state = inject(DeliveryDetailsStateService);
  private dialog = inject(MatDialog);

  openCreateReviewDialog(deliveryId: string): void {
    const dialogRef = this.dialog.open(CreateReviewComponent, {
      data: { deliveryId },
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.state.refresh();
      }
    });
  }

  openEditReviewDialog(review: any): void {
    const dialogRef = this.dialog.open(EditReviewComponent, {
      data: { review },
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.state.refresh();
      }
    });
  }
}
