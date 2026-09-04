import { HttpErrorResponse } from '@angular/common/http';

export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  timestamp: string;
}

export function getApiErrorMessage(err: unknown, fallback = 'Something went wrong.'): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) {
      return 'Unable to reach the server. Please try again.';
    }
    const body = err.error as ApiErrorResponse | undefined;
    if (body?.message) {
      return body.message;
    }
    return err.message || fallback;
  }
  if (err instanceof Error && err.message) {
    return err.message;
  }
  return fallback;
}