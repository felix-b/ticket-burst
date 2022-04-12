﻿#pragma warning disable CS8618

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Integrations;

public class MySqlCheckoutEntityRepository : ICheckoutEntityRepository
{
    private readonly ServerVersion _mySqlVersion = new MySqlServerVersion("8.0.23");
    private readonly string _connectionString;
    private readonly PooledDbContextFactory<CheckoutDbContext> _dbContextFactory;

    public MySqlCheckoutEntityRepository(ISecretsManagerPlugin secrets)
    {
        var connectionSecret = secrets.GetConnectionStringSecret("checkout-db-connstr").Result;
        _connectionString =
            $"server={connectionSecret.Host};database=checkout_service;" + 
            $"user={connectionSecret.UserName};password={connectionSecret.Password}";
        
        //Console.WriteLine($"MySqlCheckoutEntityRepository> using connection string [{_connectionString}]");

        var options = new DbContextOptionsBuilder<CheckoutDbContext>()
            .UseLazyLoadingProxies(true)
            .UseMySql(_connectionString, _mySqlVersion)
            .Options;

        _dbContextFactory = new PooledDbContextFactory<CheckoutDbContext>(options);

        using (var context = _dbContextFactory.CreateDbContext())
        {
            context.Database.EnsureCreated();
            //TODO: ALTER TABLE orders AUTO_INCREMENT=1001;
        }
    }
    
    public string MakeNewId()
    {
        return Guid.NewGuid().ToString("d");
    }

    public uint TakeNextOrderNumber()
    {
        return 0; // assigned when order is inserted
    }

    public async Task<OrderContract> InsertOrder(OrderContract order)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var orderForDb = new OrderContractForDb(order);
            context.Orders.Add(orderForDb);
            await context.SaveChangesAsync();

            Console.WriteLine($"MySqlCheckoutEntityRepository: STORED ORDER, got number [{orderForDb.OrderNumber}]");
            
            return order with {
                OrderNumber = orderForDb.OrderNumber
            };
        }
    }

    public async Task<IEnumerable<OrderContract>> GetMostRecentOrders(int count)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var results = await context.Orders
                .Include(order => order.Tickets)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderNumber)
                .Take(count)
                .ToArrayAsync();
            
            return results
                .Select(r => r.ToImmutable())
                .ToArray();
        }
    }

    public async Task<IEnumerable<AggregatedSalesRecord>> GetRecentAggregatedSales(int count)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var results = await context.AggregatedSales
                .AsNoTracking()
                .OrderByDescending(record => record.OrderDate).ThenByDescending(record => record.TicketCount)
                .Take(count)
                .ToArrayAsync();

            return results;
        }
    }

    public async Task<OrderContract?> TryGetOrderByNumber(uint orderNumber)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var result = await context.Orders
                .Include(order => order.Tickets)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber);
            return result?.ToImmutable();
        }
    }

    public async Task<OrderContract> UpdateOrderPaymentStatus(uint orderNumber, OrderStatus newStatus, string newPaymentToken)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber);

            if (order == null || order.Status != OrderStatus.CompletionInProgress)
            {
                throw new InvalidOrderStatusException(
                    $"Expected order [{orderNumber}] in status [{OrderStatus.CompletionInProgress}]");
            }

            order.Status = newStatus;
            order.PaymentToken = newPaymentToken;
            order.PaymentReceivedUtc = DateTime.UtcNow;
            
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return order.ToImmutable();
        }
    }
    
    public async Task<OrderContract> UpdateOrderShippedStatus(uint orderNumber, DateTime shippedAtUtc)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber);

            if (order == null || order.Status != OrderStatus.Completed)
            {
                throw new InvalidOrderStatusException(
                    $"Expected order [{orderNumber}] in status [{OrderStatus.Completed}]");
            }

            order.TicketsShippedUtc = shippedAtUtc;
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return order.ToImmutable();
        }
    }

    public async Task InsertWorkflowStateRecord(WorkflowStateRecord state)
    {
        using var context = _dbContextFactory.CreateDbContext();

        context.WorkflowStates.Add(state);
        await context.SaveChangesAsync();

        Console.WriteLine(
            $"MySqlCheckoutEntityRepository.InsertWorkflowStateRecord> order [{state.OrderNumber}] state [{state.AwaitStateName}] troken [{state.AwaitStateToken}]");
    }

    public async Task<WorkflowStateRecord?> TryGetWorkflowStateRecord(uint orderNumber)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var result = await context.WorkflowStates.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

        Console.WriteLine(
            $"MySqlCheckoutEntityRepository.TryGetWorkflowStateRecord> order [{orderNumber}] -> {(result != null ? result.AwaitStateToken : "NOT FOUND!")}");

        return result;
    }

    public async Task DeleteWorkflowStateRecord(uint orderNumber)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var result = await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM workflow_states WHERE OrderNumber={orderNumber}");

        Console.WriteLine(
            $"MySqlCheckoutEntityRepository.DeleteWorkflowStateRecord> {result} rows affected");
    }

    public async Task UpsertAggregatedSalesRecord(AggregatedSalesRecord record)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var orderDate = record.OrderDate.Date;   
        var eventDate = record.EventDate.Date;   
        var venueId = record.VenueId;   
        var eventId = record.EventId;   
        var areaId = record.AreaId;   
        var priceLevelId = record.PriceLevelId;
        var ticketCount = record.TicketCount;
        
        // var query = context.AggregatedSales.FromSqlInterpolated(
        //     $"INSERT INTO aggregated_sales (OrderDate,EventDate,VenueId,EventId,AreaId,PriceLevelId,TicketCount) VALUES ({orderDate},{eventDate},{venueId},{eventId},{areaId},{priceLevelId},{ticketCount}) ON DUPLICATE KEY UPDATE TicketCount=TicketCount+{ticketCount}");
        //
        // var result = query.AsAsyncEnumerable();
        // await result.FirstOrDefaultAsync();
        //
        // Console.WriteLine(
        //     $"MySqlCheckoutEntityRepository.UpsertAggregatedSalesRecord> [{record.TicketCount}] tickets");
        
        var result = await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO aggregated_sales (OrderDate,EventDate,VenueId,EventId,AreaId,PriceLevelId,TicketCount) VALUES ({orderDate},{eventDate},{venueId},{eventId},{areaId},{priceLevelId},{ticketCount}) ON DUPLICATE KEY UPDATE TicketCount=TicketCount+{ticketCount}");
        
        Console.WriteLine(
            $"MySqlCheckoutEntityRepository.UpsertAggregatedSalesRecord> [{result}] rows affected");
    }

    public class CheckoutDbContext : DbContext
    {
        public CheckoutDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderContractForDb> Orders { get; set; }
        public DbSet<TicketContractForDb> Tickets { get; set; }
        public DbSet<WorkflowStateRecord> WorkflowStates { get; set; }
        public DbSet<AggregatedSalesRecord> AggregatedSales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderContractForDb>().HasKey(x => x.OrderNumber).HasName("PK_OrderNumber");
            modelBuilder.Entity<OrderContractForDb>()
                .HasMany(x => x.Tickets)
                .WithOne()
                .IsRequired(true);
            
            modelBuilder.Entity<TicketContractForDb>()
                .HasOne<OrderContractForDb>()
                .WithMany(o => o.Tickets)
                .IsRequired(true)
                .HasForeignKey(t => t.OrderNumber);

            modelBuilder.Entity<WorkflowStateRecord>().HasKey(
                nameof(WorkflowStateRecord.OrderNumber),
                nameof(WorkflowStateRecord.AwaitStateName)
            ).HasName("PK_Workflow");

            modelBuilder.Entity<AggregatedSalesRecord>().HasKey(
                nameof(AggregatedSalesRecord.OrderDate),
                nameof(AggregatedSalesRecord.EventDate),
                nameof(AggregatedSalesRecord.EventId),
                nameof(AggregatedSalesRecord.VenueId),
                nameof(AggregatedSalesRecord.AreaId),
                nameof(AggregatedSalesRecord.PriceLevelId)
            ).HasName("PK_Workflow");
            
            modelBuilder.Entity<OrderContractForDb>().ToTable("orders");
            modelBuilder.Entity<TicketContractForDb>().ToTable("tickets");
            modelBuilder.Entity<WorkflowStateRecord>().ToTable("workflow_states");
            modelBuilder.Entity<AggregatedSalesRecord>().ToTable("aggregated_sales");
        }
    }

    public class OrderContractForDb
    {
        public OrderContractForDb()
        {
        }

        public OrderContractForDb(OrderContract source)
        {
            OrderNumber = source.OrderNumber;
            Status = source.Status;
            OrderDescription = source.OrderDescription;
            CreatedAtUtc = source.CreatedAtUtc;
            CustomerName = source.CustomerName;
            CustomerEmail = source.CustomerEmail;
            Tickets = source.Tickets.Select(t => new TicketContractForDb(t, source)).ToHashSet();
            PaymentCurrency = source.PaymentCurrency;
            PaymentSubtotal = source.PaymentSubtotal;
            PaymentTax = source.PaymentTax;
            PaymentTotal = source.PaymentTotal;
            PaymentToken = source.PaymentToken;
            ReservationId = source.ReservationId;
            PaymentReceivedUtc = source.PaymentReceivedUtc;
            TicketsShippedUtc = source.TicketsShippedUtc;
        }

        public OrderContract ToImmutable()
        {
            return new OrderContract(
                OrderNumber,
                Status,
                OrderDescription,
                CreatedAtUtc,
                CustomerName,
                CustomerEmail,
                Tickets.Select(t => t.ToImmutable()).ToImmutableList(),
                PaymentCurrency,
                PaymentSubtotal,
                PaymentTax,
                PaymentTotal,
                PaymentToken,
                ReservationId,
                PaymentReceivedUtc,
                TicketsShippedUtc
            );
        }
        
        public virtual uint OrderNumber { get; set; }
        public virtual OrderStatus Status { get; set; }
        public virtual string OrderDescription { get; set; }
        public virtual DateTime CreatedAtUtc { get; set; }
        public virtual string CustomerName { get; set; }
        public virtual string CustomerEmail { get; set; }
        public virtual ICollection<TicketContractForDb> Tickets { get; set; }
        public virtual string PaymentCurrency { get; set; }
        public virtual decimal PaymentSubtotal { get; set; }
        public virtual decimal PaymentTax { get; set; }
        public virtual decimal PaymentTotal { get; set; }
        public virtual string PaymentToken { get; set; }
        public virtual string ReservationId { get; set; }
        public virtual DateTime? PaymentReceivedUtc { get; set; }
        public virtual DateTime? TicketsShippedUtc { get; set; }
    }

    public class TicketContractForDb
    {
        public TicketContractForDb()
        {
        }

        public TicketContractForDb(TicketContract source, OrderContract orderSource)
        {
            Id = source.Id;
            OrderNumber = orderSource.OrderNumber;
            EventId = source.EventId;
            HallAreaId = source.HallAreaId;
            RowId = source.RowId;
            SeatId = source.SeatId;
            PriceLevelId = source.PriceLevelId;
            VenueName = source.VenueName;
            VenueAddress = source.VenueAddress;
            ShowTitle = source.ShowTitle;
            EventTitle = source.EventTitle;
            HallName = source.HallName;
            AreaName = source.AreaName;
            RowName = source.RowName;
            SeatName = source.SeatName;
            StartLocalTime = source.StartLocalTime;
            DurationMinutes = source.DurationMinutes;
            PriceLevelName = source.PriceLevelName;
            Price = source.Price;
        }

        public TicketContract ToImmutable()
        {
            return new TicketContract(
                Id,
                EventId,
                HallAreaId,
                RowId,
                SeatId,
                PriceLevelId,
                VenueName,
                VenueAddress,
                ShowTitle,
                EventTitle,
                HallName,
                AreaName,
                RowName,
                SeatName,
                StartLocalTime,
                DurationMinutes,
                PriceLevelName,
                Price
            );
        }
        
        public virtual string Id { get; set; }
        public virtual uint OrderNumber { get; set; }
        public virtual string EventId { get; set; }
        public virtual string HallAreaId { get; set; }
        public virtual string RowId { get; set; }
        public virtual string SeatId { get; set; }
        public virtual string PriceLevelId { get; set; }
        public virtual string VenueName { get; set; }
        public virtual string VenueAddress { get; set; }
        public virtual string ShowTitle { get; set; }
        public virtual string? EventTitle { get; set; }
        public virtual string HallName { get; set; }
        public virtual string AreaName { get; set; }
        public virtual string RowName { get; set; }
        public virtual string SeatName { get; set; }
        public virtual DateTime StartLocalTime { get; set; }
        public virtual int DurationMinutes { get; set; }
        public virtual string PriceLevelName { get; set; }
        public virtual decimal Price { get; set; }
    }
}