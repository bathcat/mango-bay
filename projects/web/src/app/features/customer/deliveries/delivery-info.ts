import { Delivery } from '@app/shared/schemas';

export interface DeliveryInfo extends Delivery {
  originName: string;
  destinationName: string;
  pilotName: string;
}
