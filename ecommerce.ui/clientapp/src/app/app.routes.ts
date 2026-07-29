import { Routes } from '@angular/router';
import { Home } from './home/home';
import { Notfound } from './shared/components/errors/notfound/notfound';
import { Play } from './play/play';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
    {
        path: '', component: Home
    },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            {
                path: 'play', component: Play
            }
        ]
    },   
    {
        path: 'account', loadChildren: () => import('./account/routes').then(r => r.accountRoute)
    },
    {
        path: 'notfound', component: Notfound
    },
    {
        path: '**', component: Notfound, pathMatch: 'full'
    }
];
