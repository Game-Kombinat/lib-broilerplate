using System.Collections.Generic;

namespace Broilerplate.Tools.Fsm {
    /// <summary>
    /// Simple state machine to make it easy on our game loops management.
    /// </summary>
    public class StateMachine { // want a class constraint here to avoid boxing allocations when switching states 
        public delegate void StateFunc();

        private State currentState;
        private readonly Dictionary<int, State> states = new Dictionary<int, State>();
        public int CurrentState => currentState.id;

        public bool StateNeedsTicking => currentState != null && currentState.tick != null;

        public void Add(int state, StateFunc onEnter, StateFunc update, StateFunc onExit) {
            states.Add(state, new State(state, onEnter, update, onExit));
        }

        public void Tick() {
            currentState?.tick?.Invoke();
        }

        public void Terminate() {
            currentState?.leave?.Invoke();
            currentState = null;
        }

        public void SwitchTo(int state) {
            if (!states.ContainsKey(state)) {
                throw new StateMachineException("Trying to switch to unknown state " + state);
            }

            if (currentState != null && currentState.id == state) {
                throw new StateMachineException("Trying to switch to " + state + " but that is already current state");
            }

            var newState = states[state];
            currentState?.leave?.Invoke();
            newState.enter?.Invoke();
            currentState = newState;
        }

        private class State {
            public State(int id, StateFunc enter, StateFunc tick, StateFunc leave) {
                this.id = id;
                this.enter = enter;
                this.tick = tick;
                this.leave = leave;
            }

            public readonly int id;
            public readonly StateFunc enter;
            public readonly StateFunc tick;
            public readonly StateFunc leave;
        }
    }

    
}