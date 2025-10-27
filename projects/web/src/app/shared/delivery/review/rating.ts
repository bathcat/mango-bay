import { Branded } from '@app/shared/branded';
import { RatingSchema } from '@app/shared/schemas';



export type Rating = Branded<number, 'Rating'>;

export const isRating = (value: unknown): value is Rating =>
  RatingSchema.safeParse(value).success;

export const Rating = (value: number): Rating => {
  return RatingSchema.parse(value) as Rating;
};
