

using FSMsCore.Enums;
using FSMsCore.Interfaces;
using Microsoft.Extensions.Logging;

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
                new IFiniteStateMachineFlow([(int)TransferState.Padding], OnNew)
            },
            {
                (int)TransferState.Padding,
                new IFiniteStateMachineFlow([], OnPadding)
            }
        };

    private readonly ILogger<TransferStateMachineFlowDictionary> logger;
    public TransferStateMachineFlowDictionary(ILogger<TransferStateMachineFlowDictionary> logger) {
        this.logger = logger;
    }

    public async Task<IFiniteStateMachine> OnNew(IFiniteStateModel transfer, CancellationToken cancellationToken)
    {
        await Task.Yield();
        // order.StateRemark += $"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Order State Changed to OnNew";
        logger.LogInformation($"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Transfer State {(TransferState)transfer.CurrentState} to {TransferState.Padding}");
        return new TransferStateMachine(transfer, (int)TransferState.Padding);
    }

    public async Task<IFiniteStateMachine> OnPadding(IFiniteStateModel transfer, CancellationToken cancellationToken)
    {
        await Task.Yield();
        logger.LogInformation($"{Environment.NewLine} ({DateTime.UtcNow:YYYYMMddHHmmss}): Transfer State {(TransferState)transfer.CurrentState} to {FiniteState.Done}");
        return new TransferStateMachine(transfer, (int)FiniteState.Done);
    }
}