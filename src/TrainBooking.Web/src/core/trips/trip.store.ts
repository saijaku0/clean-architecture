import { inject, Injectable, signal } from '@angular/core';
import { TripListItemDto } from './tripListItems.model';
import { TripService } from './trip.service';

@Injectable({ providedIn: 'root' })
export class TripStore {
  private tripService = inject(TripService);

  private tripsSignal = signal<TripListItemDto[]>([]);
  private loadingSignal = signal(false);
  private errorSignal = signal<string | null>(null);

  trips = this.tripsSignal.asReadonly();
  loading = this.loadingSignal.asReadonly();
  error = this.errorSignal.asReadonly();

  loadTrips() {
    this.loadingSignal.set(true);
    this.tripService.getTrips().subscribe({
      next: (result) => {
        this.tripsSignal.set(result.items);
        this.loadingSignal.set(false);
      },
      error: (err) => {
        this.errorSignal.set('Failed to load trips. Please try again later.');
        console.error('Error loading trips:', err);
        this.loadingSignal.set(false);
      } 
    });
  }
}
