import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { PagedResult, TripListItemDto } from "./tripListItems.model";
import { Observable } from "rxjs";
import { environment } from "../../../environment";

@Injectable({ providedIn: 'root' })
export class TripService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/trips`;

    getTrips(): Observable<PagedResult<TripListItemDto>> {
        return this.http.get<PagedResult<TripListItemDto>>(this.apiUrl);
    }
}