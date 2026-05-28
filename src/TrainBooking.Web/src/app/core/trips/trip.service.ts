import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { PagedResult, TripListItemDto } from './tripListItems.model';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';

@Injectable({ providedIn: 'root' })
export class TripService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/trips`;

  getTrips(
    pageNumber: number = 1,
    pageSize: number = 20,
    origin?: string,
    destination?: string,
    departureDate?: string,
  ): Observable<PagedResult<TripListItemDto>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);

    if (origin) params = params.set('origin', origin);
    if (destination) params = params.set('destination', destination);
    if (departureDate) params = params.set('departureDate', departureDate);

    return this.http.get<PagedResult<TripListItemDto>>(this.apiUrl, { params });
  }
}
