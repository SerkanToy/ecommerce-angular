import { Routes } from '@angular/router';
import { Home } from './home/home';
import { Notfound } from './notfound/notfound';
import { Play } from './play/play';

export const routes: Routes = [
    {
        path:'',component: Home
    },
    {
        path:'play',component: Play
    },
    {
        path:'account', loadChildren: () => import('./account/routes').then(r => r.accountRoute)
    },
    {
        path:'notfound', component: Notfound
    },
    {
        path:'**', component: Notfound, pathMatch: 'full'
    }
];
