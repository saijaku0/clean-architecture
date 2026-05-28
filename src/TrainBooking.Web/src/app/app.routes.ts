import { Routes } from '@angular/router';
import { HomePage } from './pages/home-page/home-page';
import { MyReservationList } from './components/my-reservation-list/my-reservation-list';

export const routes: Routes = [
    { path: '', component: HomePage },
    { path: 'my-account', component: MyReservationList }
];
