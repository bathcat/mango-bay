import { Delivery } from '@app/shared/schemas';

export interface AssignmentInfo extends Delivery {
  originName: string;
  destinationName: string;
  pilotName: string;
}
