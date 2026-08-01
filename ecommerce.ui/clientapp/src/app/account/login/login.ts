import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account.service';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    ValidationMessage
],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  form: FormGroup = new FormGroup({});
  submitted = false;
  errorMessage: string[] = [];
  returnUrl: string | null = null;

  constructor(private formBuilder: FormBuilder,
          private router: Router,
          private activatedRoute: ActivatedRoute,
          private accountService: AccountService
  ){
    if(this.accountService.$user())
    {
      this.router.navigateByUrl('/');
    }
    else
    {
      this.activatedRoute.queryParamMap.subscribe({
        next: (params: any) => {
          if(params){
            this.returnUrl = params.get('returnUrl');
          }
        }
      });
    }
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.form = this.formBuilder.group({
      email:['',Validators.required],
      password:['',Validators.required]
    });
  }

  

  login(){    
    console.log("Tıklandı...");
    if (this.form.valid) {
      this.accountService.login(this.form.value).subscribe({
        next: _ => {                
          if(this.returnUrl)
          {                
            this.router.navigateByUrl(this.returnUrl);          
          }
          else
          {        
            this.router.navigateByUrl("/");
          }        
        },
        error: error => {
          console.log(error);        
        }
      });
    }
    //this.button.set(true);
  }
}
