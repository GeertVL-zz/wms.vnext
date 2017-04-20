using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ordering.Infrastructure;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Wms.BuildingBlocks.EventBus.Abstractions;
using Wms.BuildingBlocks.EventBus.Events;
using Wms.BuildingBlocks.IntegrationEventLogEF.Services;
using Wms.BuildingBlocks.IntegrationEventLogEF.Utilities;

namespace OrderingAPI.IntegrationEvents
{
    public class OrderingIntegrationEventService : IOrderingIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly OrderingContext _orderingContext;
        private readonly IIntegrationEventLogService _eventLogService;

        public OrderingIntegrationEventService(IEventBus eventBus, OrderingContext orderingContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
        {
            _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_orderingContext.Database.GetDbConnection());
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            _eventBus.Publish(evt);
            await _eventLogService.MarkEventAsPublishedAsync(evt);
        }

        public async Task SaveEventAndOrderingContextChangesAsync(IntegrationEvent evt)
        {
            await ResilientTransaction.New(_orderingContext)
                .ExecuteAsync(async () => {
                    // Achieving atomicity between original ordering database operation and the IntegrationEventLog thanks to a local transaction
                    await _orderingContext.SaveChangesAsync();
                    await _eventLogService.SaveEventAsync(evt, _orderingContext.Database.CurrentTransaction.GetDbTransaction());
            });
        }
    }
}
