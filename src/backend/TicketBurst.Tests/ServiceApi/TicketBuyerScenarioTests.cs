using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.ServiceApi;

[TestFixture(Category = "e2e")]
public class TicketBuyerScenarioTests
{
    [Test]
    public async Task BuyTicketsEndToEnd()
    {
        var searchResult = await FindEvent();
        var eventDetails = await ViewEventDetails(searchResult);
        var seatingMap = await ViewSeatingMap(eventDetails);
        var reservationReply = await GrabSeats(seatingMap);
        var orderPreview = await PreviewCheckout(reservationReply);
        var realOrder = await BeginCheckout(reservationReply);
        await EnterPaymentDetails(realOrder);
        await WaitForOrderCompleted(realOrder.OrderNumber);
        await WaitForTickets(realOrder);

        async Task WaitForTickets(OrderContract order)
        {
            var outboxFolderPath =
                @"D:\oss\ticket-burst\src\backend\TicketBurst.CheckoutService\bin\Debug\net6.0\mock-email-outbox";
            
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(40))
            {
                var allFiles = Directory.GetFiles(outboxFolderPath);
                if (allFiles.Any(f => f.Contains(order.CustomerEmail)))
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            
            throw new AssertionFailedException("Order was not completed");
        }

        async Task WaitForOrderCompleted(uint orderNumber)
        {
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(40))
            {
                var allOrders = (await ServiceClient.HttpGetJson<IEnumerable<OrderContract>>(
                    ServiceName.Checkout,
                    path: new[] { "order" }
                ))?.ToArray();

                allOrders.Should().NotBeNull();

                var order = allOrders!.FirstOrDefault(o => o.OrderNumber == orderNumber);
                if (order != null && order.Status == OrderStatus.Completed)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            
            throw new AssertionFailedException("Order was not completed");
        }

        async Task EnterPaymentDetails(OrderContract order)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var reply = await ServiceClient.HttpPostJson<string>(
                ServiceName.Checkout,
                path: new[] { "payment-mock", "confirm-payment" },
                body: order.PaymentToken);

            reply.Should().Be("SUCCESS");
        }

        async Task<OrderContract> BeginCheckout(SeatReservationReplyContract reservation)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));

            var request = new BeginCheckoutRequestContract {
                CheckoutToken = reservation.CheckoutToken,
                EventId = reservation.Request.EventId,
                HallAreaId = reservation.Request.HallAreaId,
                CustomerName = "John Smith",
                CustomerEmail = "john.smith@success.com"
            };
            var order = await ServiceClient.HttpPostJson<OrderContract>(
                ServiceName.Checkout,
                path: new[] { "checkout", "begin" },
                body: request);
            
            order.Should().NotBeNull();
            order!.OrderNumber.Should().BeGreaterThan(0);
            order.Status.Should().Be(OrderStatus.CompletionInProgress);
            order.PaymentToken.Should().NotBeNullOrWhiteSpace();

            return order;
        }

        async Task<OrderContract> PreviewCheckout(SeatReservationReplyContract reservation)
        {
            var request = new BeginCheckoutRequestContract {
                Preview = true,
                CheckoutToken = reservation.CheckoutToken,
                EventId = reservation.Request.EventId,
                HallAreaId = reservation.Request.HallAreaId
            };
            var preview = await ServiceClient.HttpPostJson<OrderContract>(
                ServiceName.Checkout,
                path: new[] { "checkout", "begin" },
                body: request);

            preview.Should().NotBeNull();
            preview!.Status.Should().Be(OrderStatus.Preview);
            preview.OrderNumber.Should().Be(0);
            preview.PaymentToken.Should().BeEmpty();
            preview.ReservationId.Should().BeEmpty();
            preview.Tickets.Count.Should().Be(3);
            preview.Tickets.All(t => t.Price > 10).Should().BeTrue();
            preview.Tickets.Sum(t => t.Price).Should().Be(preview.PaymentSubtotal);
            preview.PaymentSubtotal.Should().BeGreaterThan(100);
            preview.PaymentTax.Should().BeGreaterThan(10);
            preview.PaymentTotal.Should().Be(preview.PaymentSubtotal + preview.PaymentTax);

            return preview;
        }

        async Task<SeatReservationReplyContract> GrabSeats(EventSearchAreaSeatingContract area)
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
            
            var seating = await ServiceClient.HttpGetJson<EventSearchAreaSeatingContract>(
                ServiceName.Search, 
                path: new[] { "search", "event", details.Event.EventId, "area", area.HallAreaId });
            
            seating.Should().NotBeNull();
            seating!.SeatingMap.Rows.Count.Should().BeGreaterThan(3);
            seating.SeatingMap.Rows[0].Seats.Count.Should().BeGreaterThan(10);
            seating.SeatingMap.Rows[0].Seats.All(seat => seat.Status == SeatStatus.Available).Should().BeTrue();

            return seating;
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

            while (clock.Elapsed < TimeSpan.FromSeconds(70))
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
