import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripStore } from '../../core/trips/trip.store';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-trip-list',
  imports: [CommonModule, RouterLink, MatIcon],
  templateUrl: './trip-list.html',
  styleUrl: './trip-list.scss',
})
export class TripList {
  protected tripStore = inject(TripStore);

  ngOnInit() {
    this.tripStore.loadTrips();
  }
}
