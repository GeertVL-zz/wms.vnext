using Ordering.Domain.Exceptions;
using Ordering.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class OrderItem : Entity
    {
        private string _productName;
        private decimal _unitPrice;
        private int _units;

        public int ProductId { get; private set; }

        public OrderItem()
        {

        }

        public OrderItem(int productId, string productName, decimal unitPrice, int units = 1)
        {
            if (units <= 0)
            {
                throw new OrderingDomainException("Invalid number of units");
            }

            ProductId = productId;

            _productName = productName;
            _unitPrice = unitPrice;
            _units = units;
        }

        public void AddUnits(int units)
        {
            if (units < 0)
            {
                throw new OrderingDomainException("Invalid units");
            }

            _units += units;
        }
    }
}
