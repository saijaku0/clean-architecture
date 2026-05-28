import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '@auth0/auth0-angular';

interface ReservationItem {
  id: string;
  status: 'pending' | 'confirmed' | 'cancelled';
  statusLabel: string;
  timing: string;
  urgent: boolean;
  departureTime: string;
  arrivalTime: string;
  originStation: string;
  destinationStation: string;
  trainInfo: string;
  seats: string;
  price: number;
}

@Component({
  selector: 'app-my-reservation-list',
  imports: [AsyncPipe, MatIcon],
  templateUrl: './my-reservation-list.html',
  styleUrl: './my-reservation-list.scss',
})
export class MyReservationList {
  protected auth = inject(AuthService);

  reservations: ReservationItem[] = [
    {
      id: 'A7F3',
      status: 'pending',
      statusLabel: 'Pending payment',
      timing: 'Expires in 12:34',
      urgent: true,
      departureTime: '12:00',
      arrivalTime: '14:20',
      originStation: 'Berlin Hbf',
      destinationStation: 'Munich Hbf',
      trainInfo: 'ICE 705 · May 27',
      seats: '6A, 6B',
      price: 158,
    },
    {
      id: 'B2K9',
      status: 'confirmed',
      statusLabel: 'Confirmed',
      timing: 'In 4 days',
      urgent: false,
      departureTime: '08:30',
      arrivalTime: '10:15',
      originStation: 'Hamburg Hbf',
      destinationStation: 'Frankfurt Hbf',
      trainInfo: 'ICE 1023 · May 31',
      seats: '4C',
      price: 69,
    },
    {
      id: 'C5M1',
      status: 'confirmed',
      statusLabel: 'Confirmed',
      timing: 'In 11 days',
      urgent: false,
      departureTime: '17:45',
      arrivalTime: '21:30',
      originStation: 'Munich Hbf',
      destinationStation: 'Berlin Hbf',
      trainInfo: 'EC 173 · Jun 7',
      seats: '11A, 11B, 12A',
      price: 117,
    },
  ];

  login() {
    this.auth.loginWithRedirect();
  }
}
