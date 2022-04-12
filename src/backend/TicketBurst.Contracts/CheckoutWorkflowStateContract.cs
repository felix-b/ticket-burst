#pragma warning disable CS8618

using System;

namespace TicketBurst.Contracts;

public class CheckoutWorkflowStateContract
{
    public OrderPart Order { get; set; }
    public PaymentResultPart PaymentResult { get; set; }

    public class PaymentResultPart
    {
        public string PaymentStatus { get; set; } 
    }
    
    public class OrderPart
    {
        public OrderPart()
        {
        }

        public OrderPart(OrderContract order)
        {
            OrderNumber = order.OrderNumber;  
            ReservationId = order.ReservationId;  
            OrderDate = order.CreatedAtUtc.Date;  
            EventDate = order.Tickets[0].StartLocalTime.Date;  
            VenueId = string.Empty;  
            EventId = order.Tickets[0].EventId;  
            AreaId = order.Tickets[0].HallAreaId;  
            PriceLevelId = order.Tickets[0].PriceLevelId;  
            TicketCount = order.Tickets.Count;  
        }

        public uint OrderNumber { get; set; }
        public string ReservationId { get; set; }
        public DateTime OrderDate { get; set; }   
        public DateTime EventDate { get; set; }   
        public string VenueId { get; set; }   
        public string EventId { get; set; }   
        public string AreaId { get; set; }   
        public string PriceLevelId { get; set; }
        public int TicketCount { get; set; }
    }
}
