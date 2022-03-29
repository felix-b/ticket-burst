export interface EventSearchResult {
    eventId: string;
    hallId: string;
    hallName: string;
    venueId: string;
    venueName: string;
    venueAddress: string;
    venueLocation: VenueLocation;
    venueTimeZone: VenueTimeZone;
    showId: string;
    showName: string;
    showDescription: string;
    showTypeId: string;
    showTypeName: string;
    genreId: string;
    genreName: string;
    eventTitle: string;
    eventDescription: string;
    posterImageUrl: string;
    troupes?: Troupe[];
    saleStartTime: string;
    eventStartTime: string;
    durationMinutes: number;
    minPrice: number;
    maxPrice: number;
    isOpenForSale: boolean;
    numberOfSeatsLeft: number;
}

export interface VenueLocation {
    lat: number;
    lon: number;
}

export interface VenueTimeZone {
    name: string;
    utcOffsetHours: number;
}

export interface Troupe {
    id: string;
    genreId: string;
    name: string;
    posterImageUrl: string;
}

export interface EventSearchFullDetail {
    event: EventSearchResult;
    hall: EventSearchHallInfo;
    priceList: PriceList;
}

export interface EventSearchHallInfo {
    seatingPlanImageUrl: string;
    totalCapacity: number;
    availableCapacity: number;
    areas: EventSearchAreaInfo[];
    priceLevels: PriceLevel[];
}

export interface EventSearchAreaInfo {
    hallAreaId: string;
    name: string;
    seatingPlanImageUrl: string;
    totalCapacity: number;
    availableCapacity: number;
    minPrice: number;
    maxPrice: number;
}

export interface PriceLevel {
    id: string;
    name: string;
    colorHexRgb: string;
}

export interface PriceList {
    priceByLevelId: Record<string, number>;
}

export interface AreaSeatingMapSeat {
    id: string;
    name: string;
    priceLevelId: string;
    status: number;
}

export interface AreaSeatingMapRow {
    id: string;
    name: string;
    seats: AreaSeatingMapSeat[];
}

export interface AreaSeatingMap {
    seatingMapId: string;
    hallAreaId: string;
    hallAreaName: string;
    planImageUrl: string;
    capacity: number;
    rows: AreaSeatingMapRow[];
}

export interface EventSearchAreaFullDetail {
    header: EventSearchFullDetail;
    hallAreaId: string;
    hallAreaName: string;
    availableCapacity: number;
    priceLevels: PriceLevel[];
    seatingMap: AreaSeatingMap;
}
