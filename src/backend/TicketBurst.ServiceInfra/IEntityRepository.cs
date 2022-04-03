namespace TicketBurst.ServiceInfra;

public interface IEntityRepository
{
    public EntityNotFoundException NotFound<TEntity>(string id) => 
        new EntityNotFoundException(typeof(TEntity).Name, id);
}
