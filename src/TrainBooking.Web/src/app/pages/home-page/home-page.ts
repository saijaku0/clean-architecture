import { Component } from '@angular/core';
import { TripList } from '../../components/trip-list/trip-list';

@Component({
  selector: 'app-home-page',
  imports: [TripList],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {}
