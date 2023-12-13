
using FSMsCore;
using FSMsCore.Interfaces;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FSMsTests;

[TestClass]
public class TransferFSMTests
{

    [TestMethod]
    public async Task TransferFSMNewToPadding()
    {
        var transfer = new Transfer()
        {
            Id = 1,
            CurrentState = (int)TransferState.New,
            Name = "測試"
        };

        var container = new Container(cfg =>
     {
         cfg.Scan(scanner =>
         {
             scanner.AssemblyContainingType(typeof(Transfer));
             scanner.IncludeNamespaceContainingType<TransferStateMachine>();
             scanner.WithDefaultConventions();
             scanner.AddAllTypesOf(typeof(IRequestHandler<>));
             scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
         });
         cfg.AddLogging();
         cfg.For<IMediator>().Use<Mediator>();
         cfg.For<IFiniteStateMachineFlowDictionary>().Add<TransferStateMachineFlowDictionary>();
     });
        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new TransferStateMachine(transfer, (int)TransferState.Padding));
        Assert.AreEqual(response.CurrentState, (int)TransferState.Padding);
    }
}
