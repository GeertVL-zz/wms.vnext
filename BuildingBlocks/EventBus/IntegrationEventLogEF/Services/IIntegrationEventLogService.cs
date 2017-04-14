using Wms.BuildingBlocks.EventBus.Events;
using System.Data.Common;
using System.Threading.Tasks;

namespace Wms.BuildingBlocks.IntegrationEventLogEF.Services
{
    public interface IIntegrationEventLogService
    {
        Task SaveEventAsync(IntegrationEvent @event, DbTransaction transaction);
        Task MarkEventAsPublishedAsync(IntegrationEvent @event);
    }
}
