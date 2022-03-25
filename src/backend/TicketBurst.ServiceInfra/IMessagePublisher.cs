namespace TicketBurst.ServiceInfra;

public interface IMessagePublisher<T>
    where T : class
{
    void Publish(T message);
}
