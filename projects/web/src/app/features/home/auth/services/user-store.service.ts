import { Injectable } from '@angular/core';
import { User } from '@app/shared/schemas';

@Injectable({
  providedIn: 'root',
})
export class UserStoreService {
  private readonly USER_KEY = 'mbc_user';

  getUser(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (!userJson) {
      return null;
    }
    try {
      return JSON.parse(userJson) as User;
    } catch {
      return null;
    }
  }

  setUser(user: User): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  clearUser(): void {
    localStorage.removeItem(this.USER_KEY);
  }
}


