using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools.Fsm {
    /// <summary>
    /// It's what it says on the tin.
    /// It's a state machine. The specialty here is that it has no transitions.
    /// We have a bit of an anti-pattern going on here.
    /// Use this for scenarios where you want managed states (enter / exit) but don't need transitions in particular.
    /// The type parameter TStateId is being used as the concrete type to identify a state.
    /// For instance, it could be an enum. Or anything that implements IComparable, really.
    /// </summary>
    public class StateMachine<TStateId> where TStateId : IComparable { // want a class constraint here to avoid boxing allocations when switching states 

        /// <summary>
        /// The currently active state.
        /// This can be null if the state machine isn't running yet or anymore.
        /// </summary>
        private FsmState<TStateId> currentState;
        
        /// <summary>
        /// Dictionary of states that exist in this state machine.
        /// </summary>
        private readonly Dictionary<TStateId, FsmState<TStateId>> states = new Dictionary<TStateId, FsmState<TStateId>>();
        
        /// <summary>
        /// Retrieves the identifier object for the current state.
        /// </summary>
        public TStateId CurrentState => currentState != null ? currentState.id : default;

        /// <summary>
        /// Retrieves the identifier object for the previous state.
        /// </summary>
        public TStateId PreviousState {
            get;
            protected set;
        }


        /// <summary>
        /// Add a state with enter, update and exit callbacks, identified by the value of the state parameter.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="onEnter"></param>
        /// <param name="update"></param>
        /// <param name="onExit"></param>
        public void Add(TStateId state, StateFunc onEnter, StateFunc update, StateFunc onExit) {
            states.Add(state, new FsmState<TStateId>(state, onEnter, update, null, onExit));
        }
        
        /// <summary>
        /// Add a state with enter, update, lateUpdate and exit callbacks, identified by the value of the state parameter.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="onEnter"></param>
        /// <param name="update"></param>
        /// <param name="lateUpdate"></param>
        /// <param name="onExit"></param>
        public void Add(TStateId state, StateFunc onEnter, StateFunc update, StateFunc lateUpdate, StateFunc onExit) {
            states.Add(state, new FsmState<TStateId>(state, onEnter, update, lateUpdate, onExit));
        }

        /// <summary>
        /// Ticks this state machine by calling tick on the active state.
        /// </summary>
        public void Tick() {
            currentState?.tick?.Invoke();
        }

        /// <summary>
        /// Terminate this state machine.
        /// Calls Exit on the active state and sets currentState to null.
        /// </summary>
        public void Terminate() {
            currentState?.exit?.Invoke();
            currentState = null;
        }

        /// <summary>
        /// Switches to another state. Calls Exit on the old state and Enter on the new one.
        /// Set the previous state.
        /// If reEnter is true, will exit and re-enter the current state.
        /// Otherwise just spit out a warning and bail.
        /// Re-Entering a state will not affect the previous state property.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reEnter"></param>
        /// <exception cref="StateMachineException"></exception>
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

        /// <summary>
        /// Switch to previous state.
        /// </summary>
        public void SwitchToPreviousState() {
            SwitchTo(PreviousState);
        }

        /// <summary>
        /// Handle re-entering a state.
        /// </summary>
        private void ReEnterState() {
            currentState?.exit?.Invoke();
            currentState?.enter?.Invoke();
        }
    }
}