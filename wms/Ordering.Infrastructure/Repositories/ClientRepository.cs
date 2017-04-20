using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregatesModel.ClientAggregate;
using Ordering.Domain.SeedWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly OrderingContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public ClientRepository(OrderingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Client> Add(Client client)
        {
            if (client.IsTransient())
            {
                var result = await _context.Clients
                    .AddAsync(client);

                return result.Entity;
                   
            }
            else
            {
                return client;
            }
        }

        public async Task<Client> FindAsync(string clientIdentityId)
        {
            var client = await _context.Clients
                .Where(c => c.IdentityGuid == clientIdentityId)
                .SingleOrDefaultAsync();

            return client;
        }

        public Client Update(Client client)
        {
            return _context.Clients
                .Update(client)
                .Entity;
        }
    }
}
