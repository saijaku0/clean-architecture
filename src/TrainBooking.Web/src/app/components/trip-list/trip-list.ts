import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripStore } from '../../../core/trips/trip.store';

@Component({
  selector: 'app-trip-list',
  imports: [CommonModule],
  templateUrl: './trip-list.html',
  styleUrl: './trip-list.scss',
})
export class TripList {
  protected tripStore = inject(TripStore);

  ngOnInit() {
    this.tripStore.loadTrips();
  }
}
