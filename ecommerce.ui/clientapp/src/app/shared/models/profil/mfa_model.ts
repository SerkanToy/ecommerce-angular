import { EditMyProfileModel } from "./editmyprofilemodel";

export interface QrCodeModel {
    secret: string;
    uri: string;
}

export interface MfaEnableModel extends EditMyProfileModel {
    secret: string;
    code: string;
}