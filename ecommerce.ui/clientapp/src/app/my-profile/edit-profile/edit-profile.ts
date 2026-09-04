import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ValidationMessage } from '../../shared/components/validationmessage/validationmessage';
import { FormInput } from '../../shared/components/form-input/form-input';
import { MyProfileService } from '../my-profile.service';
import { AccountService } from '../../account/account.service';
import { SharedService } from '../../shared/shared.service';
import { Router } from '@angular/router';
import { ApiResponse } from '../../shared/models/apiRespose';
import { UserModel } from '../../shared/models/account/user_model';
import { UserProfilModel } from '../../shared/models/profil/profil_model';

@Component({
  selector: 'app-edit-profile',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ValidationMessage,
    FormInput
  ],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfile implements OnInit {
  myProfile: UserProfilModel | undefined;
  form: FormGroup = new FormGroup({});
  submitted = false;
  editMode = false;
  errorMessages: string[] = [];

  constructor(private formBuilder: FormBuilder,
    private myProfileService: MyProfileService,
    private accountService: AccountService,
    private sharedService: SharedService,
    private router: Router
  ) {

  }

  ngOnInit(): void {
    this.getMyProfile();
  }

  initializeForm() {
    if (this.myProfile) {
      this.form = this.formBuilder.group({
        name: [{ value: `${this.myProfile.firstName} ${this.myProfile.lastName}` , disabled: true }, [Validators.required, Validators.minLength(3), Validators.maxLength(16), Validators.pattern('^[a-zA-Z][a-zA-Z0-9]*$')]],
        email: [{ value: this.myProfile.email, disabled: true }, [Validators.required, Validators.pattern('^.+@[^\\.].*\\.[a-z]{2,}$')]],
        currentPassword: [{ value: '', disabled: true }, [Validators.required]]
      })
    }
  }

  edit() {
    this.editMode = true;
    this.form.controls['name'].enable();
    this.form.controls['email'].enable();
    this.form.controls['currentPassword'].enable();
  }

  cancel() {
    this.editMode = false;
    this.submitted = false;
    this.errorMessages = [];

    this.initializeForm();
    this.form.markAsPristine();
  }

  save() {
    this.submitted = true;
    this.errorMessages = [];
    const ms = 'You have updated your email address. If you save, you will be logged out and must confirm your new email before signing in again. Would you like to continue?';
    if (this.form.valid && this.myProfile) {
      const isEmailChanged = this.myProfile.email !== this.form.get('email')?.value.trim().toLowerCase();
      if (isEmailChanged) {
        this.sharedService.confirmBox("")
          .subscribe((result: boolean) => {
            if (result) {
              // the user has clicked on Yes
              this.proceedSaving(isEmailChanged);
            }
          });

      } else {
        this.proceedSaving(isEmailChanged);
      }
    }
  }


  private proceedSaving(isEmailChanged: boolean) {
    this.myProfileService.editMyProfile(this.form.value).subscribe({
      next: (response: ApiResponse<UserProfilModel>) => {
        if (isEmailChanged) {
          this.sharedService.showNotification(response);
          this.myProfileService.setEditUser(response);
          this.router.navigateByUrl('/account/confirm-email?email=' + this.form.controls['email'].value);
        }else {
          this.sharedService.showNotification(response);
          this.myProfileService.setEditUser(response);
          this.getMyProfile();
          this.cancel();
        }
      }, error: error => {
        if (error) {
          if (error.errors) {
            this.errorMessages = error.errors;
          }
        }
      }
    });
  }

  private getMyProfile() {
    this.myProfileService.getMyProfile().subscribe({
      next:response => {
        this.myProfile = response.data;
        this.initializeForm();
      }
    });
  }

}
