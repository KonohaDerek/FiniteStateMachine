
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FSMsCore.Interfaces;

public interface IFiniteStateMachineFlow
{
    public  List<int> NextState {get;}

    public  Func<IFiniteStateModel, CancellationToken, Task<IFiniteStateMachine>> EventAction {get;}
}

public interface IFiniteStateMachineFlowDictionary {
    public string Name { get; }

    public Dictionary<int, IFiniteStateMachineFlow> Flows {get;}
}

