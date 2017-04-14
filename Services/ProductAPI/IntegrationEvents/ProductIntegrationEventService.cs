using System;
using System.Threading.Tasks;
using Wms.BuildingBlocks.EventBus.Events;
using System.Data.Common;
using Wms.BuildingBlocks.IntegrationEventLogEF.Services;
using Wms.BuildingBlocks.EventBus.Abstractions;
using ProductAPI.InfraStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wms.BuildingBlocks.IntegrationEventLogEF.Utilities;

namespace ProductAPI.IntegrationEvents
{
    public class ProductIntegrationEventService : IProductIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly ProductContext _productContext;
        private readonly IIntegrationEventLogService _eventLogService;

        public ProductIntegrationEventService(IEventBus eventBus, ProductContext productContext, Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
        {
            _productContext = productContext ?? throw new ArgumentNullException(nameof(productContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_productContext.Database.GetDbConnection());
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            _eventBus.Publish(evt);
            await _eventLogService.MarkEventAsPublishedAsync(evt);
        }

        public async Task SaveEventAndProductContextChangesAsync(IntegrationEvent evt)
        {
            await ResilientTransaction.New(_productContext)
                .ExecuteAsync(async () =>
                {
                    await _productContext.SaveChangesAsync();
                    await _eventLogService.SaveEventAsync(evt, _productContext.Database.CurrentTransaction.GetDbTransaction());
                });
        }
    }
}
