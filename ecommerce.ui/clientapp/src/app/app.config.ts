import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideToastr } from 'ngx-toastr';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { CoreService } from './core/core.service';
import { lastValueFrom } from 'rxjs';
import { credentialInterceptor } from './core/interceptors/credential-interceptor';
import { errorInterceptor } from './core/interceptors/error-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true
    }),
    provideHttpClient(withInterceptors([credentialInterceptor, errorInterceptor])),
    provideAppInitializer(async () => {
      const coreService = inject(CoreService)
      return lastValueFrom(coreService.initializeApp()).finally(() => {
        
      })
    })
  ]
};
