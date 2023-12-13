using System.Collections.Concurrent;
using FSMsCore.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FSMsCore.Interfaces;


public interface IFiniteStateModel {
    public int CurrentState{get;set;}
}

public interface IFiniteStateMachine: IRequest<IFiniteStateModel>
{
    public IFiniteStateModel Model { get; } 

    public int NextState { get; }
}

public class FiniteStateMachineHandler : IRequestHandler<IFiniteStateMachine, IFiniteStateModel>
{
    private readonly ILogger<OrderStateMachineHandler> logger;
    private readonly IMediator mediator;
    private readonly IList<IFiniteStateMachineFlowDictionary> modelFlows;

    public FiniteStateMachineHandler(IMediator mediator,
    ILogger<OrderStateMachineHandler> logger, IList<IFiniteStateMachineFlowDictionary> modelFlows) {
        this.mediator = mediator;
        this.logger = logger;
        this.modelFlows = modelFlows;
    }

    public async Task<IFiniteStateModel> Handle(IFiniteStateMachine request, CancellationToken cancellationToken)
    {
        Console.WriteLine("FiniteStateMachineHandler.Handle");
        //var lockObject = locks.GetOrAdd(notification.Order.GId, guid => new SemaphoreSlim(0, 1));
        var oriModel = request.Model;
        var nextState = request.NextState;
        var name = oriModel.GetType().Name;
        var flows = modelFlows.FirstOrDefault(f=> f.Name.Equals(name))?.Flows;
       if (flows is null) {
        return oriModel;
       }

        try
        {
            // 檢查狀態是否在 Order FMS 狀態之中
            if (!flows.ContainsKey(oriModel.CurrentState))
            {
                throw new InvalidOperationException($"Invalid state: {oriModel.CurrentState}");
            }
            // 檢查 NextState
            if (flows[oriModel.CurrentState].NextState.Any(o => o == nextState))
            {
                // 執行State Event Action 並取得下一階段的State
                var stateMachine = await flows[nextState].EventAction(oriModel, cancellationToken);
                // 更新要進入的狀態
                oriModel.CurrentState = request.NextState;
                // 如果 stateMachine 狀態為Done 表示不處理
                if (stateMachine.NextState == (int)FiniteState.Done)
                {
                    return oriModel;
                }
                else
                {
                    oriModel = await mediator.Send(stateMachine, cancellationToken);
                }
                logger.LogInformation($"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): State Machine From {request.Model.CurrentState} to {oriModel.CurrentState}");
            }
            else
            {
                throw new InvalidOperationException($"Invalid nextState state: cannot transition from {oriModel.CurrentState} to {request.NextState}");
            }
        }
        finally
        {
            //lockObject.Release();
        }

        return oriModel;
    }
}



