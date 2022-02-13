using System.Collections.Generic;
using System;

namespace Shared.FSM {
    public class StateController {
        public IState CurrentState { get; private set; }
        Dictionary<Type, List<Transition>> _allTransitions 
            = new Dictionary<Type, List<Transition>>();
        List<Transition> _currentTransitions = new List<Transition>();
        List<Transition> _highPriorityTransitions = new List<Transition>();
        List<Transition> _emptyTransitions = new List<Transition>();

        public void Update(float deltaTime) {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            CurrentState?.UpdateState(deltaTime);
        }

        public void AddTransition(IState from, IState to, Func<bool> condition) {
            if (_allTransitions.TryGetValue(from.GetType(), out var transitions) == false) {
                transitions = new List<Transition>();
                _allTransitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, condition));
        }

        public void AddHighPriorityTransition(IState to, Func<bool> condition) {
            _highPriorityTransitions.Add(new Transition(to, condition));
        }

        public void SetState(IState state) {
            if (state == CurrentState)
                return;

            CurrentState?.OnExit();
            CurrentState = state;

            _allTransitions.TryGetValue(CurrentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = _emptyTransitions;

            CurrentState?.OnEnter();
        }

        Transition GetTransition() {
            foreach (var transition in _highPriorityTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in _currentTransitions) {
                if (transition.Condition())
                    return transition;
            }

            return null;
        }

        private class Transition {
            public IState To { get; }
            public Func<bool> Condition { get; }

            public Transition(IState to, Func<bool> condition) {
                To = to;
                Condition = condition;
            }
        }
    }
}