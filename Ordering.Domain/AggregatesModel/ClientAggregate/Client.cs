using Ordering.Domain.SeedWork;
using System;

namespace Ordering.Domain.AggregatesModel.ClientAggregate
{
    public class Client : Entity, IAggregateRoot
    {
        public string IdentityGuid { get; private set; }

        public Client()
        {

        }

        public Client(string identity) : this()
        {
            IdentityGuid = !string.IsNullOrWhiteSpace(identity) ? identity : throw new ArgumentNullException(nameof(identity));
        }
    }
}
