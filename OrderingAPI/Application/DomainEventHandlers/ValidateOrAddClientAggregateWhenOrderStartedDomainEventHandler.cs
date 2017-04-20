using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Domain.AggregatesModel.ClientAggregate;
using Ordering.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingAPI.Application.DomainEventHandlers
{
    public class ValidateOrAddClientAggregateWhenOrderStartedDomainEventHandler : IAsyncNotificationHandler<OrderStartedDomainEvent>
    {
        private ILoggerFactory _logger;
        private IClientRepository _clientRepository;


        public ValidateOrAddClientAggregateWhenOrderStartedDomainEventHandler(ILoggerFactory logger, IClientRepository clientRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public Task Handle(OrderStartedDomainEvent notification)
        {
            return null;
        }
    }
}
