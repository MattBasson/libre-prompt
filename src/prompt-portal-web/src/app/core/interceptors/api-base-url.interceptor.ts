import { HttpInterceptorFn } from '@angular/common/http';

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  // In development, the proxy handles routing to the API.
  // In production, this could prepend a base URL from environment config.
  return next(req);
};
