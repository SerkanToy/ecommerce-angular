import { EditProfileBaseModel } from "./editprofilebasemodel";

export interface ChangePasswordModel extends EditProfileBaseModel {
    newPassword: string;
}