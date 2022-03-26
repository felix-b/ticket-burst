using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.ServiceApi;

[TestFixture]
public class TicketBuyerScenarioTests
{
    [Test]
    public async Task CanBuyTickets()
    {

        async Task<SeatReservationReplyContract> GrabSeats(EventSearchAreaSeatingContract area, int count)
        {
            var row = area.SeatingMap.Rows[2];
            var seats = new[] {
                row.Seats[3],
                row.Seats[4],
                row.Seats[5]
            };

            var request = new SeatReservationRequestContract(
                eventId: area.Header.Event.EventId,
                hallAreaId: area.HallAreaId,
                seatIds: seats.Select(s => s.Id).ToArray(),
                clientContext: null);
            var reservationReply = await ServiceClient.HttpPostJson<SeatReservationReplyContract>(
                ServiceName.Search,
                path: new[] { "search", "reservation", "grab" },
                body: request);

            reservationReply.Should().NotBeNull();
            reservationReply!.Success.Should().BeTrue();
            reservationReply!.CheckoutToken.Should().NotBeNullOrWhiteSpace();

            return reservationReply;
        }

        async Task<EventSearchAreaSeatingContract> ViewSeatingMap(EventSearchFullDetailContract details)
        {
            var area = details.Hall.Areas[1];
            area.AvailableCapacity.Should().BeGreaterThan(100);
            
            var seatingMap = await ServiceClient.HttpGetJson<EventSearchAreaSeatingContract>(
                ServiceName.Search, 
                path: new[] { "search", "event", details.Event.EventId, "area", area.HallAreaId });
            
            seatingMap.Should().NotBeNull();
            seatingMap!.SeatingMap.Rows.Count.Should().BeGreaterThan(3);

            return seatingMap;
        }
        
        async Task<EventSearchFullDetailContract> ViewEventDetails(EventSearchResultContract result)
        {
            var fullDetails = await ServiceClient.HttpGetJson<EventSearchFullDetailContract>(
                ServiceName.Search, 
                path: new[] { "search", "event", result.EventId });
                        
            fullDetails.Should().NotBeNull();
            fullDetails!.Hall.Areas.Count.Should().BeGreaterThan(2);

            return fullDetails;
        }

        async Task<EventSearchResultContract> FindEvent()
        {
            var clock = Stopwatch.StartNew();

            while (clock.Elapsed < TimeSpan.FromSeconds(40))
            {
                var searchResults = (await ServiceClient.HttpGetJson<IEnumerable<EventSearchResultContract>>(
                    ServiceName.Search,
                    path: new[] { "search" }
                ))?.ToArray();

                searchResults.Should().NotBeNull();
                searchResults!.Length.Should().BeGreaterThan(2);

                var pickedResult = searchResults[0];
                if (pickedResult.IsOpenForSale)
                {
                    return pickedResult;
                }
            }
            
            throw new AssertionFailedException("Event was not open for sale");
        }
    }
}
