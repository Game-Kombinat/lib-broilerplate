# Broilerplate
### The boilerplate with the opinion

Hello. You see here a piece of library code that aims to put some heavy opinion into Unity,
in terms of how you will be setting up your game code and data structure and workflows.

This is taking all the leaves out of Unreals book and you see me, trying to make that work
in Unity so it will make sense and give the user the benefit of a structured and well defined
execution path from boot to destroy, while at the same time not completely working against
the strengths that Unity has on offer.

What I'm doing here is a very naive implementation of a very small subset of Unreals game framework, re-imagined for Unity.

### How does this work, how is it done?

We begin with adding a single-relevant entry point of the application.
Unity offers the `RuntimeInitializeOnLoadMethod` attribute. The class `GameInstace` has
a method decorated with this attribute. It will spawn a game instance after the first scene was loaded.

Which one depends on the project configuration.

The GameInstance will then boot up a World object. Which one, again, depends on your configuration.
The world will spawn a `GameMode` which may contain data and logic relevant to your gameplay.
Which `GameMode` type it is in detail, depends on your configuration.

After this is bootstrapped, the world will use the `GameMode` to spawn a player controller,
a player pawn which the player controller will take control of.

The types of `PlayerController` and `Pawn` that are spawned depend on your configuration.


TBC when I have time. But at that point, surely you get the picture.
But if you're in doubt about the usefulness of this: Trust me. You will love it.