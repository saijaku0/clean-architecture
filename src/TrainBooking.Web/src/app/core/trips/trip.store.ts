import { computed, inject, Injectable, signal } from '@angular/core';
import { TripFilters, TripListItemDto } from './tripListItems.model';
import { TripService } from './trip.service';

@Injectable({ providedIn: 'root' })
export class TripStore {
  private tripService = inject(TripService);

  private readonly tripsSignal = signal<TripListItemDto[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);

  private readonly totalCountSignal = signal(0);
  private readonly pageIndexSignal = signal(0);
  private readonly pageSizeSignal = signal(20);

  private readonly _filters = signal<TripFilters>({
    origin: '',
    destination: '',
    date: null,
  });

  readonly trips = this.tripsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly filters = this._filters.asReadonly();

  readonly totalCount = this.totalCountSignal.asReadonly();
  readonly pageIndex = this.pageIndexSignal.asReadonly();
  readonly pageSize = this.pageSizeSignal.asReadonly();

  readonly filteredTrips = computed(() => {
    const trips = this.trips();
    const { origin, destination, date } = this._filters();

    const o = origin.trim().toLowerCase();
    const d = destination.trim().toLowerCase();

    return trips.filter((trip) => {
      const matchOrigin = !o || trip.originStation.toLowerCase().includes(o);
      const matchDest = !d || trip.destinationStation.toLowerCase().includes(d);
      const matchDate = !date || this.isSameDay(trip.departureTime, date);
      return matchOrigin && matchDest && matchDate;
    });
  });

  loadTrips() {
    this.loadingSignal.set(true);

    const { origin, destination, date } = this._filters();
    const pageNumber = this.pageIndexSignal() + 1;
    const pageSize = this.pageSizeSignal();

    const dateString = date ? new Date(date).toISOString() : undefined;

    this.tripService.getTrips(pageNumber, pageSize, origin, destination, dateString).subscribe({
      next: (result) => {
        this.tripsSignal.set(result.items);
        this.totalCountSignal.set(result.totalCount);
        this.loadingSignal.set(false);
      },
      error: (err) => {
        this.errorSignal.set('Failed to load trips. Please try again later.');
        console.error('Error loading trips:', err);
        this.loadingSignal.set(false);
      },
    });
  }

  setFilters(partial: Partial<TripFilters>): void {
    this._filters.update((f) => ({ ...f, ...partial }));
    this.pageIndexSignal.set(0);
    this.loadTrips();
  }

  setPagination(pageIndex: number, pageSize: number): void {
    this.pageIndexSignal.set(pageIndex);
    this.pageSizeSignal.set(pageSize);
    this.loadTrips();
  }

  private isSameDay(value: string | Date, target: Date): boolean {
    const d = new Date(value);
    return (
      d.getFullYear() === target.getFullYear() &&
      d.getMonth() === target.getMonth() &&
      d.getDate() === target.getDate()
    );
  }
}
