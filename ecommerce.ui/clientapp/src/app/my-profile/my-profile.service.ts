import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { MyProfileModel } from '../shared/models/profil/myprofilemodel';
import { ApiResponse } from '../shared/models/apiRespose';
import { EditMyProfileModel } from '../shared/models/profil/editmyprofilemodel';
import { ChangePasswordModel } from '../shared/models/profil/changepasswordmodel ';
import { DeleteAccountModel } from '../shared/models/profil/deleteaccountmodel';
import { UserModel } from '../shared/models/account/user_model';

@Injectable({
  providedIn: 'root',
})
export class MyProfileService {
  apiUrl = environment.apiUrl;
  constructor(private http:HttpClient){}
  
  getMyProfile(){
    return this.http.get<ApiResponse<UserModel>>(this.apiUrl+'profil/my-profile');
  }

  editMyProfile(model:EditMyProfileModel){
    return this.http.put<ApiResponse<any>>(this.apiUrl+'profil/my-profile-update',model);
  }

  changePassword(model:ChangePasswordModel){
    return this.http.put<ApiResponse<any>>(this.apiUrl+'profil/change-password',model);
  }

  deleteAccount(model:DeleteAccountModel){
    return this.http.delete<ApiResponse<any>>(this.apiUrl+'profil/delete-account',{body:model});
  }
}
