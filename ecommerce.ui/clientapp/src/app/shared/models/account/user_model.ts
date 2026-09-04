export interface UserModel {
    jwt: string
    name: string
    mfaToken:string
    email:string
}

export interface AutStatusModel {
    isAuthenticated: boolean
}