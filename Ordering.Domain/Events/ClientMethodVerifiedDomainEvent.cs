using MediatR;
using Ordering.Domain.AggregatesModel.ClientAggregate;

namespace Ordering.Domain.Events
{
    public class ClientMethodVerifiedDomainEvent : INotification
    {
        public Client Client { get; private set; }
        public int OrderId { get; private set; }

        public ClientMethodVerifiedDomainEvent(Client client, int orderId)
        {
            Client = client;
            OrderId = orderId;
        }
    }
}
