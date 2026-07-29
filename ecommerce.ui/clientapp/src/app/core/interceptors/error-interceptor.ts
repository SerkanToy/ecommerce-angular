import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SharedService } from '../../shared/shared.service';
import { AccountService } from '../../account/account.service';
import { catchError } from 'rxjs';
import { ApiResponse } from '../../shared/models/apiRespose';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);
  const sharedService = inject(SharedService);
  const accountService = inject(AccountService);
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error) {
        let apiResponse: ApiResponse<any>;
        apiResponse = error.error;

        if (error.status === 400) {
          if (apiResponse.displayByDefault) {
            if (apiResponse.showWithToastr) {
              toastr.error(apiResponse.message);
            } else {
              sharedService.showNotification(apiResponse);
            }
          }
        }

        if (error.status === 401) {
          sharedService.showNotification(apiResponse);
        }

        if (error.status === 403) {
          sharedService.showNotification(apiResponse);
          accountService.logout();
        }

        if (error.status === 404) {
          router.navigateByUrl('/notfound')
        }

        if (error.status === 400) {
          for (let index = 0; index < apiResponse.errors.length; index++) {
            toastr.error(apiResponse.errors[index])
          }
        }

        throw apiResponse;
      }
      throw error;
    })
  );
};
