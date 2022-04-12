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
