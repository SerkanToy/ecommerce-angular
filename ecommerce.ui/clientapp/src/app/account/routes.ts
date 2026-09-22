import { Route } from "@angular/router";
import { Login } from "./login/login";
import { Register } from "./register/register";
import { SendEmail } from "./sendemail/sendemail";
import { ResetPassword } from "./reset-password/reset-password";
import { ConfirmEmail } from "./confirm-email/confirm-email";
import { MfaVerify } from "./mfa-verify/mfa-verify";

export const accountRoute : Route[] = [
    {
        path: 'login', component: Login,
    },
    {
        path: 'register', component: Register,
    },
    { 
        path: 'confirm-email', component: ConfirmEmail 
    },
    { 
        path: 'sendemail/:mode', component: SendEmail 
    },
    { 
        path: 'reset-password', component: ResetPassword 
    },
    { 
        path: 'mfa-verify', component: MfaVerify 
    }
]