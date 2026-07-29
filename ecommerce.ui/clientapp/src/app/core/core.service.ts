import { inject, Injectable } from '@angular/core';
import { AccountService } from '../account/account.service';
import { of, switchMap } from 'rxjs';
import { AutStatusModel } from '../shared/models/account/user_model';
import { ApiResponse } from '../shared/models/apiRespose';

@Injectable({
  providedIn: 'root',
})
export class CoreService {
  private accountService = inject(AccountService);

  initializeApp(){
    return this.accountService.autStatus().pipe(
      switchMap((res: ApiResponse<AutStatusModel>) => {
        if(res.data && res.data.isAuthenticated)
        {
          return this.accountService.refreshUser();
        }
        else
        {
          return of(null);
        }
      })
    );
  }

}
