import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TripService } from '../core/services/trip.service';
import { TripListItemDto } from '../core/models/tripListItems.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('TrainBooking.Web');
  private tripService = inject(TripService);
  
  trips = signal<TripListItemDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  ngOnInit() {
    this.loadTrips();
  }

  private loadTrips() {
    this.loading.set(true);
    this.tripService.getTrips().subscribe({
      next: (result) => {
        this.trips.set(result.items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load trips. Please try again later.' + err.message);
        this.loading.set(false);
      } 
    });
  }
}
