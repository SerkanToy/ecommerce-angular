export interface UserModel {
    name:string
    jwt:string
    mfaToken:string
}

export interface AutStatusModel {
    isAuthenticated: boolean
}