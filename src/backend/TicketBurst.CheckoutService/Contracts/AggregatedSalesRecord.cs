using System.ComponentModel.DataAnnotations.Schema;

namespace TicketBurst.CheckoutService.Contracts;

public class AggregatedSalesRecord
{
    public virtual DateTime OrderDate { get; set; }   
    
    public virtual DateTime EventDate { get; set; }

    [Column(TypeName = "VARCHAR(64)")]
    public virtual string VenueId { get; set; }   
    
    [Column(TypeName = "VARCHAR(64)")]
    public virtual string EventId { get; set; }   
    
    [Column(TypeName = "VARCHAR(64)")]
    public virtual string AreaId { get; set; }   
    
    [Column(TypeName = "VARCHAR(64)")]
    public virtual string PriceLevelId { get; set; }
 
    public virtual int TicketCount { get; set; }
}
