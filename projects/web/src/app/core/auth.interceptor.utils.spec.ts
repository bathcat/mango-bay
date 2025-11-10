import { describe, it, expect } from 'vitest';
import { HttpRequest } from '@angular/common/http';
import { shouldAttemptTokenRefresh, createAuthHeader, addAuthHeadersToRequest, applyAuthToken } from './auth.interceptor.utils';

describe('Auth Interceptor Utils', () => {
  describe('shouldAttemptTokenRefresh', () => {
    it('should return true for 401 error on non-auth URLs', () => {
      expect(shouldAttemptTokenRefresh(401, '/api/users')).toBe(true);
      expect(shouldAttemptTokenRefresh(401, '/api/posts/123')).toBe(true);
      expect(shouldAttemptTokenRefresh(401, '/data/profile')).toBe(true);
    });

    it('should return false for 401 error on auth URLs', () => {
      expect(shouldAttemptTokenRefresh(401, '/auth/login')).toBe(false);
      expect(shouldAttemptTokenRefresh(401, '/auth/refresh')).toBe(false);
      expect(shouldAttemptTokenRefresh(401, '/api/auth/register')).toBe(false);
      expect(shouldAttemptTokenRefresh(401, 'https://example.com/auth/signin')).toBe(false);
    });

    it('should return false for non-401 error status codes', () => {
      expect(shouldAttemptTokenRefresh(400, '/api/users')).toBe(false);
      expect(shouldAttemptTokenRefresh(403, '/api/users')).toBe(false);
      expect(shouldAttemptTokenRefresh(404, '/api/users')).toBe(false);
      expect(shouldAttemptTokenRefresh(500, '/api/users')).toBe(false);
      expect(shouldAttemptTokenRefresh(200, '/api/users')).toBe(false);
    });

    it('should be case-sensitive when checking auth URLs', () => {
      expect(shouldAttemptTokenRefresh(401, '/AUTH/login')).toBe(true);
      expect(shouldAttemptTokenRefresh(401, '/api/auth/refresh')).toBe(false);
    });

    it('should handle edge cases', () => {
      expect(shouldAttemptTokenRefresh(401, '')).toBe(true);
      expect(shouldAttemptTokenRefresh(0, '/api/users')).toBe(false);
    });
  });

  describe('createAuthHeader', () => {
    it('should create Bearer token header when token exists', () => {
      const token = 'test-access-token-123';
      const header = createAuthHeader(token);
      
      expect(header).toEqual({ Authorization: 'Bearer test-access-token-123' });
    });

    it('should return empty object when token is null', () => {
      const header = createAuthHeader(null);
      
      expect(header).toEqual({});
    });

    it('should return empty object when token is empty string', () => {
      const header = createAuthHeader('');
      
      expect(header).toEqual({});
    });

    it('should handle various token formats', () => {
      expect(createAuthHeader('simple-token')).toEqual({ Authorization: 'Bearer simple-token' });
      expect(createAuthHeader('token.with.dots')).toEqual({ Authorization: 'Bearer token.with.dots' });
      expect(createAuthHeader('very-long-jwt-token-with-multiple-segments')).toEqual({ 
        Authorization: 'Bearer very-long-jwt-token-with-multiple-segments' 
      });
    });
  });

  describe('addAuthHeadersToRequest', () => {
    it('should add auth headers to request when headers are provided', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      const authHeaders = { Authorization: 'Bearer test-token' };
      
      const result = addAuthHeadersToRequest(mockRequest, authHeaders);
      
      expect(result.headers.get('Authorization')).toBe('Bearer test-token');
      expect(result).not.toBe(mockRequest);
    });

    it('should return original request when auth headers are empty', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      const emptyHeaders = {};
      
      const result = addAuthHeadersToRequest(mockRequest, emptyHeaders);
      
      expect(result).toBe(mockRequest);
      expect(result.headers.has('Authorization')).toBe(false);
    });

    it('should handle multiple headers', () => {
      const mockRequest = new HttpRequest('POST', '/api/data', {});
      const headers = { 
        Authorization: 'Bearer token-123',
        'X-Custom-Header': 'custom-value'
      };
      
      const result = addAuthHeadersToRequest(mockRequest, headers);
      
      expect(result.headers.get('Authorization')).toBe('Bearer token-123');
      expect(result.headers.get('X-Custom-Header')).toBe('custom-value');
    });

    it('should preserve existing headers when adding auth headers', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      const authHeaders = { Authorization: 'Bearer test-token' };
      
      const result = addAuthHeadersToRequest(mockRequest, authHeaders);
      
      expect(result.headers.get('Authorization')).toBe('Bearer test-token');
    });
  });

  describe('applyAuthToken', () => {
    it('should apply token to request when token exists', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      const token = 'test-token-123';
      
      const result = applyAuthToken(token, mockRequest);
      
      expect(result.headers.get('Authorization')).toBe('Bearer test-token-123');
    });

    it('should return original request when token is null', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      
      const result = applyAuthToken(null, mockRequest);
      
      expect(result).toBe(mockRequest);
      expect(result.headers.has('Authorization')).toBe(false);
    });

    it('should return original request when token is empty', () => {
      const mockRequest = new HttpRequest('GET', '/api/users');
      
      const result = applyAuthToken('', mockRequest);
      
      expect(result).toBe(mockRequest);
      expect(result.headers.has('Authorization')).toBe(false);
    });

    it('should work with different HTTP methods', () => {
      const getRequest = new HttpRequest('GET', '/api/users');
      const postRequest = new HttpRequest('POST', '/api/data', {});
      const token = 'my-token';
      
      const getResult = applyAuthToken(token, getRequest);
      const postResult = applyAuthToken(token, postRequest);
      
      expect(getResult.headers.get('Authorization')).toBe('Bearer my-token');
      expect(postResult.headers.get('Authorization')).toBe('Bearer my-token');
    });
  });
});

