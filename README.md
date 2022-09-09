# Broilerplate
### The boilerplate with the opinion

Hello. You see here a piece of library code that aims to put some heavy opinion into Unity,
in terms of how you will be setting up your game code and data structure and workflows.

This is taking all the leaves out of Unreals book and you see me, trying to make that work
in Unity so it will make sense and give the user the benefit of a structured and well defined
execution path from boot to destroy, while at the same time not completely working against
the strengths that Unity has on offer.

What I'm doing here is a very naive implementation of a very small subset of Unreals game framework, re-imagined for Unity.
Conversely this code may or may not change in inconvenient ways in the future.

### What is this good for?
* Single player games
* Big single player games
* Small single player games
* 2D or 3D, doesn't really matter

### What is this not good for?
* Networked games. I have plans for that but currently no time.


### How does this work, how is it done?
I'll eventually put together some documentation on how to work this code.
If you're curious, it's not super complicated to figure out. 

Look at the `GameInstance` class which has an entry point for the framework defined.
It's used to bootstrap a `World` and from there a `GameMode`. 
You define what exactly those are in  the `BroilerConfiguration` class which 
is a `ScriptableObject` that contains all the necessary details to bootstrap a game.