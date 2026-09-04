import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';

const excludedUrls = ['/auth/login', '/auth/register', '/auth/refresh-token'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);

  const accessToken = tokenService.getAccessToken();

  if (accessToken) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${accessToken}` }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall = excludedUrls.some((url) => req.url.includes(url));

      if (error.status === 401 && !isAuthCall && authService.isAuthenticated) {
        return authService.refreshToken().pipe(
          switchMap(({ accessToken: newAccessToken }) =>
            next(
              req.clone({
                setHeaders: { Authorization: `Bearer ${newAccessToken}` }
              })
            )
          ),
          catchError((refreshError) => {
            tokenService.clearTokens();
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};