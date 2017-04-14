using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductAPI.InfraStructure;
using ProductAPI.IntegrationEvents;
using ProductAPI.IntegrationEvents.Events;
using ProductAPI.Model;
using ProductAPI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductContext _productContext;
        private readonly IOptionsSnapshot<Settings> _settings;
        private readonly IProductIntegrationEventService _productIntegrationEventService;

        public ProductController(ProductContext context, IOptionsSnapshot<Settings> settings, IProductIntegrationEventService productIntegrationEventService)
        {
            _productContext = context;
            _productIntegrationEventService = productIntegrationEventService;
            _settings = settings;

            ((DbContext)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
        {
            var totalItems = await _productContext.Products
                .LongCountAsync();

            var itemsOnPage = await _productContext.Products
                .OrderBy(c => c.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            var model = new PaginatedItemsViewModel<Product>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ProductTypes()
        {
            var items = await _productContext.ProductTypes.ToListAsync();

            return Ok(items);
        }

        [HttpGet]
        public async Task<IActionResult> ProductBrands()
        {
            var items = await _productContext.ProductBrands.ToListAsync();

            return Ok(items);
        }

        [Route("update")]
        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody]Product productToUpdate)
        {
            var product = await _productContext.Products.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);
            if (product == null) return NotFound();
            var raiseProductPriceChangeEvent = product.Price != productToUpdate.Price;
            var oldPrice = product.Price;

            product = productToUpdate;
            _productContext.Products.Update(product);

            if (raiseProductPriceChangeEvent)
            {
                var priceChangeEvent = new ProductPriceChangedIntegrationEvent(product.Id, productToUpdate.Price, oldPrice);
                await _productIntegrationEventService.SaveEventAndProductContextChangesAsync(priceChangeEvent);
                await _productIntegrationEventService.PublishThroughEventBusAsync(priceChangeEvent);
            }
            else
            {
                await _productContext.SaveChangesAsync();
            }

            return Ok();
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody]Product product)
        {
            _productContext.Products.Add(
                new Product
                {
                    ProductBrandId = product.ProductBrandId,
                    ProductTypeId = product.ProductTypeId,
                    Name = product.Name,
                    Price = product.Price
                });
            await _productContext.SaveChangesAsync();

            return Ok();
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _productContext.Products.SingleOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _productContext.Products.Remove(product);
            await _productContext.SaveChangesAsync();

            return Ok();
        }
    }
}
