import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from '../shared/models/account/login_model';
import { UserModel } from '../shared/models/account/user_model';
import { map } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class AccountService {
  apiUrl = environment.apiUrl;
  $user = signal<UserModel | null>(null);
  constructor(private http: HttpClient, private route: Router)
  {

  }

  login(model:LoginModel){    
    return this.http.post<UserModel>(`https://localhost:7011/account/login`,model,{
      withCredentials:true
    }).pipe(map((user:UserModel) => {
      if(user)
      {        
        this.setUser(user);
        //return user;
      }
    }));
  }

  private setUser(user:UserModel)
  {
    this.$user.set(user);
  }
}
