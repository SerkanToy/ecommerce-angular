import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';
import { FormInput } from '../../shared/components/form-input/form-input';
import { MyProfileService } from '../my-profile.service';
import { SharedService } from '../../shared/shared.service';
import { matchValues } from '../../shared/sharedHelper';
import { CodeInput } from "../../shared/components/code-input/code-input/code-input";

@Component({
  selector: 'app-change-password',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ValidationMessage,
    FormInput,
    CodeInput
],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
})
export class ChangePassword implements OnInit {
  form: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];

  constructor(private formBuilder: FormBuilder,
    private myProfileService: MyProfileService,
    private sharedService: SharedService
  )
  {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.submitted = false;
    this.form = this.formBuilder.group({
      currentPassword:['',[Validators.required]],
      newPassword:['',[Validators.required, Validators.minLength(6), Validators.maxLength(15)]],
      confirmNewPassword:['',[Validators.required,matchValues('newPassword')]],
    });
    this.form.controls['newPassword'].valueChanges.subscribe({
      next: () => this.form.controls['confirmNewPassword'].updateValueAndValidity()
    });
  }

  save(){
    this.submitted = true;
    this.errorMessages = [];
    if(this.form.valid)
    {
      this.myProfileService.changePassword(this.form.value).subscribe({
        next: response => {
          this.sharedService.showNotification(response);
          this.initializeForm();
        },
        error: error => {
          if(error){
            if(error.errors)
            {
              this.errorMessages = error.errors;
            }
          }
          this.initializeForm()
        }
      });
    }
  }
}
