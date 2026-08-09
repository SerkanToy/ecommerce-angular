import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CodeInput } from '../../shared/components/code-input/code-input/code-input';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';
import { AccountService } from '../account.service';
import { SharedService } from '../../shared/shared.service';
import { ResetPasswordModel } from '../../shared/models/account/resetPassword_m';
import { matchValues } from '../../shared/sharedHelper';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule,
    CodeInput,
    ReactiveFormsModule,
    ValidationMessage,
    RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})

export class ResetPassword {
  email: string | undefined;
  fullCode: string | undefined;
  form: FormGroup = new FormGroup({});
  errorMessages: string[] = [];

  constructor(private accountService: AccountService,
      private router: Router,
      private activatedRoute: ActivatedRoute,
      private sharedService: SharedService,
      private formBuilder: FormBuilder
  ){
    if(this.accountService.$user())
    {
      this.router.navigateByUrl('/');
    } else {
      this.activatedRoute.queryParamMap.subscribe({
        next: (params:any) => {
          const email = params.get('email');
          if(email)
          {
            this.email = email;
            this.initializeForm();
          }else{
            this.router.navigateByUrl('/account/login');
          }
        }
      });
    }
  }

  fullCodeReceive(code: string) {
    this.fullCode = code.replace(/ /g, '');
  }

  initializeForm(){
    this.form = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(15)]],
      confirmNewPassword: ['', [Validators.required, matchValues('newPassword')]]
    });
  }

  submit() {
    this.errorMessages = [];
    if(this.form.valid && this.email && this.fullCode && this.fullCode.length == 6)
    {
      this.accountService.resetPassword(
        new ResetPasswordModel(this.fullCode,this.email,this.form.get('newPassword')?.value))
        .subscribe({
          next: (response:any) => {
            this.sharedService.showNotification(response);
            this.router.navigateByUrl('/account/login');
          },
          error: (error:any) => {
            if(error.errors)
            {
              this.errorMessages = error.errors;
            }
            else
            {
              if(error && error.title.includes('Confirm your email first'))
              {
                this.router.navigateByUrl('/account/confirm-email?email=' + this.email);
              }
            }
          } 
        });
      
    }
  }

}
