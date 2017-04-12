using System;

namespace ProductAPI.InfraStructure.Exceptions
{
    public class ProductDomainException : Exception
    {
        public ProductDomainException() {}
        public ProductDomainException(string message)
            : base(message)
        {            
        }
        public ProductDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}