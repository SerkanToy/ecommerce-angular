import { EditProfileBaseModel } from "./editprofilebasemodel";

export interface QrCodeModel {
    secret: string;
    uri: string;
}

export interface MfaEnableModel extends EditProfileBaseModel {
    secret: string;
    code: string;
}