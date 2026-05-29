import { DatePipe } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';
import { ReservationService } from './reservations.service';
import { ReservationDto, ReservationFilterType, ReservationItem } from './reservations.model';

@Injectable({ providedIn: 'root' })
export class ReservationStore {
  private datePipe = inject(DatePipe);
  private reservationService = inject(ReservationService);

  private readonly _reservations = signal<ReservationItem[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _currentFilter = signal<ReservationFilterType>('basic');

  readonly reservations = this._reservations.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly currentFilter = this._currentFilter.asReadonly();

  readonly filteredReservations = computed(() => {
    const allReservations = this._reservations();
    const filter = this._currentFilter();

    switch (filter) {
      case 'basic':
        return allReservations.filter((r) => r.status === 'pending' || r.status === 'confirmed');
      case 'cancelled':
        return allReservations.filter((r) => r.status === 'cancelled');
      case 'expired':
        return allReservations.filter((r) => r.status === 'expired');
      case 'past':
        const now = new Date();
        return allReservations.filter((r) => new Date(r.departureTime) < now);
      default:
        return allReservations;
    }
  });

  setFilter(filter: ReservationFilterType): void {
    this._currentFilter.set(filter);
  }

  loadReservations() {
    this._isLoading.set(true);

    this.reservationService.getMyReservation().subscribe({
      next: (dataFromServer: ReservationDto[]) => {
        const mappedData = dataFromServer.map((dto) => this.mapToUiModel(dto));
        this._reservations.set(mappedData);
        this._isLoading.set(false);
      },
      error: (err) => {
        console.error('Error when try to fetch data: ', err);
        this._isLoading.set(false);
      },
    });
  }

  private mapToUiModel(dto: ReservationDto): ReservationItem {
    const statusLower = dto.status.toLowerCase() as
      | 'pending'
      | 'confirmed'
      | 'cancelled'
      | 'expired';
    const isPending = statusLower === 'pending';

    const depTime = this.datePipe.transform(dto.departureTime, 'HH:mm') || '';
    const arrTime = this.datePipe.transform(dto.arrivalTime, 'HH:mm') || '';
    const depDateStr = this.datePipe.transform(dto.departureTime, 'MMM d') || '';

    let timingLabel = '';
    let isUrgent = false;

    if (isPending) {
      timingLabel = `Expires at ${this.datePipe.transform(dto.expiresAt, 'HH:mm')}`;
      isUrgent = true;
    } else if (statusLower === 'confirmed') {
      timingLabel = 'Ready for boarding';
    } else {
      timingLabel = `Reservation ${statusLower}`;
    }

    return {
      id: dto.id,
      displayId: dto.id.split('-')[0].toUpperCase(),
      status: statusLower,
      statusLabel: dto.status,
      timing: timingLabel,
      urgent: isUrgent,
      departureTime: depTime,
      arrivalTime: arrTime,
      originStation: dto.originStation,
      destinationStation: dto.destinationStation,
      trainInfo: `${dto.trainName} · ${depDateStr}`,
      seats: dto.seatLabels.join(', '),
      price: dto.totalPrice,
    };
  }
}
