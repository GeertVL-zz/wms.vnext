using Wms.BuildingBlocks.EventBus.Events;
using System.Threading.Tasks;

namespace ProductAPI.IntegrationEvents
{
    public interface IProductIntegrationEventService
    {
        Task SaveEventAndProductContextChangesAsync(IntegrationEvent evt);
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
