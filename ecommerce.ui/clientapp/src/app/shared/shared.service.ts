import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ApiResponse } from './models/apiRespose';
import { Observable, of, switchMap } from 'rxjs';
import { ConfirmBox } from './components/confirm-box/confirm-box';

@Injectable({
  providedIn: 'root',
})

export class SharedService {

  constructor(private toastr: ToastrService){}

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
    const modalRef = this.toastr.show;
  }

  confirmBox(message: string, backdrop: boolean = false): Observable<boolean> {
    const options: any = {
      backdrop
    };

    const modalRef = this.toastr.success("İşlem Başarılı", options);
    modalRef.message = message;

    return new Observable<boolean>((observable) => {
      modalRef.onShown.pipe(switchMap(_ => {
                if(modalRef.message.length <= 0)
                {
                  return of(null);
                }
                return modalRef.message
              }));
    })

    
}}
