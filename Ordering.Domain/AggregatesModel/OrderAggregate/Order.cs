using Ordering.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class Order : Entity, IAggregateRoot
    {
        private DateTime _orderDate;

        public OrderStatus OrderStatus { get; private set; }
        private int _orderStatusId;

        private int? _clientId;

        private readonly List<OrderItem> _orderItems;

        public IEnumerable<OrderItem> OrderItems => _orderItems.AsReadOnly();

        protected Order() { }

        public Order(int? clientId = null)
        {
            _clientId = clientId;
            _orderItems = new List<OrderItem>();
            _orderStatusId = OrderStatus.Active.Id;
            _orderDate = DateTime.UtcNow;
        }

        public void AddOrderItem(int productId, string productName, decimal unitPrice, int units = 1)
        {
            var existingOrderForProduct = _orderItems.Where(o => o.ProductId == productId).SingleOrDefault();
            if (existingOrderForProduct != null)
            {
                existingOrderForProduct.AddUnits(units);
            }
            else
            {
                var orderItem = new OrderItem(productId, productName, unitPrice, units);
                _orderItems.Add(orderItem);
            }
        }

        public void SetClientId(int id)
        {
            _clientId = id;
        }
    }
}
