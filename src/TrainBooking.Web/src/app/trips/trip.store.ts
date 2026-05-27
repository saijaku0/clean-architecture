import { inject, Injectable, signal } from '@angular/core';
import { TripListItemDto } from './tripListItems.model';
import { TripService } from './trip.service';

@Injectable({ providedIn: 'root' })
export class TripStore {
  private tripService = inject(TripService);

  private readonly tripsSignal = signal<TripListItemDto[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);

  readonly trips = this.tripsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();

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
