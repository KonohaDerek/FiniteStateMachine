using FSMsCore;
using FSMsCore.Models;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FSMsTests;

[TestClass]
public class OrerFSMTests
{
    [TestMethod]
    public async Task OrerOrerFSMNewToPadding()
    {
        var order = new Order(){
            Id = 1,
            CurrentState = FSMsCore.Enums.OrderState.New
        };

        var container = new Container(cfg =>
     {
         cfg.Scan(scanner =>
         {
             scanner.AssemblyContainingType(typeof(Order));
             scanner.IncludeNamespaceContainingType<OrderStateMachine>();
             scanner.WithDefaultConventions();
             scanner.AddAllTypesOf(typeof(IRequestHandler<>));
             scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
       
         });
         cfg.AddLogging();
         cfg.For<IMediator>().Use<Mediator>();
     });

        var mediator = container.GetInstance<IMediator>();
        
        var response = await mediator.Send(new OrderStateMachine(order , FSMsCore.Enums.OrderState.Padding));
        Assert.AreEqual(response.CurrentState , FSMsCore.Enums.OrderState.Padding);
    }

    [TestMethod]
    public async Task OrerOrerFSMPaddingToPaySuccess()
    {
        var order = new Order()
        {
            Id = 1,
            CurrentState = FSMsCore.Enums.OrderState.Padding
        };

        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(Order));
                scanner.IncludeNamespaceContainingType<OrderStateMachine>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));

            });
            cfg.AddLogging();
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new OrderStateMachine(order, FSMsCore.Enums.OrderState.PaySuccess));
        Assert.AreEqual(response.CurrentState, FSMsCore.Enums.OrderState.PaySuccess);
    }
}
