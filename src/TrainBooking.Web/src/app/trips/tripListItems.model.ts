type ISODateTime = string;

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface TripListItemDto {
    tripId: string;
    originStation: string;
    destinationStation: string;
    departureTime: ISODateTime;
    arrivalTime: ISODateTime;
    availableSeatsCount: number;
    minPrice: number;
}
