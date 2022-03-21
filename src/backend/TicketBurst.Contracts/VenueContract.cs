using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace TicketBurst.Contracts;

public record VenueContract(
    string Id,   
    string Name,   
    string Address,
    GeoPointContract Location,
    TimeZoneContract TimeZone,
    string Description,
    string WebSiteUrl,
    string PhotoImageUrl,
    int DefaultCapacity,
    ImmutableList<HallContract> Halls
);

public record GeoPointContract(double Lat, double Lon);
public record TimeZoneContract(string Name, int UtcOffsetHours);

