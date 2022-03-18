using System.Text.Json.Serialization;

namespace TicketBurst.Contracts;

public record VenueContract(
    string Id,   
    string Name,   
    string Address,   
    double LocationLat,
    double LocationLon,
    string TimezoneId,   
    int UtcOffsetHours,   
    string Description,   
    string PhotoImageUrl,
    string SeatingPlanImageUrl,   
    HallContract[]? Halls 
);
