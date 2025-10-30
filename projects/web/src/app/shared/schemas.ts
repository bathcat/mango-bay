import { z } from 'zod';

export const PilotSchema = z.object({
  id: z.guid(),
  fullName: z.string(),
  shortName: z.string(),
  avatarUrl: z.string().nullable().optional(),
  bio: z.string(),
});

export const LocationSchema = z.object({
  x: z.number().int().min(0).max(255),
  y: z.number().int().min(0).max(255),
});

export const SiteStatusSchema = z.enum(['Upcoming', 'Current', 'Inactive']);

export const SiteSchema = z.object({
  id: z.guid(),
  name: z.string(),
  notes: z.string(),
  island: z.string(),
  address: z.string(),
  location: LocationSchema,
  status: SiteStatusSchema,
  imageUrl: z.string().nullable().optional(),
});

export const CreateOrUpdateSiteRequestSchema = z.object({
  name: z.string().min(1, { error: 'Name is required' }).max(100, { error: 'Name cannot exceed 100 characters' }),
  notes: z.string().min(1, { error: 'Notes are required' }).max(500, { error: 'Notes cannot exceed 500 characters' }),
  island: z.string().min(1, { error: 'Island is required' }).max(100, { error: 'Island cannot exceed 100 characters' }),
  address: z.string().min(1, { error: 'Address is required' }).max(200, { error: 'Address cannot exceed 200 characters' }),
  location: LocationSchema,
  status: SiteStatusSchema,
});

export const PageSchema = <T extends z.ZodTypeAny>(itemSchema: T) =>
  z.object({
    items: z.array(itemSchema),
    totalCount: z.number(),
    offset: z.number(),
    countRequested: z.number(),
    hasMore: z.boolean(),
  });

const PageShape = PageSchema(z.any());

export const SignUpRequestSchema = z.object({
  email: z.email(),
  password: z.string(),
  nickname: z.string(),
});

export const SignInRequestSchema = z.object({
  email: z.email(),
  password: z.string(),
});

export const RefreshTokenRequestSchema = z.object({
  refreshToken: z.string(),
});

export const UserRoleSchema = z.enum(['Customer', 'Pilot', 'Administrator']);

export const UserSchema = z.object({//TODO: Use an algebraic type here.
  id: z.guid(),
  email: z.string(),
  nickname: z.string().nullable(),
  role: UserRoleSchema,
  customerId: z.guid().nullable(),
  pilotId: z.guid().nullable(),
});

export const AuthResponseSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
  user: UserSchema,
});

export const CreditCardInfoSchema = z.object({
  cardNumber: z.string().regex(/^\d{16}$/, 'Card number must be exactly 16 digits'),
  expiration: z.string(),
  cvc: z.string().regex(/^\d{3,4}$/, 'CVC must be 3-4 digits'),
  cardholderName: z.string(),
});

export const DeliveryStatusSchema = z.enum(['Pending', 'Confirmed', 'InTransit', 'Delivered', 'Cancelled']);

export const JobDetailsSchema = z.object({
  originId: z.guid(),
  destinationId: z.guid(),
  cargoDescription: z.string().min(1, { error: 'Cargo description is required' }),
  cargoWeightKg: z.number().min(0.1, { error: 'Weight must be at least 0.1 kg' }).max(2000, { error: 'Weight cannot exceed 2000 kg' }),
  scheduledFor: z.string().min(1, { error: 'Scheduled date is required' }),
});

export const BookingRequestSchema = z.object({
  pilotId: z.guid(),
  details: JobDetailsSchema,
  creditCard: CreditCardInfoSchema,
});

export const CostEstimateSchema = z.object({
  totalCost: z.number(),
  baseRate: z.number(),
  distanceCost: z.number(),
  weightCost: z.number(),
  rushFee: z.number(),
  distance: z.number(),
});

export const PaymentStatusSchema = z.enum(['Pending', 'Completed', 'Failed', 'Refunded']);

export const PaymentSchema = z.object({
  id: z.guid(),
  deliveryId: z.guid(),
  amount: z.number(),
  status: PaymentStatusSchema,
  transactionId: z.string(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
});

export const RatingSchema = z.number().int().min(1).max(5);

export const ReviewSchema = z.object({
  id: z.guid(),
  pilotId: z.guid(),
  rating: RatingSchema,
  notes: z.string(),
  createdAt: z.coerce.date(),
});

export const CreateReviewRequestSchema = z.object({
  deliveryId: z.guid(),
  rating: RatingSchema,
  notes: z.string().max(2000, { error: 'Review notes cannot exceed 2000 characters' }),
});

export const UpdateReviewRequestSchema = z.object({
  rating: RatingSchema,
  notes: z.string().max(2000, { error: 'Review notes cannot exceed 2000 characters' }),
});

export const CustomerSchema = z.object({
  id: z.guid(),
  nickname: z.string(),
});

export const DeliverySchema = z.object({
  id: z.guid(),
  customerId: z.guid(),
  pilotId: z.guid(),
  originId: z.guid(),
  destinationId: z.guid(),
  scheduledFor: z.coerce.date(),
  completedOn: z.coerce.date().nullable().optional(),
  status: DeliveryStatusSchema,
  cargoDescription: z.string(),
  cargoWeightKg: z.number(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  paymentId: z.guid(),
});

export const DeliveryProofSchema = z.object({
  id: z.guid(),
  deliveryId: z.guid(),
  pilotId: z.guid(),
  customerId: z.guid(),
  imagePath: z.string(),
  createdAt: z.coerce.date(),
});

export type Pilot = z.infer<typeof PilotSchema>;
export type Location = z.infer<typeof LocationSchema>;
export type SiteStatus = z.infer<typeof SiteStatusSchema>;
export type Site = z.infer<typeof SiteSchema>;
export type CreateOrUpdateSiteRequest = z.infer<typeof CreateOrUpdateSiteRequestSchema>;
type PageCommon = z.infer<typeof PageShape>;
export type Page<T> = Omit<PageCommon, 'items'> & { items: T[] };
export type SignUpRequest = z.infer<typeof SignUpRequestSchema>;
export type SignInRequest = z.infer<typeof SignInRequestSchema>;
export type RefreshTokenRequest = z.infer<typeof RefreshTokenRequestSchema>;
export type UserRole = z.infer<typeof UserRoleSchema>;
export type User = z.infer<typeof UserSchema>;
export type AuthResponse = z.infer<typeof AuthResponseSchema>;
export type CreditCardInfo = z.infer<typeof CreditCardInfoSchema>;
export type DeliveryStatus = z.infer<typeof DeliveryStatusSchema>;
export type JobDetails = z.infer<typeof JobDetailsSchema>;
export type BookingRequest = z.infer<typeof BookingRequestSchema>;
export type CostEstimate = z.infer<typeof CostEstimateSchema>;
export type Delivery = z.infer<typeof DeliverySchema>;
export type PaymentStatus = z.infer<typeof PaymentStatusSchema>;
export type Payment = z.infer<typeof PaymentSchema>;
export type Customer = z.infer<typeof CustomerSchema>;
export type Rating = z.infer<typeof RatingSchema>;
export type Review = z.infer<typeof ReviewSchema>;
export type CreateReviewRequest = z.infer<typeof CreateReviewRequestSchema>;
export type UpdateReviewRequest = z.infer<typeof UpdateReviewRequestSchema>;
export type DeliveryProof = z.infer<typeof DeliveryProofSchema>;

