import { Component, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TripStore } from '../../core/trips/trip.store';

@Component({
  selector: 'app-trips-filter',
  imports: [
    MatFormFieldModule,
    MatDatepickerModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatIconModule,
  ],
  templateUrl: './trips-filter.html',
  styleUrl: './trips-filter.scss',
})
export class TripsFilter {
  private tripStore = inject(TripStore);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    origin: [''],
    destination: [''],
    date: [null as Date | null],
  });

  search(): void {
    const { origin, destination, date } = this.form.getRawValue();
    this.tripStore.setFilters({
      origin: origin ?? '',
      destination: destination ?? '',
      date: date ?? null,
    });

    console.log('origin: ' + origin);
    console.log('destination: ' + destination);
    console.log('date: ' + date);
  }
}
