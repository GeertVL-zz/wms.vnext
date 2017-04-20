using Ordering.Domain.SeedWork;
using System.Threading.Tasks;

namespace Ordering.Domain.AggregatesModel.ClientAggregate
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client> Add(Client client);
        Client Update(Client client);
        Task<Client> FindAsync(string clientIdentityId);
    }
}
