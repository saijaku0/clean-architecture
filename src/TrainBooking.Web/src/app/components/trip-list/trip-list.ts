import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripStore } from '../../core/trips/trip.store';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-trip-list',
  imports: [CommonModule, RouterLink, MatIcon, MatPaginatorModule, MatProgressSpinnerModule],
  templateUrl: './trip-list.html',
  styleUrl: './trip-list.scss',
})
export class TripList {
  protected tripStore = inject(TripStore);

  ngOnInit() {
    this.tripStore.loadTrips();
  }

  onPageChange(event: PageEvent) {
    this.tripStore.setPagination(event.pageIndex, event.pageSize);
  }
}
