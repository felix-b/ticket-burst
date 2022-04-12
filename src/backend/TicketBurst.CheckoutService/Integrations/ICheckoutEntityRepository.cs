﻿using TicketBurst.CheckoutService.Contracts;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public interface ICheckoutEntityRepository
{
    string MakeNewId();
    uint TakeNextOrderNumber();
    Task<OrderContract> InsertOrder(OrderContract order);
    Task<IEnumerable<OrderContract>> GetMostRecentOrders(int count);
    Task<IEnumerable<AggregatedSalesRecord>> GetRecentAggregatedSales(int count);
    Task<OrderContract?> TryGetOrderByNumber(uint orderNumber);
    Task<OrderContract> UpdateOrderPaymentStatus(uint orderNumber, OrderStatus newStatus, string newPaymentToken);
    Task<OrderContract> UpdateOrderShippedStatus(uint orderNumber, DateTime shippedAtUtc);
    Task InsertWorkflowStateRecord(WorkflowStateRecord state);
    Task<WorkflowStateRecord?> TryGetWorkflowStateRecord(uint orderNumber);
    Task DeleteWorkflowStateRecord(uint orderNumber);
    Task UpsertAggregatedSalesRecord(AggregatedSalesRecord record);
}
