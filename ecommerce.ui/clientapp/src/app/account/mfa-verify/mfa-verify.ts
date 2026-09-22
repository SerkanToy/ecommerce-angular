import { Component } from '@angular/core';
import { AccountService } from '../account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { MfaVerifyModel } from '../../shared/models/account/mfaVerify_model';

@Component({
  selector: 'app-mfa-verify',
  imports: [],
  templateUrl: './mfa-verify.html',
  styleUrl: './mfa-verify.css',
})
export class MfaVerify {
  mfaToken: string | undefined;
  fullCode: string | undefined;

  constructor(private accountService:AccountService,
      private router: Router,
      private activatedRoute: ActivatedRoute
  ){
    this.activatedRoute.queryParamMap.subscribe({
      next:(params:any) => {
        const mfaToken = params.get('mfaToken');
        if(mfaToken)
        {
          this.mfaToken = mfaToken;
        } else {
          this.router.navigateByUrl('/account/login');
        }
      }
    });
  }

  fullCodeReceive(code: string){
    this.fullCode = code.replace(/ /g,'');
  }

  submit(){
    if(this.mfaToken && this.fullCode && this.fullCode.length == 6){
      this.accountService.mfaVerify(new MfaVerifyModel(this.mfaToken, this.fullCode)).subscribe({
        next: _ => {
          this.router.navigateByUrl('/');
        }
      })
    }
  }

}
