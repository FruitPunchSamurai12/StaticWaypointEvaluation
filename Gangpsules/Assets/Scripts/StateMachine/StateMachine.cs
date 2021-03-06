using System;
using System.Collections.Generic;
using UnityEngine;

public class StateTransition
{
    public IState From;
    public IState To;
    public Func<bool> Condition;

    public StateTransition(IState from, IState to, Func<bool> condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }
}

public class StateMachine
{
    private List<StateTransition> _stateTransitions = new List<StateTransition>();
    private List<StateTransition> _anyStateTransitions = new List<StateTransition>();
    private IState _currentState;
    public IState CurrentState => _currentState;
    public event Action<IState> OnStateChanged;

    public void AddAnyTransition(IState to, Func<bool> condition)
    {
        var stateTransition = new StateTransition(null, to, condition);
        _anyStateTransitions.Add(stateTransition);
    }

    public void AddTransition(IState from, IState to, Func<bool> condition)
    {     
        var stateTransition = new StateTransition(from, to, condition);
        _stateTransitions.Add(stateTransition);
    }

    public void SetState(IState state)
    {
        if (_currentState == state)
        {
            return;
        }
        _currentState?.OnExit();

        _currentState = state;
        Debug.Log($"Changed to state {state}");
        _currentState.OnEnter();
        OnStateChanged?.Invoke(_currentState);
    }

    public void Tick()
    {
        var transition = CheckForTransition();
        Debug.Log("ticking");
        if(transition!=null)
        {           
            SetState(transition.To);
        }
        _currentState.Tick();
    }

    StateTransition CheckForTransition()
    {
        foreach (var transition in _anyStateTransitions)
        {
            if (transition.Condition())
                return transition;
        }
        foreach (var transition in _stateTransitions)
        {
            if (transition.From == _currentState && transition.Condition())
            {
                return transition;
            }
        }

        return null;
    } 
}
