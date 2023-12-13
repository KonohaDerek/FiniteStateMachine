
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FSMsCore.Interfaces;

public record class IFiniteStateMachineFlow(List<int> NextState, Func<IFiniteStateModel, CancellationToken, Task<IFiniteStateMachine>> EventAction);
public interface IFiniteStateMachineFlowDictionary {
    public string Name { get; }

    public Dictionary<int, IFiniteStateMachineFlow> Flows {get;}
}

