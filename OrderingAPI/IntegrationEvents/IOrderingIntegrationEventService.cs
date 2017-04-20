using System.Threading.Tasks;
using Wms.BuildingBlocks.EventBus.Events;

namespace OrderingAPI.IntegrationEvents
{
    public interface IOrderingIntegrationEventService
    {
        Task SaveEventAndOrderingContextChangesAsync(IntegrationEvent evt);
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
