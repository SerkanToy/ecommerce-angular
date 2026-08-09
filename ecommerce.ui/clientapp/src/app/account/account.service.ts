import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from '../shared/models/account/login_model';
import { AutStatusModel, UserModel } from '../shared/models/account/user_model';
import { map } from 'rxjs';
import { ApiResponse } from '../shared/models/apiRespose';
import { RegisterUserModel } from '../shared/models/account/registeruser_model';
import { ResetPasswordModel } from '../shared/models/account/resetPassword_m';
import { ConfirmEmailModel, EmailModel } from '../shared/models/account/confirmEmail_m';


@Injectable({
  providedIn: 'root',
})
export class AccountService {
  apiUrl = environment.apiUrl;
  $user = signal<ApiResponse<UserModel> | null>(null);
  constructor(private http: HttpClient, private route: Router) {

  }

  autStatus() {
    return this.http.get<ApiResponse<AutStatusModel>>(`${environment.apiUrl}account/isauthenticated`);
  }

  refreshUser() {
    return this.http.get<ApiResponse<UserModel>>(`${environment.apiUrl}account/refresh-user`).pipe(map((user: ApiResponse<UserModel>) => {
          if(user)
          {
            this.setUser(user);
          }
        }));
  }

  logout() {
    return this.http.post<{}>(`${environment.apiUrl}account/logout`, {}).pipe(map(() => {
      this.$user.set(null);
      this.route.navigateByUrl("/");
    }));
  }

  register(model: RegisterUserModel) {
    return this.http.post<ApiResponse<RegisterUserModel>>(`${environment.apiUrl}account/register`, model).pipe(map((user: ApiResponse<RegisterUserModel>) => {
      if (user) {
        user.statusCode = 200;
        //this.setUser(user);
        //return user;
      }
    }));
  }

  checkNameTaken(name: string){
    return this.http.get(`${environment.apiUrl}account/name-taken?name=${name}`);
  }

  checkEmailTaken(name: string){
    return this.http.get(`${environment.apiUrl}account/email-taken?email=${name}`);
  }

  resetPassword(model: ResetPasswordModel) {
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}account/reset-password`, model);
  }

  confirmEmail(confirmemail: ConfirmEmailModel)
  {
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}account/confirm-email`, confirmemail);
  }

  login(model: LoginModel) {
    return this.http.post<ApiResponse<UserModel>>(`${environment.apiUrl}account/login`, model).pipe(map((user: ApiResponse<UserModel>) => {
      if (user) {
        this.setUser(user);
        //return user;
      }
    }));
  }

  resendConfirmationEmail(model: EmailModel){
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}account/resend-confirmation-email`, model);
  }

  forgotUsernameOrPassword(model: EmailModel){
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}account/forgot-username-or-password`, model);    
  }

  private setUser(user: ApiResponse<UserModel>) {
    this.$user.set(user);
  }
}
