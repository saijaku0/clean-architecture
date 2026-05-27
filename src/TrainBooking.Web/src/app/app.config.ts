import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { authHttpInterceptorFn, provideAuth0 } from '@auth0/auth0-angular';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authHttpInterceptorFn])
    ),
    provideAuth0({
      domain: 'demian-trainbooking.eu.auth0.com',
      clientId: 'EQIyCDwCzhzRcqrMPZVyh1f7I76bnseh',
      authorizationParams: {
        redirect_uri: window.location.origin,
        audience: 'https://api.trainbooking.app'
      },
      httpInterceptor: {
        allowedList: [
          {
            uri: 'https://api.trainbooking.app/*',
            tokenOptions: {
              authorizationParams: {
                audience: 'https://api.trainbooking.app'
              }
            }
          }
        ]
      }
    })
  ]
};
