import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, of, switchMap, throwError } from 'rxjs';
import { z } from 'zod';
import { API_CONFIG } from '../api.config';
import { API_ENDPOINTS } from './api-endpoints';
import { ErrorService } from '../errors/error.service';
import { ErrorInfo } from '../errors/error.models';
import { asDataUrl, handleImageError } from './image-utils';
import { isSchemaNullable } from './schema-utils';
import {
  AuthWebResponse,
  AuthWebResponseSchema,
  SignInRequest,
  SignInRequestSchema,
  SignUpRequest,
  SignUpRequestSchema,
  Pilot,
  PilotSchema,
  Page,
  PageSchema,
  Site,
  SiteSchema,
  CreateOrUpdateSiteRequest,
  CreateOrUpdateSiteRequestSchema,
  BookingRequest,
  BookingRequestSchema,
  Delivery,
  DeliverySchema,
  JobDetails,
  JobDetailsSchema,
  CostEstimate,
  CostEstimateSchema,
  Payment,
  PaymentSchema,
  Review,
  ReviewSchema,
  CreateReviewRequest,
  CreateReviewRequestSchema,
  UpdateReviewRequest,
  UpdateReviewRequestSchema,
  Rating,
  DeliveryProof,
  DeliveryProofSchema,
} from '@app/shared/schemas';

interface RequestConfig<TReq, TRes> {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  endpoint: string;
  responseSchema: z.ZodSchema<TRes>;
  requestSchema?: z.ZodSchema<TReq>;
  body?: TReq;
  params?: Record<string, string | number>;
}

const NoContentSchema = z.unknown();

@Injectable({
  providedIn: 'root',
})
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);
  private readonly errorService = inject(ErrorService);

  signUp = (request: SignUpRequest): Observable<AuthWebResponse> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.auth.signUp(),
      body: request,
      responseSchema: AuthWebResponseSchema,
      requestSchema: SignUpRequestSchema,
    });

  signIn = (request: SignInRequest): Observable<AuthWebResponse> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.auth.signIn(),
      body: request,
      responseSchema: AuthWebResponseSchema,
      requestSchema: SignInRequestSchema,
    });

  refreshToken = (): Observable<AuthWebResponse> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.auth.refresh(),
      responseSchema: AuthWebResponseSchema,
    });

  signOut = (): Observable<unknown> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.auth.signOut(),
      responseSchema: NoContentSchema,
    });

  getPilots = (skip: number, take: number): Observable<Page<Pilot>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.pilots.list(),
      responseSchema: PageSchema(PilotSchema),
      params: { skip: skip.toString(), take: take.toString() },
    });

  getPilotById = (id: string): Observable<Pilot> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.pilots.get(id),
      responseSchema: PilotSchema,
    });

  getSites = (skip: number, take: number): Observable<Page<Site>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.sites.list(),
      responseSchema: PageSchema(SiteSchema),
      params: { skip: skip.toString(), take: take.toString() },
    });

  getSiteById = (id: string): Observable<Site> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.sites.get(id),
      responseSchema: SiteSchema,
    });

  createSite = (request: CreateOrUpdateSiteRequest): Observable<Site> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.sites.create(),
      body: request,
      responseSchema: SiteSchema,
      requestSchema: CreateOrUpdateSiteRequestSchema,
    });

  updateSite = (id: string, request: CreateOrUpdateSiteRequest): Observable<Site> =>
    this.request({
      method: 'PUT',
      endpoint: API_ENDPOINTS.sites.update(id),
      body: request,
      responseSchema: SiteSchema,
      requestSchema: CreateOrUpdateSiteRequestSchema,
    });

  deleteSite = (id: string): Observable<unknown> =>
    this.request({
      method: 'DELETE',
      endpoint: API_ENDPOINTS.sites.delete(id),
      responseSchema: NoContentSchema,
    });

  uploadSiteImage = (siteId: string, file: File): Observable<string> => {
    const formData = new FormData();
    formData.append('file', file);

    const url = `${this.apiConfig.baseUrl}${API_ENDPOINTS.sites.uploadImage(siteId)}`;

    return this.http.post<string>(url, formData, { withCredentials: true }).pipe(
      map(response => {
        const validationResult = z.string().safeParse(response);
        if (!validationResult.success) {
          this.logValidationError(
            `Response from POST ${API_ENDPOINTS.sites.uploadImage(siteId)}`,
            validationResult.error
          );
          const errorInfo = ErrorInfo.fromZodValidation(
            validationResult.error,
            `POST ${API_ENDPOINTS.sites.uploadImage(siteId)}`,
            API_ENDPOINTS.sites.uploadImage(siteId)
          );
          this.errorService.report(errorInfo);
          throw new Error(`Response validation failed: ${validationResult.error.message}`);
        }
        return validationResult.data;
      }),
      catchError((error) => {
        const operation = `POST ${API_ENDPOINTS.sites.uploadImage(siteId)}`;
        const endpoint = API_ENDPOINTS.sites.uploadImage(siteId);

        if (error.status) {
          const errorInfo = ErrorInfo.fromHttpError(error, operation, endpoint);
          this.errorService.report(errorInfo);
        } else if (error.name === 'HttpErrorResponse' || error.message?.includes('Http failure')) {
          const errorInfo = ErrorInfo.fromNetworkError(error, operation, endpoint);
          this.errorService.report(errorInfo);
        } else {
          const errorInfo = ErrorInfo.fromUnknown(error, operation, endpoint);
          this.errorService.report(errorInfo);
        }

        return throwError(() => error);
      })
    );
  };

  calculateCost = (jobDetails: JobDetails): Observable<CostEstimate> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.deliveries.calculateCost(),
      body: jobDetails,
      responseSchema: CostEstimateSchema,
      requestSchema: JobDetailsSchema,
    });

  bookDelivery = (request: BookingRequest): Observable<Delivery> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.deliveries.create(),
      body: request,
      responseSchema: DeliverySchema,
      requestSchema: BookingRequestSchema,
    });

  getMyDeliveries = (skip: number, take: number): Observable<Page<Delivery>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.deliveries.myDeliveries(),
      responseSchema: PageSchema(DeliverySchema),
      params: { skip: skip.toString(), take: take.toString() },
    });

  getMyAssignments = (skip: number, take: number): Observable<Page<Delivery>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.deliveries.myAssignments(),
      responseSchema: PageSchema(DeliverySchema),
      params: { skip: skip.toString(), take: take.toString() },
    });

  getDeliveryById = (id: string): Observable<Delivery> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.deliveries.get(id),
      responseSchema: DeliverySchema,
    });

  searchDeliveries = (searchTerm: string, skip: number, take: number): Observable<Page<Delivery>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.deliveries.search(),
      responseSchema: PageSchema(DeliverySchema),
      params: { searchTerm, skip: skip.toString(), take: take.toString() },
    });

  getPayment = (id: string): Observable<Payment> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.payments.get(id),
      responseSchema: PaymentSchema,
    });

  getPaymentByDeliveryId = (deliveryId: string): Observable<Payment | null> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.payments.byDelivery(deliveryId),
      responseSchema: PaymentSchema.nullable(),
    });

  searchPaymentsByCardholderNames = (names: string[]): Observable<Payment[]> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.payments.searchByCardholders(),
      responseSchema: z.array(PaymentSchema),
      params: names.length > 0 ? { names: names.join(',') } : {},
    });

  uploadProofOfDelivery = (deliveryId: string, file: File): Observable<DeliveryProof> => {
    const formData = new FormData();
    formData.append('file', file);

    const url = `${this.apiConfig.baseUrl}${API_ENDPOINTS.proofs.upload(deliveryId)}`;

    return this.http.post<DeliveryProof>(url, formData, { withCredentials: true }).pipe(
      map(response => {
        const validationResult = DeliveryProofSchema.safeParse(response);
        if (!validationResult.success) {
          this.logValidationError(
            `Response from POST ${API_ENDPOINTS.proofs.upload(deliveryId)}`,
            validationResult.error
          );
          const errorInfo = ErrorInfo.fromZodValidation(
            validationResult.error,
            `POST ${API_ENDPOINTS.proofs.upload(deliveryId)}`,
            API_ENDPOINTS.proofs.upload(deliveryId)
          );
          this.errorService.report(errorInfo);
          throw new Error(`Response validation failed: ${validationResult.error.message}`);
        }
        return validationResult.data;
      }),
      catchError((error) => {
        const operation = `POST ${API_ENDPOINTS.proofs.upload(deliveryId)}`;
        const endpoint = API_ENDPOINTS.proofs.upload(deliveryId);

        if (error.status) {
          const errorInfo = ErrorInfo.fromHttpError(error, operation, endpoint);
          this.errorService.report(errorInfo);
        } else if (error.name === 'HttpErrorResponse' || error.message?.includes('Http failure')) {
          const errorInfo = ErrorInfo.fromNetworkError(error, operation, endpoint);
          this.errorService.report(errorInfo);
        } else {
          const errorInfo = ErrorInfo.fromUnknown(error, operation, endpoint);
          this.errorService.report(errorInfo);
        }

        return throwError(() => error);
      })
    );
  };

  getProofImageDataUrl = (deliveryId: string): Observable<string | null> => {
    const url = `${this.apiConfig.baseUrl}${API_ENDPOINTS.proofs.image(deliveryId)}`;
    const operation = `GET ${API_ENDPOINTS.proofs.image(deliveryId)}`;
    const endpoint = API_ENDPOINTS.proofs.image(deliveryId);

    return this.http.get(url, { responseType: 'blob', withCredentials: true }).pipe(
      switchMap(blob => asDataUrl(blob)),
      catchError(error => handleImageError(error, operation, endpoint, this.errorService))
    );
  };

  createReview = (deliveryId: string, rating: Rating, notes: string): Observable<Review> =>
    this.request({
      method: 'POST',
      endpoint: API_ENDPOINTS.reviews.create(),
      body: { deliveryId, rating, notes },
      responseSchema: ReviewSchema,
      requestSchema: CreateReviewRequestSchema,
    });

  getReviewByDeliveryId = (deliveryId: string): Observable<Review | null> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.reviews.byDelivery(deliveryId),
      responseSchema: ReviewSchema.nullable(),
    });

  getReviewsByPilotId = (pilotId: string, skip: number, take: number): Observable<Page<Review>> =>
    this.request({
      method: 'GET',
      endpoint: API_ENDPOINTS.reviews.byPilot(pilotId),
      responseSchema: PageSchema(ReviewSchema),
      params: { skip: skip.toString(), take: take.toString() },
    });

  updateReview = (reviewId: string, rating: Rating, notes: string): Observable<Review> =>
    this.request({
      method: 'PUT',
      endpoint: API_ENDPOINTS.reviews.update(reviewId),
      body: { rating, notes },
      responseSchema: ReviewSchema,
      requestSchema: UpdateReviewRequestSchema,
    });

  private request<TReq, TRes>(config: RequestConfig<TReq, TRes>): Observable<TRes> {
    const url = `${this.apiConfig.baseUrl}${config.endpoint}`;

    if (config.requestSchema && config.body !== undefined) {
      const validationResult = config.requestSchema.safeParse(config.body);
      if (!validationResult.success) {
        this.logValidationError(
          `Request to ${config.method} ${config.endpoint}`,
          validationResult.error
        );
        const errorInfo = ErrorInfo.fromZodValidation(
          validationResult.error,
          `${config.method} ${config.endpoint}`,
          config.endpoint
        );
        this.errorService.report(errorInfo);
        return throwError(() => new Error(`Request validation failed: ${validationResult.error.message}`));
      }
    }

    const httpParams = config.params
      ? new HttpParams({ fromObject: config.params })
      : undefined;

    const httpOptions = {
      params: httpParams,
      withCredentials: true,
    };

    let request$: Observable<unknown>;

    switch (config.method) {
      case 'GET':
        request$ = this.http.get(url, httpOptions);
        break;
      case 'POST':
        request$ = this.http.post(url, config.body, httpOptions);
        break;
      case 'PUT':
        request$ = this.http.put(url, config.body, httpOptions);
        break;
      case 'DELETE':
        request$ = this.http.delete(url, httpOptions);
        break;
    }

    return request$.pipe(
      map((response) => {
        const validationResult = config.responseSchema.safeParse(response);
        if (!validationResult.success) {
          this.logValidationError(
            `Response from ${config.method} ${config.endpoint}`,
            validationResult.error
          );
          const errorInfo = ErrorInfo.fromZodValidation(
            validationResult.error,
            `${config.method} ${config.endpoint}`,
            config.endpoint
          );
          this.errorService.report(errorInfo);
          throw new Error(`Response validation failed: ${validationResult.error.message}`);
        }
        return validationResult.data;
      }),
      catchError((error) => {
        const operation = `${config.method} ${config.endpoint}`;

        if (error.message?.startsWith('Response validation failed')) {
          return throwError(() => error);
        }

        if (error.status === 404 && isSchemaNullable(config.responseSchema)) {
          return of(null as TRes);
        }

        if (error.status) {
          const errorInfo = ErrorInfo.fromHttpError(error, operation, config.endpoint);
          this.errorService.report(errorInfo);
        } else if (error.name === 'HttpErrorResponse' || error.message?.includes('Http failure')) {
          const errorInfo = ErrorInfo.fromNetworkError(error, operation, config.endpoint);
          this.errorService.report(errorInfo);
        } else {
          const errorInfo = ErrorInfo.fromUnknown(error, operation, config.endpoint);
          this.errorService.report(errorInfo);
        }

        return throwError(() => error);
      })
    );
  }


  private logValidationError(context: string, error: z.ZodError): void {
    console.error(`[ApiClient] Validation failed in ${context}:`, {
      issues: error.issues,
      formattedErrors: error.issues.map((issue) => ({
        path: issue.path.join('.'),
        message: issue.message,
        code: issue.code,
      })),
    });
  }

}

