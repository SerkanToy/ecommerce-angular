import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from '../shared/models/account/login_model';
import { UserModel } from '../shared/models/account/user_model';


@Injectable({
  providedIn: 'root',
})
export class AccountService {
  apiUrl = environment.apiUrl;
  constructor(private http: HttpClient, private route: Router)
  {

  }

  login(model:LoginModel){
    
    return this.http.post<UserModel>(`https://localhost:7011/account/login`,model,{
      withCredentials:true
    });
  }
}
