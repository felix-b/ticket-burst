namespace TicketBurst.CheckoutService.Contracts;

public class InvalidOrderStatusException : Exception
{
    public InvalidOrderStatusException(string? message) : base(message)
    {
    }
}