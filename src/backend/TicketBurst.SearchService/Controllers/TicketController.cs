using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("ticket")]
public class TicketController : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public ActionResult<ReplyContract<IEnumerable<TicketContract>>> CreateTickets(
        [FromBody] ReservationInfoContract reservation)
    {
        var @event = MockDatabase.Events.All.FirstOrDefault(e => e.Id == reservation.EventId);
        if (@event == null)
        {
            return ApiResult.Error(400, reason: "EventNotFound");
        }

        var show = MockDatabase.Shows.All.First(s => s.Id == @event.ShowId);
        var venue = MockDatabase.Venues.All.First(v => v.Id == @event.VenueId);
        var hall = venue.Halls[0];
        var hallArea = hall.Areas.FirstOrDefault(a => a.Id == reservation.HallAreaId);
        if (hallArea == null)
        {
            return ApiResult.Error(400, reason: "AreaNotFound");
        }
        
        var hallSeatingMap = MockDatabase.HallSeatingMaps.All.First(m => m.Id == @event.HallSeatingMapId);
        var areaSeatingMap = hallSeatingMap.Areas.First(area => area.HallAreaId == reservation.HallAreaId);

        var tickets = new List<TicketContract>();
        CreateAll();
        
        return tickets.Count == reservation.SeatIds.Count
            ? ApiResult.Success(200, tickets)
            : ApiResult.Error(400, reason: "BadSeatIds");

        void CreateAll()
        {
            foreach (var row in areaSeatingMap.Rows)
            {
                foreach (var seat in row.Seats)
                {
                    if (reservation.SeatIds.Contains(seat.Id))
                    {
                        tickets.Add(CreateOne(row, seat));
                    }
                }
            }
        }
        
        TicketContract CreateOne(SeatingMapRowContract row, SeatingMapSeatContract seat)
        {
            var priceLevel = hallSeatingMap.PriceLevels.First(p => p.Id == seat.PriceLevelId);
            var price = @event.PriceList.PriceByLevelId[priceLevel.Id];
            
            return new TicketContract(
                Id: MockDatabase.MakeNewId(),
                EventId: @event.Id,
                HallAreaId: hallArea.Id,
                RowId: row.Id,
                SeatId: seat.Id,
                PriceLevelId: seat.PriceLevelId,
                VenueName: venue.Name,
                VenueAddress: venue.Address,
                ShowTitle: show.Title,
                EventTitle: @event.Title,
                HallName: hall.Name,
                AreaName: hallArea.Name,
                RowName: row.Name,
                SeatName: seat.Name,
                StartLocalTime: @event.EventStartUtc, //TODO: use local time
                DurationMinutes: @event.DurationMinutes,
                PriceLevelName: priceLevel.Name,
                Price: price);
        }
    }
}