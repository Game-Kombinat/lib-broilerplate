using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools.Fsm {
    /// <summary>
    /// Simple state machine to make it easy on our game loops management.
    /// </summary>
    public class StateMachine<TStateId> where TStateId : IComparable{ // want a class constraint here to avoid boxing allocations when switching states 

        private FsmState<TStateId> currentState;
        private readonly Dictionary<TStateId, FsmState<TStateId>> states = new Dictionary<TStateId, FsmState<TStateId>>();
        public TStateId CurrentState => currentState != null ? currentState.id : default;

        public TStateId PreviousState {
            get;
            protected set;
        }

        public bool StateNeedsTicking => currentState != null && currentState.tick != null;

        public void Add(TStateId state, StateFunc onEnter, StateFunc update, StateFunc onExit) {
            states.Add(state, new FsmState<TStateId>(state, onEnter, update, null, onExit));
        }
        public void Add(TStateId state, StateFunc onEnter, StateFunc update, StateFunc lateUpdate, StateFunc onExit) {
            states.Add(state, new FsmState<TStateId>(state, onEnter, update, lateUpdate, onExit));
        }

        public void Tick() {
            currentState?.tick?.Invoke();
        }

        public void Terminate() {
            currentState?.exit?.Invoke();
            currentState = null;
        }

        public void SwitchTo(TStateId state, bool reEnter = false) {
            if (!states.ContainsKey(state)) {
                throw new StateMachineException("Trying to switch to unknown state " + state);
            }

            if (currentState != null) {
                bool wouldReEnter = currentState.id.CompareTo(state) == 0;
                if (wouldReEnter && !reEnter) {
                    Debug.LogWarning($"Trying to switch to {state} but that is already current state");
                    return;
                }

                if (wouldReEnter) {
                    // Re-Enter will not update previous state. Obviously.
                    ReEnterState();
                    return;
                }
            }

            var newState = states[state];
            currentState?.exit?.Invoke();
            if (currentState != null) {
                PreviousState = currentState.id;
            }
            
            newState.enter?.Invoke();
            currentState = newState;
        }

        public void SwitchToPreviousState() {
            SwitchTo(PreviousState);
        }

        private void ReEnterState() {
            currentState?.exit?.Invoke();
            currentState?.enter?.Invoke();
        }
    }
}