import { Injectable, signal } from '@angular/core';

const ACCESS_TOKEN_KEY = 'taskManagement.accessToken';
const REFRESH_TOKEN_KEY = 'taskManagement.refreshToken';

export interface JwtPayload {
  sub: string;
  email: string;
  role?: string;
  [key: string]: unknown;
}

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly accessToken = signal<string | null>(this.read(ACCESS_TOKEN_KEY));
  private readonly refreshToken = signal<string | null>(this.read(REFRESH_TOKEN_KEY));

  readonly accessTokenValue = this.accessToken.asReadonly();
  readonly refreshTokenValue = this.refreshToken.asReadonly();

  get isAuthenticated(): boolean {
    return !!this.accessToken();
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    this.accessToken.set(accessToken);
    this.refreshToken.set(refreshToken);
  }

  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.accessToken.set(null);
    this.refreshToken.set(null);
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  getRefreshToken(): string | null {
    return this.refreshToken();
  }

  getRole(): string | null {
    const token = this.accessToken();
    if (!token) {
      return null;
    }

    const payload = this.decodeToken(token);
    if (!payload) {
      return null;
    }

    return (
      (payload.role as string | undefined) ??
      (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string | undefined) ??
      null
    );
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const encodedPayload = token.split('.')[1];
      const base64 = encodedPayload.replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(atob(base64)) as JwtPayload;
    } catch {
      return null;
    }
  }

  private read(key: string): string | null {
    return localStorage.getItem(key);
  }
}