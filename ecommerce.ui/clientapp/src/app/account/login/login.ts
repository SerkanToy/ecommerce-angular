import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account.service';
import { Validationmessage } from '../../shared/components/validationmessage/validationmessage';
import { email } from '@angular/forms/signals';
import { NgIf } from "../../../../node_modules/@angular/common/types/_common_module-chunk";

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    Validationmessage,
    Validationmessage,
    NgIf
],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  form: FormGroup = new FormGroup({});
  submitted = false;
  errorMessage: string[] = [];
  returnUrl: string | null = null;
  button = false;

  constructor(private formBuilder: FormBuilder,
          private router: Router,
          private activatedRoute: ActivatedRoute,
          private accountService: AccountService
  ){

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
    console.log("Tıklandı...")
    this.button = true;
    this.accountService.login(this.form.value).subscribe({
      next: res => {
        console.log(res);
        this.button = false;
      },
      error: error => {
        console.log(error);
        this.button = false;
      }
    });
  }
}
