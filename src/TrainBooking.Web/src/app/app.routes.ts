import { Routes } from '@angular/router';
import { MyAccountComponent } from './components/my-account-component/my-account-component';
import { HomePage } from './pages/home-page/home-page';

export const routes: Routes = [
    { path: '', component: HomePage },
    { path: 'my-account', component: MyAccountComponent }
];
