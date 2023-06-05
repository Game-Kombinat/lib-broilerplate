using System;
using System.Collections.Generic;

namespace Broilerplate.Tools.Fsm.Transitional {
    /// <summary>
    /// A state machine like the other one but with automatic transitions.
    /// </summary>
    public class TransitionalFsm<TStateId> {

        private readonly Dictionary<TStateId, List<FsmTransition<TStateId>>> transitions = new Dictionary<TStateId, List<FsmTransition<TStateId>>>();
        private readonly Dictionary<TStateId, FsmState<TStateId>> stateList = new Dictionary<TStateId, FsmState<TStateId>>();
        private List<FsmTransition<TStateId>> currentTransitions = new List<FsmTransition<TStateId>>();
        private readonly List<FsmTransition<TStateId>> transitionsFromAnyState = new List<FsmTransition<TStateId>>();
        private static readonly List<FsmTransition<TStateId>> EmptyTransitionList = new List<FsmTransition<TStateId>>(0);

        private FsmState<TStateId> currentState;

        private TStateId defaultState;
        public TStateId CurrentState => currentState != null ? currentState.id : default;

        public void Prepare() {
            SetState(GetState(defaultState));
        }

        public void Tick() {
            var transition = GetTransition();
            if (transition != null) {
                SetState(transition.To);
            }
            currentState?.tick?.Invoke();
        }

        public void LateTick() {
            // no transitioning in late tick.
            // if tick happened, lateTick should happen too for consistency and predictability.
            currentState?.lateTick?.Invoke();
        }

        /// <summary>
        /// First add the states of this state machine. Use this method to add a state
        /// identified by a state ID and 3 callbacks to cover all execution points.
        /// Each of them can be null.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="enter"></param>
        /// <param name="tick"></param>
        /// <param name="exit"></param>
        public void AddState(TStateId state, StateFunc enter, StateFunc tick, StateFunc exit) {
            stateList.Add(state, new FsmState<TStateId>(state, enter, tick, null, exit));
        }
        
        public void AddState(TStateId state, StateFunc enter, StateFunc tick, StateFunc lateTick, StateFunc exit) {
            stateList.Add(state, new FsmState<TStateId>(state, enter, tick, lateTick, exit));
        }

        /// <summary>
        /// Add a transition between two known states.
        /// To make a state known, use AddState.
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddTransition(TStateId from, TStateId to, Func<bool> predicate) {
            AddTransition(from, GetState(to), predicate);
        }

        /// <summary>
        /// Add a transition from any state to the given target state.
        /// The state must be known. TO make a state known, use AddState.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddAnyTransition(TStateId to, Func<bool> predicate) {
            AddAnyTransition(GetState(to), predicate);
        }

        public void SetDefaultState(TStateId defaultStateId) {
            defaultState = defaultStateId;
        }

        private FsmState<TStateId> GetState(TStateId stateId) {
            if (stateList.TryGetValue(stateId, out FsmState<TStateId> result)) {
                return result;
            }

            throw new StateMachineException($"Tried accessing unknown state: {stateId}");
        }


        private void SetState(FsmState<TStateId> state)
        {
            if (state == currentState) {
                return;
            }

            currentState?.exit?.Invoke();
            currentState = state;

            transitions.TryGetValue(currentState.id, out currentTransitions);
            if (currentTransitions == null) {
                currentTransitions = EmptyTransitionList;
            }

            currentState?.enter?.Invoke();
        }

        private void AddTransition(TStateId from, FsmState<TStateId> to, Func<bool> predicate)
        {
            if (transitions.TryGetValue(from, out var targetTransitions) == false)
            {
                targetTransitions = new List<FsmTransition<TStateId>>();
                transitions[from] = targetTransitions;
            }

            targetTransitions.Add(new FsmTransition<TStateId>(to, predicate));
        }

        private void AddAnyTransition(FsmState<TStateId> state, Func<bool> predicate)
        {
            transitionsFromAnyState.Add(new FsmTransition<TStateId>(state, predicate));
        }

        private FsmTransition<TStateId> GetTransition() {
            for (var i = 0; i < transitionsFromAnyState.Count; i++) {
                var transition = transitionsFromAnyState[i];
                if (transition.Condition()) {
                    return transition;
                }
            }

            for (var i = 0; i < currentTransitions.Count; i++) {
                var transition = currentTransitions[i];
                if (transition.Condition()) {
                    return transition;
                }
            }

            return null;
        }
    }
}