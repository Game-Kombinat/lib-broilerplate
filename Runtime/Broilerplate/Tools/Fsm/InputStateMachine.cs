using System.Collections.Generic;

namespace Broilerplate.Tools.Fsm {
    /// <summary>
    /// State Machine specifically designed to process player input.
    /// It's mostly the same as the normal StateMachine but instead of a leave
    /// callback we have a fixedTick callback as inputs need to be handled and managed
    /// in tick and fixedTick to avoid all kinds of ugly-ass jitter
    /// </summary>
    public class InputStateMachine {
        public delegate void StateFunc();
        
        private State currentState;
        private readonly Dictionary<int, State> states = new Dictionary<int, State>();
        public int CurrentState => HasCurrentState ? currentState.id : -1;
        public bool HasCurrentState => currentState != null;
        
        public void Add(int state, StateFunc onEnter, StateFunc tick, StateFunc lateTick, StateFunc exit) {
            states.Add(state, new State(state, onEnter, tick, lateTick, exit));
        }

        public void Tick() {
            currentState?.tick?.Invoke();
        }

        public void LateTick() {
            currentState?.lateTick?.Invoke();
        }

        public void Terminate() {
            currentState?.exit?.Invoke();
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
            currentState?.exit?.Invoke();
            newState.enter?.Invoke();
            currentState = newState;
        }
        
        private class State {
            public State(int id, StateFunc enter, StateFunc tick, StateFunc lateTick, StateFunc exit) {
                this.id = id;
                this.enter = enter;
                this.tick = tick;
                this.lateTick = lateTick;
                this.exit = exit;
            }

            public readonly int id;
            public readonly StateFunc enter;
            public readonly StateFunc tick;
            public readonly StateFunc lateTick;
            public readonly StateFunc exit;
        }
    }
}