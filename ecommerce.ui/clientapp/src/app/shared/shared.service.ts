import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ApiResponse } from './models/apiRespose';

@Injectable({
  providedIn: 'root',
})
export class SharedService {
  constructor(private toastr: ToastrService,private models: Notification){}

  showNotification(apiResponse: ApiResponse<any>, backdrop: boolean = false){
    let isSuccess = false;
    if(apiResponse.statusCode == 200 || apiResponse.statusCode == 201)
    {
      isSuccess = true;
    }
    if(apiResponse.showWithToastr)
    {
      if(isSuccess)
      {
        this.toastr.success(apiResponse.message,apiResponse.title);
      }
      else
      {
        this.toastr.error(apiResponse.message,apiResponse.title);
      }
    }
    const modalRef = this.models.onshow;
    

  }
}
