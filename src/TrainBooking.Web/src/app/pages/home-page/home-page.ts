import { Component } from '@angular/core';
import { TripList } from '../../components/trip-list/trip-list';
import { TripsFilter } from '../../components/trips-filter/trips-filter';

@Component({
  selector: 'app-home-page',
  imports: [TripList, TripsFilter],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {}
