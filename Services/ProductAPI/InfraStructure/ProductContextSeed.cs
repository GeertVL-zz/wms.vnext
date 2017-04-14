using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductAPI.Model;

namespace ProductAPI.InfraStructure
{
    public class ProductContextSeed
    {
        public static async Task SeedAsync(IApplicationBuilder applicationBuilder, ILoggerFactory loggerFactory, int? retry = 0)
        {
            var context = (ProductContext)applicationBuilder.ApplicationServices.GetService(typeof(ProductContext));
            context.Database.Migrate();
            if (!context.ProductBrands.Any())
            {
                context.ProductBrands.AddRange(GetPreconfiguredProductBrands());
                await context.SaveChangesAsync();
            }

            if (!context.ProductTypes.Any())
            {
                context.ProductTypes.AddRange(GetPreconfiguredProductTypes());
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(GetPreconfiguredProducts());
                await context.SaveChangesAsync();
            }
        }

        private static IEnumerable<ProductBrand> GetPreconfiguredProductBrands()
        {
            return new List<ProductBrand>
            {
                new ProductBrand { Brand = "Douwe Egberts" },
                new ProductBrand { Brand = "Lavazza" },
                new ProductBrand { Brand = "Jacquemotte" },
                new ProductBrand { Brand = "Illy" },
                new ProductBrand { Brand = "Nespresso" }
            };
        }

        private static IEnumerable<ProductType> GetPreconfiguredProductTypes()
        {
            return new List<ProductType>
            {
                new ProductType { Type = "Grinded" },
                new ProductType { Type = "Beans" },
                new ProductType { Type = "Pads" },
                new ProductType { Type = "Cups" }
            };
        }

        private static IEnumerable<Product> GetPreconfiguredProducts()
        {
            return new List<Product>
            {
                new Product { ProductBrandId = 1, ProductTypeId = 1, Name = "Mildou", Price = 2.5M },
                new Product { ProductBrandId = 1, ProductTypeId = 1, Name = "Dessert", Price = 2.3M },
                new Product { ProductBrandId = 2, ProductTypeId = 4, Name = "Soave", Price = 1.56M },
                new Product { ProductBrandId = 2, ProductTypeId = 4, Name = "Magia", Price = 1.45M },
                new Product { ProductBrandId = 2, ProductTypeId = 4, Name = "Intenso", Price = 1.46M },
                new Product { ProductBrandId = 4, ProductTypeId = 2, Name = "Monoarabica Brasil", Price = 6.55M },
                new Product { ProductBrandId = 4, ProductTypeId = 2, Name = "Monoarabica Guatemala", Price = 6.50M }
            };
        }
    }
}
