namespace TicketBurst.ReservationService.Integrations.ProtoActor;

public static class EventAreaManagerGrainIdentity
{
    public static void ParseOrThrow(string identity, out string eventId, out string areaId)
    {
        var identityParts = identity.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (identityParts.Length != 2)
        {
            throw new Exception($"EventAreaManagerGrain.ctor cannot parse identity [{identity}]");
        }

        eventId = identityParts[0];
        areaId = identityParts[1];
    }
    
    public static string Construct(string eventId, string areaId)
    {
        return $"{eventId}/{areaId}";
    }
}
