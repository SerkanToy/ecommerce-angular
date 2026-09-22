import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { MyProfileModel } from '../shared/models/profil/myprofilemodel';
import { ApiResponse } from '../shared/models/apiRespose';
import { EditMyProfileModel } from '../shared/models/profil/editmyprofilemodel';
import { ChangePasswordModel } from '../shared/models/profil/changepasswordmodel ';
import { DeleteAccountModel } from '../shared/models/profil/deleteaccountmodel';
import { UserModel } from '../shared/models/account/user_model';
import { UserProfilModel } from '../shared/models/profil/profil_model';
import { MfaEnableModel, QrCodeModel } from '../shared/models/profil/mfa_model';
import { EditProfileBaseModel } from '../shared/models/profil/editprofilebasemodel';

@Injectable({
  providedIn: 'root',
})
export class MyProfileService {
  apiUrl = environment.apiUrl;
  $userEdit = signal<ApiResponse<UserProfilModel> | null>(null);
  constructor(private http:HttpClient){}
  
  getMyProfile(){
    return this.http.get<ApiResponse<UserProfilModel>>(this.apiUrl+'profil/my-profile');
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


  public setEditUser(user: ApiResponse<UserProfilModel>) {
    this.$userEdit.set(user);
  }

  mfaStatus() {
    return this.http.get(this.apiUrl + 'myProfile/mfa-status');
  }

  getQrCode() {
    return this.http.get<QrCodeModel>(this.apiUrl + 'myProfile/qr-code');
  }

  mfaEnable(model: MfaEnableModel) {
    return this.http.put<ApiResponse<any>>(this.apiUrl + 'myProfile/mfa-enable', model);
  }

  mfaDisable(model: EditProfileBaseModel) {
    return this.http.put<ApiResponse<any>>(this.apiUrl + 'myProfile/mfa-disable', model);
  }

}
