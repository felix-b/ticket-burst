using System;

namespace TicketBurst.Contracts;

public class EventSearchRequestContract
{
    public string? Id { get; set; }
    public DateTime? FromDate { get; set; }    
    public DateTime? ToDate { get; set; }
    public bool? Selling { get; set; }
    public int? Count { get; set; }
}
