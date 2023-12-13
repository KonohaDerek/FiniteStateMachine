using System.Collections.Concurrent;
using FSMsCore.Enums;
using FSMsCore.Interfaces;
using FSMsCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FSMsCore;


public class OrderStateMachine(Order order, OrderState nextState) : IRequest<Order>
{
    public Order Order { get; } = order;

    public OrderState NextState { get; } = nextState;
}

public class OrderFlow(List<OrderState> nextState, Func<Order, CancellationToken, Task<OrderStateMachine>> eventAction)
{
    public List<OrderState> NextState { get; } = nextState;
    public Func<Order, CancellationToken, Task<OrderStateMachine>> EventAction { get; } = eventAction;

    public static async Task<OrderStateMachine> OnNew(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnNew";
        return new OrderStateMachine(order, OrderState.Padding);
    }

    public static async Task<OrderStateMachine> OnPadding(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnPadding";
        return new OrderStateMachine(order, OrderState.Done);
    }

    public static async Task<OrderStateMachine> OnPaySuccess(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnPaySuccess";
        return new OrderStateMachine(order, OrderState.Completed);
    }

    public static async Task<OrderStateMachine> OnPayFailed(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnPayFailed";
        return new OrderStateMachine(order, OrderState.Done);
    }

    public static async Task<OrderStateMachine> OnCompleted(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // Update User Subscribe Info 
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnCompleted";
        // order.ComplatedAt = DateTime.UtcNow;
        return new OrderStateMachine(order, OrderState.Done);
    }

    public static async Task<OrderStateMachine> OnCanceled(Order order, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnCanceled";
        // order.CanceledAt = DateTime.UtcNow;
        return new OrderStateMachine(order, OrderState.Done);
    }
}

public class OrderStateMachineHandler(IMediator mediator,
    ILogger<OrderStateMachineHandler> logger) : IRequestHandler<OrderStateMachine, Order>
{
    private readonly Dictionary<OrderState, OrderFlow> orderFlows = new()
    {
           {
               OrderState.New,
               new OrderFlow([OrderState.Padding], OrderFlow.OnNew)
           },
           {
               OrderState.Padding,
               new OrderFlow( [OrderState.PaySuccess, OrderState.PayFailed], OrderFlow.OnPadding)
           },
           {
               OrderState.PaySuccess,
               new OrderFlow([OrderState.Completed], OrderFlow.OnPaySuccess)},
           {
               OrderState.PayFailed,
               new OrderFlow([],OrderFlow.OnPayFailed)
           },
           {
               OrderState.Canceled,
               new OrderFlow([], OrderFlow.OnCanceled)
           },
           {
               OrderState.Completed,
               new OrderFlow([], OrderFlow.OnCompleted)
           }
    };

    private readonly IMediator mediator = mediator;

    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> locks = new();

    private readonly ILogger<OrderStateMachineHandler> logger = logger;

    public async Task<Order> Handle(OrderStateMachine notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("OrderStateMachineHandler.Handle");
        //var lockObject = locks.GetOrAdd(notification.Order.GId, guid => new SemaphoreSlim(0, 1));
        var order = notification.Order;
        try
        {
            // 檢查狀態是否在 Order FMS 狀態之中
            if (!orderFlows.ContainsKey(order.CurrentState))
            {
                throw new InvalidOperationException($"Invalid state: {order.CurrentState}");
            }
            // 檢查 NextState
            if (orderFlows[order.CurrentState].NextState.Any(o => o == notification.NextState))
            {
                // 執行State Event Action 並取得下一階段的State
                var stateChanged = await orderFlows[notification.NextState].EventAction(order, cancellationToken);
                // 更新要進入的狀態
                order.CurrentState = notification.NextState;
                // await orderRepository.UpdateAsync(order, cancellationToken);
                // 如果 stateChanged 狀態為Done 表示不處理
                if (stateChanged.NextState == OrderState.Done)
                {
                    return order;
                }
                else
                {
                    order = await mediator.Send(stateChanged, cancellationToken);
                }
                logger.LogInformation($"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed From {notification.Order.CurrentState} to {order.CurrentState}");
            }
            else
            {
                throw new InvalidOperationException($"Invalid nextState state: cannot transition from {order.CurrentState} to {notification.NextState}");
            }
        }
        finally
        {
            //lockObject.Release();
        }
        return order;
    }
}
