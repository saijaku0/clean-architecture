export interface ReservationDto {
  id: string;
  status: string;
  expiresAt: string;
  createdAt: string;
  tripId: string;
  trainName: string;
  originStation: string;
  destinationStation: string;
  departureTime: string;
  arrivalTime: string;
  seatLabels: string[];
  totalPrice: number;
}

export interface ReservationItem {
  id: string; 
  displayId: string; 
  status: 'pending' | 'confirmed' | 'cancelled' | 'expired';
  statusLabel: string;
  timing: string;
  urgent: boolean;
  departureTime: string; 
  arrivalTime: string; 
  originStation: string;
  destinationStation: string;
  trainInfo: string;
  seats: string;
  price: number;
}

export type ReservationFilterType = 'basic' | 'past' | 'cancelled' | 'expired';