import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { Observable } from 'rxjs';
import { ReservationDto } from './reservations.model';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/reservations/me`;

  getMyReservation(): Observable<ReservationDto[]> {
    return this.http.get<ReservationDto[]>(this.apiUrl);
  }
}
