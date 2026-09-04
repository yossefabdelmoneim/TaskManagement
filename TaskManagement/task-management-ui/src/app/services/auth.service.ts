import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(
    private readonly http: HttpClient,
    private readonly tokenService: TokenService
  ) {}

  get isAuthenticated(): boolean {
    return this.tokenService.isAuthenticated;
  }

  get role(): string | null {
    return this.tokenService.getRole();
  }

  get isAdmin(): boolean {
    return this.role === 'Admin';
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, payload).pipe(
      tap((response) => this.tokenService.setTokens(response.accessToken, response.refreshToken))
    );
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, payload).pipe(
      tap((response) => this.tokenService.setTokens(response.accessToken, response.refreshToken))
    );
  }

  logout(): Observable<void> {
    const refreshToken = this.tokenService.getRefreshToken();

    if (!refreshToken) {
      this.tokenService.clearTokens();
      return new Observable<void>((subscriber) => subscriber.complete());
    }

    return this.http.post<void>(`${this.baseUrl}/logout`, { refreshToken }).pipe(
      tap(() => this.tokenService.clearTokens())
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenService.getRefreshToken();

    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }

    return this.http.post<AuthResponse>(`${this.baseUrl}/refresh-token`, { refreshToken }).pipe(
      tap((response) => this.tokenService.setTokens(response.accessToken, response.refreshToken))
    );
  }
}