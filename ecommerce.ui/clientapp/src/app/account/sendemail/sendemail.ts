import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AccountService } from '../account.service';
import { SharedService } from '../../shared/shared.service';
import { EmailModel } from '../../shared/models/account/confirmEmail_m';

@Component({
  selector: 'app-sendemail',
  imports: [ ReactiveFormsModule,
    ValidationMessage,
    RouterLink],
  templateUrl: './sendemail.html',
  styleUrl: './sendemail.css',
})
export class SendEmail implements OnInit {
  form: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  mode: string | undefined;
  
  constructor(private formBuilder: FormBuilder,
    private accountService: AccountService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private sharedService: SharedService
  ) { 
    if(this.accountService.$user())
    {
      this.router.navigateByUrl('/');
    }
    else
    {
      const mode =this.activatedRoute.snapshot.paramMap.get('mode');
      if(mode && (mode.includes('resend-confirmation-email') || mode.includes('forgot-username-or-password')))
      {
        this.mode = mode;
      }
      else
      {
        this.router.navigateByUrl('/account/login');
      }
    }
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.form = this.formBuilder.group({
      email: ['',[Validators.required, Validators.pattern('^.+@[^\\.].*\\.[a-z]{2,}$')]]
    });
  }

  sendEmail(){
    this.submitted = true;
    this.errorMessages = [];
    if(this.form.valid && this.mode){
      const email = this.form.get('email')?.value;
      
      if(this.mode.includes('resend-confirmation-email')){
        this.accountService.resendConfirmationEmail(new EmailModel(email)).subscribe({
          next: (response:any) => {
            this.sharedService.showNotification(response);           
            this.router.navigateByUrl('/account/confirm-email?email=' + email);
          },
          error: (error:any) => {      
            if(error && error.title.includes('Email was confirmed')){
              this.router.navigateByUrl('/account/login');
            }
            else if(error)
            {
              
              this.errorMessages = error;
            }
          }
        });
      } 
      else
      {
        this.accountService.forgotUsernameOrPassword(new EmailModel(email)).subscribe({
          next: (response:any) => {
            console.log(email + "---- İçeride");
            this.sharedService.showNotification(response);
            this.router.navigateByUrl('/account/login');
          }, error: (error:any) => {
            console.log(email + "---- Hata");
            if (error && error.title.includes('Confirm your email first')) {
              this.router.navigateByUrl('/account/confirm-email?email=' + email);
            } else if (error) {
              this.errorMessages = error;
            }
          }
        });
      }
    }
  }


}
