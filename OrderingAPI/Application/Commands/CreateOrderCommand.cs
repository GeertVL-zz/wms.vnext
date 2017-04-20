using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Runtime.Serialization;

namespace OrderingAPI.Application.Commands
{
    public class CreateOrderCommand : IRequest<bool>
    {
        [DataMember]
        private readonly List<OrderItemDTO> _orderItems;
        [DataMember]
        public int ClientId { get; private set; }
        [DataMember]
        public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

        public void AddOrderItem(OrderItemDTO item)
        {
            _orderItems.Add(item);
        }

        public CreateOrderCommand()
        {
            _orderItems = new List<OrderItemDTO>();
        }

        public CreateOrderCommand(int clientId) : this()
        {
            ClientId = clientId;
        }

        public class OrderItemDTO
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Units { get; set; }
        }
    }
}
