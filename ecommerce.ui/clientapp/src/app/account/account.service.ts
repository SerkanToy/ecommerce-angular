import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from '../shared/models/account/login_model';
import { AutStatusModel, UserModel } from '../shared/models/account/user_model';
import { map } from 'rxjs';
import { ApiResponse } from '../shared/models/apiRespose';


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
