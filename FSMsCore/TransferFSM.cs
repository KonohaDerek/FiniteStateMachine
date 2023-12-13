

using FSMsCore.Enums;
using FSMsCore.Interfaces;

namespace FSMsCore;

public enum TransferState
{
    Done = -1,
    New,
    Padding,
    Completed
}

public class Transfer : IFiniteStateModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CurrentState { get; set; }
}

public class TransferStateMachine(IFiniteStateModel transfer, int nextState) : IFiniteStateMachine
{
    public IFiniteStateModel Model => transfer;

    public int NextState => nextState;
}

public class TransferStateMachineFlowDictionary : IFiniteStateMachineFlowDictionary
{
    public string Name => nameof(Transfer);

    public Dictionary<int, IFiniteStateMachineFlow> Flows => new Dictionary<int, IFiniteStateMachineFlow>(){
            {
                (int)TransferState.New,
                new TransferStateMachineFlow([(int)TransferState.Padding], TransferStateMachineFlow.OnNew)
            },
            {
                (int)TransferState.Padding,
                new TransferStateMachineFlow([], TransferStateMachineFlow.OnPadding)
            }
        };
}

public class TransferStateMachineFlow(List<int> nextStates, Func<IFiniteStateModel, CancellationToken, Task<IFiniteStateMachine>> eventAction) : IFiniteStateMachineFlow
{
    public List<int> NextState => nextStates;

    public Func<IFiniteStateModel, CancellationToken, Task<IFiniteStateMachine>> EventAction => eventAction;

    public static async Task<IFiniteStateMachine> OnNew(IFiniteStateModel transfer, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnNew";
        return new TransferStateMachine(transfer, (int)TransferState.Padding);
    }

    public static async Task<IFiniteStateMachine> OnPadding(IFiniteStateModel transfer, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnNew";
        return new TransferStateMachine(transfer, (int)FiniteState.Done);
    }
}