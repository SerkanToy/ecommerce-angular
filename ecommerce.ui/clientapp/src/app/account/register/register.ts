import { Component, inject, OnInit } from '@angular/core';
import { AsyncValidatorFn, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account.service';
import { CommonModule } from '@angular/common';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';
import { SharedService } from '../../shared/shared.service';
import { map, of, switchMap, timer } from 'rxjs';

@Component({
  selector: 'app-register',
  imports: [ ReactiveFormsModule, 
            ValidationMessage, 
            CommonModule
   ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register implements OnInit {
  form: FormGroup = new FormGroup({});
  submitted = false;
  errormessage: string[] = [];

  constructor(private formBuilder: FormBuilder,
              private router: Router,
              private activatedRoute: ActivatedRoute,
              private accountService: AccountService,
              private sharedService: SharedService
  ) {
      if(this.accountService.$user())
      {
        this.router.navigateByUrl("/");
      }
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.form = this.formBuilder.group({
      email: ['', [Validators.required],[this.checkNameNotToken()],[this.checkEmailotToken()]],
      password: ['', Validators.required],
      lastName: ['', Validators.required],
      firstName: ['', Validators.required]
    });
  }

  register() {    
    if (this.form.valid) {
      this.accountService.register(this.form.value).subscribe({
        next: (response:any) => {
          this.sharedService.showNotification(response.message);
          this.router.navigateByUrl("/");
        },
        error: error => {
          if(error.errors)
          {
              this.errormessage = error.errors;
              
          }
        }
      });
    }
  }

  private checkNameNotToken(): AsyncValidatorFn {
    return control => {
      return timer(500).pipe(
        switchMap(_ => {
          if(!control.value)
          {
            return of(null);
          }
          return this.accountService.checkNameTaken(control.value).pipe(
            map((res: any) => {
              if(res && res.isToken)
              {
                return { nameToken: true };
              }
              return null;
            })
          );
        })
      )
    }
  }

  private checkEmailotToken(): AsyncValidatorFn {
    return control => {
      return timer(500).pipe(
        switchMap(_ => {
          if(!control.value)
          {
            return of(null);
          }
          return this.accountService.checkEmailTaken(control.value).pipe(
            map((res: any) => {
              if(res && res.isToken)
              {
                return { emailToken: true };
              }
              return null;
            })
          );
        })
      )
    }
  }

}
