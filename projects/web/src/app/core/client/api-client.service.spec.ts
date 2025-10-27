import { describe, it, expect, vi, beforeEach } from 'vitest';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ApiClient } from './api-client.service';
import { API_CONFIG } from '../api.config';
import { z } from 'zod';

const TestRequestSchema = z.object({
  name: z.string(),
  age: z.number(),
});

const TestResponseSchema = z.object({
  id: z.string(),
  message: z.string(),
});

describe('ApiClient', () => {
  let service: ApiClient;
  let httpClient: HttpClient;
  let mockHttpClient: any;

  beforeEach(() => {
    mockHttpClient = {
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        ApiClient,
        { provide: HttpClient, useValue: mockHttpClient },
        { provide: API_CONFIG, useValue: { baseUrl: 'http://test-api.com' } },
      ],
    });

    service = TestBed.inject(ApiClient);
    httpClient = TestBed.inject(HttpClient);
  });

  describe('request method', () => {
    it('should make GET request with correct URL and params', () => {
      const mockResponse = { id: '123', message: 'success' };
      mockHttpClient.get.mockReturnValue(of(mockResponse));

      const result$ = service['request']({
        method: 'GET',
        endpoint: '/test',
        responseSchema: TestResponseSchema,
        params: { skip: '0', take: '10' },
      });

      result$.subscribe(result => {
        expect(result).toEqual(mockResponse);
        expect(mockHttpClient.get).toHaveBeenCalledWith(
          'http://test-api.com/test',
          { params: expect.any(Object) }
        );
      });
    });

    it('should make POST request with body and validation', () => {
      const mockRequest = { name: 'John', age: 30 };
      const mockResponse = { id: '123', message: 'created' };
      mockHttpClient.post.mockReturnValue(of(mockResponse));

      const result$ = service['request']({
        method: 'POST',
        endpoint: '/test',
        body: mockRequest,
        responseSchema: TestResponseSchema,
        requestSchema: TestRequestSchema,
      });

      result$.subscribe(result => {
        expect(result).toEqual(mockResponse);
        expect(mockHttpClient.post).toHaveBeenCalledWith(
          'http://test-api.com/test',
          mockRequest,
          { params: undefined }
        );
      });
    });

    it('should validate request body and throw error for invalid data', () => {
      const invalidRequest = { name: 'John' }; // missing age

      const result$ = service['request']({
        method: 'POST',
        endpoint: '/test',
        body: invalidRequest,
        responseSchema: TestResponseSchema,
        requestSchema: TestRequestSchema,
      });

      result$.subscribe({
        error: (error) => {
          expect(error.message).toContain('Request validation failed');
        },
      });
    });

    it('should validate response and throw error for invalid response', () => {
      const mockRequest = { name: 'John', age: 30 };
      const invalidResponse = { id: '123' }; // missing message
      mockHttpClient.post.mockReturnValue(of(invalidResponse));

      const result$ = service['request']({
        method: 'POST',
        endpoint: '/test',
        body: mockRequest,
        responseSchema: TestResponseSchema,
        requestSchema: TestRequestSchema,
      });

      result$.subscribe({
        error: (error) => {
          expect(error.message).toContain('Response validation failed');
        },
      });
    });

    it('should handle HTTP errors and propagate them', () => {
      const mockRequest = { name: 'John', age: 30 };
      const httpError = new HttpErrorResponse({
        status: 500,
        statusText: 'Internal Server Error',
      });
      mockHttpClient.post.mockReturnValue(throwError(() => httpError));

      const result$ = service['request']({
        method: 'POST',
        endpoint: '/test',
        body: mockRequest,
        responseSchema: TestResponseSchema,
        requestSchema: TestRequestSchema,
      });

      result$.subscribe({
        error: (error) => {
          expect(error).toBe(httpError);
        },
      });
    });

    it('should handle PUT and DELETE methods', () => {
      const mockResponse = { id: '123', message: 'updated' };
      mockHttpClient.put.mockReturnValue(of(mockResponse));
      mockHttpClient.delete.mockReturnValue(of(mockResponse));

      const putResult$ = service['request']({
        method: 'PUT',
        endpoint: '/test/123',
        body: { name: 'John', age: 30 },
        responseSchema: TestResponseSchema,
        requestSchema: TestRequestSchema,
      });

      const deleteResult$ = service['request']({
        method: 'DELETE',
        endpoint: '/test/123',
        responseSchema: TestResponseSchema,
      });

      putResult$.subscribe(result => {
        expect(mockHttpClient.put).toHaveBeenCalled();
      });

      deleteResult$.subscribe(result => {
        expect(mockHttpClient.delete).toHaveBeenCalled();
      });
    });
  });
});
