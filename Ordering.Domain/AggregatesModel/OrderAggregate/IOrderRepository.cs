
using Ordering.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Add(Order order);
        Task<Order> GetAsync(int orderId);
        void Update(Order order);
    }
}
