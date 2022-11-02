namespace Broilerplate.Gameplay.Input {
    public delegate void ButtonPress();
    public class AxisInputData<T> {
        public delegate void AxisInput(T value);
        public event AxisInput InputCallbacks;
        public T lastInput;
        
        public void Invoke() {
            InputCallbacks?.Invoke(lastInput);
        }

        public void UpdateInput(T newInput) {
            lastInput = newInput;
        }
    }
}