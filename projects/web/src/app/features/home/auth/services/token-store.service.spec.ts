import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { TokenStoreService } from './token-store.service';

describe('TokenStoreService', () => {
  let service: TokenStoreService;
  let mockLocalStorage: any;

  beforeEach(() => {
    mockLocalStorage = {
      getItem: vi.fn(),
      setItem: vi.fn(),
      removeItem: vi.fn(),
    };

    Object.defineProperty(window, 'localStorage', {
      value: mockLocalStorage,
      writable: true,
    });

    TestBed.configureTestingModule({
      providers: [TokenStoreService],
    });

    service = TestBed.inject(TokenStoreService);
  });

  describe('token operations', () => {
    it('should store and retrieve access token', () => {
      const token = 'test-access-token-123';
      
      service.setAccessToken(token);
      
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('mbc_access_token', token);
      
      mockLocalStorage.getItem.mockReturnValue(token);
      const retrievedToken = service.getAccessToken();
      
      expect(retrievedToken).toBe(token);
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('mbc_access_token');
    });

    it('should store and retrieve refresh token', () => {
      const token = 'test-refresh-token-456';
      
      service.setRefreshToken(token);
      
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('mbc_refresh_token', token);
      
      mockLocalStorage.getItem.mockReturnValue(token);
      const retrievedToken = service.getRefreshToken();
      
      expect(retrievedToken).toBe(token);
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('mbc_refresh_token');
    });

    it('should return null when token does not exist', () => {
      mockLocalStorage.getItem.mockReturnValue(null);
      
      const accessToken = service.getAccessToken();
      const refreshToken = service.getRefreshToken();
      
      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
    });

    it('should clear all tokens', () => {
      service.clearTokens();
      
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('mbc_access_token');
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('mbc_refresh_token');
    });

    it('should check if tokens exist', () => {
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'mbc_access_token') return 'access-token';
        if (key === 'mbc_refresh_token') return 'refresh-token';
        return null;
      });
      
      const hasTokens = service.hasTokens();
      
      expect(hasTokens).toBe(true);
    });

    it('should return false when tokens do not exist', () => {
      mockLocalStorage.getItem.mockReturnValue(null);
      
      const hasTokens = service.hasTokens();
      
      expect(hasTokens).toBe(false);
    });

    it('should return false when only one token exists', () => {
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'mbc_access_token') return 'access-token';
        return null;
      });
      
      const hasTokens = service.hasTokens();
      
      expect(hasTokens).toBe(false);
    });
  });
});
