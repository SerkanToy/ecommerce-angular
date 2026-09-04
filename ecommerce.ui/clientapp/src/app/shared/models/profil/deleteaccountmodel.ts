import { EditProfileBaseModel } from "./editprofilebasemodel";

export interface DeleteAccountModel extends EditProfileBaseModel {
    currentUserName: string;
    confirmation: boolean;
}