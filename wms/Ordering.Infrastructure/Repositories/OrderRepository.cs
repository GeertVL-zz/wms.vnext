using Ordering.Domain.AggregatesModel.OrderAggregate;
using System;
using Ordering.Domain.SeedWork;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderingContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public OrderRepository(OrderingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Order Add(Order order)
        {
            return _context.Orders.Add(order).Entity;
        }

        public async Task<Order> GetAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        public void Update(Order order)
        {
            _context.Entry(order).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
    }
}
