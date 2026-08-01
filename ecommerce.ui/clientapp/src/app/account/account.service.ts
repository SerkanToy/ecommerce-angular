import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from '../shared/models/account/login_model';
import { AutStatusModel, UserModel } from '../shared/models/account/user_model';
import { map } from 'rxjs';
import { ApiResponse } from '../shared/models/apiRespose';
import { RegisterUserModel } from '../shared/models/account/registeruser_model';


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


  login(model: LoginModel) {
    return this.http.post<ApiResponse<UserModel>>(`${environment.apiUrl}account/login`, model).pipe(map((user: ApiResponse<UserModel>) => {
      if (user) {
        this.setUser(user);
        //return user;
      }
    }));
  }

  private setUser(user: ApiResponse<UserModel>) {
    this.$user.set(user);
  }
}
