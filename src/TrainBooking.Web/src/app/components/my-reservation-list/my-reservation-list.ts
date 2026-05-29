import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '@auth0/auth0-angular';
import { ReservationStore } from '../../core/reservations/reservation.store';

@Component({
  selector: 'app-my-reservation-list',
  imports: [AsyncPipe, MatIcon],
  providers: [DatePipe],
  templateUrl: './my-reservation-list.html',
  styleUrl: './my-reservation-list.scss',
})
export class MyReservationList {
  protected auth = inject(AuthService);
  protected reservationStore = inject(ReservationStore); 
  login() {
    this.auth.loginWithRedirect();
  }

  ngOnInit() {
    this.reservationStore.loadReservations();
  }
}
