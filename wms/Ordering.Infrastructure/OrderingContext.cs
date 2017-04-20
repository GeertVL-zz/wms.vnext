using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregatesModel.ClientAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Infrastructure
{
    public class OrderingContext : DbContext, IUnitOfWork
    {
        const string DEFAULT_SCHEMA = "ordering";

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<Client> Clients { get; set; }

        private readonly IMediator _mediator;

        public OrderingContext(DbContextOptions options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(ConfigureClient);
            modelBuilder.Entity<Order>(ConfigureOrder);
            modelBuilder.Entity<OrderItem>(ConfigureOrderItems);
            modelBuilder.Entity<OrderStatus>(ConfigureOrderStatus);
        }

        void ConfigureClient(EntityTypeBuilder<Client> clientConfiguration)
        {
            clientConfiguration.ToTable("clients", DEFAULT_SCHEMA);

            clientConfiguration.HasKey(c => c.Id);

            clientConfiguration.Ignore(c => c.DomainEvents);

            clientConfiguration.Property(c => c.Id)
                .ForSqlServerUseSequenceHiLo("clientseq", DEFAULT_SCHEMA);

            clientConfiguration.Property(c => c.IdentityGuid)
                .HasMaxLength(200)
                .IsRequired();

            clientConfiguration.HasIndex("IdentityGuid")
                .IsUnique(true);           
        }

        void ConfigureOrder(EntityTypeBuilder<Order> orderConfiguration)
        {
            orderConfiguration.ToTable("orders", DEFAULT_SCHEMA);

            orderConfiguration.HasKey(o => o.Id);

            orderConfiguration.Ignore(o => o.DomainEvents);

            orderConfiguration.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("orderseq", DEFAULT_SCHEMA);

            orderConfiguration.Property<DateTime>("OrderDate").IsRequired();
            orderConfiguration.Property<int?>("ClientId").IsRequired(false);
            orderConfiguration.Property<int>("OrderStatusId").IsRequired();

            var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));
            navigation.SetPropertyAccessMode(Microsoft.EntityFrameworkCore.Metadata.PropertyAccessMode.Field);

            orderConfiguration.HasOne<Client>()
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("ClientId");

            orderConfiguration.HasOne(o => o.OrderStatus)
                .WithMany()
                .HasForeignKey("OrderStatusId");
        }

        void ConfigureOrderItems(EntityTypeBuilder<OrderItem> orderItemConfiguration)
        {
            orderItemConfiguration.ToTable("orderitems", DEFAULT_SCHEMA);

            orderItemConfiguration.HasKey(o => o.Id);

            orderItemConfiguration.Ignore(o => o.DomainEvents);

            orderItemConfiguration.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("orderitemsseq", DEFAULT_SCHEMA);

            orderItemConfiguration.Property("OrderId")
                .IsRequired();

            orderItemConfiguration.Property<int>("ProductId")
                .IsRequired();

            orderItemConfiguration.Property<string>("ProductName")
                .IsRequired();

            orderItemConfiguration.Property<decimal>("UnitPrice")
                .IsRequired();

            orderItemConfiguration.Property<int>("Units")
                .IsRequired();
        }

        void ConfigureOrderStatus(EntityTypeBuilder<OrderStatus> orderStatusConfiguration)
        {
            orderStatusConfiguration.ToTable("orderstatus", DEFAULT_SCHEMA);

            orderStatusConfiguration.HasKey(o => o.Id);

            orderStatusConfiguration.Property(o => o.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            orderStatusConfiguration.Property(o => o.Name)
                .HasMaxLength(200)
                .IsRequired();
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);

            var result = await base.SaveChangesAsync();

            return true;
        }
    }
}
