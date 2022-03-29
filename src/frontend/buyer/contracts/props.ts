export interface EventCardProps {
    eventId: string,
    dateText: string,
    showNameText: string,
    eventNameText: string,
    priceText: string,
    seatsLeftBadgeText: string,
    venueText: string,
}

export interface PriceLevelProps {
    id: string;
    name: string;
    colorHexRgb: string;
    price: number;
}
