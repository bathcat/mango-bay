import { Delivery, Pilot, Site, Payment, Review } from '@app/shared/schemas';

export interface DeliveryDetailInfo {
  delivery: Delivery;
  origin: Site;
  destination: Site;
  pilot: Pilot;
  payment: Payment | null;
  review: Review | null;
  proofImageDataUrl: string | null;
}
