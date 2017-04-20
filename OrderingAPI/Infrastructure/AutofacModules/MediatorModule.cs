using System.Linq;
using Autofac;
using MediatR;
using System.Reflection;
using OrderingAPI.Application.Commands;
using Autofac.Core;
using OrderingAPI.Application.DomainEventHandlers;

namespace OrderingAPI.Infrastructure.AutofacModules
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(Mediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(CreateOrderCommand).GetTypeInfo().Assembly)
                .As(o => o.GetInterfaces()
                    .Where(i => i.IsClosedTypeOf(typeof(IAsyncRequestHandler<,>)))
                    .Select(i => new KeyedService("IAsyncRequestHandler", i)));
            builder.RegisterAssemblyTypes(typeof(ValidateOrAddClientAggregateWhenOrderStartedDomainEventHandler).GetTypeInfo().Assembly);

        }
    }
}
