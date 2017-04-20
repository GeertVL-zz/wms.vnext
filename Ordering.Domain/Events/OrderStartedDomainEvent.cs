using MediatR;
using Ordering.Domain.AggregatesModel.OrderAggregate;

namespace Ordering.Domain.Events
{
    public class OrderStartedDomainEvent
        : INotification
    {
        public Order Order { get; private set; }

        public OrderStartedDomainEvent(Order order)
        {
            Order = order;
        }
    }
}
