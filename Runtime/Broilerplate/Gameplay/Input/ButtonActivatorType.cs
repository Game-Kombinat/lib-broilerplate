namespace Broilerplate.Gameplay.Input {
    public enum ButtonActivatorType {
        Press,
        Holding, // in unity input system this is called "Performed". But its actually "holding" an action down
        Release,
        DoublePress, // custom behaviour to get notifications for double presses / clicks / taps
        Tap, // custom behaviour to get notifications for actual taps (press down and back up with a given timeout)
    }
}