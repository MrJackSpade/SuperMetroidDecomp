using Engine.Shared;
using Godot;

namespace Engine.Core;

/// <summary>
/// The game the engine is running. It is the one thing the engine is pointed at, by the project setting
/// "engine/game_definition", and everything the engine knows about a particular game it learns from here.
/// </summary>
[GlobalClass]
public partial class GameDefinition : Resource
{
    /// <summary>The scene of the first mode: the screen the game starts on. Its root is a <see cref="GameMode"/>.</summary>
    [Export]
    public PackedScene? FirstMode { get; set; }

    /// <summary>
    /// The names of the actions this game adds to the engine's own. Each is also the name of an action in Godot's input
    /// map, where the player's buttons are bound to it.
    /// </summary>
    [Export]
    public string[] Actions { get; set; } = [];

    /// <summary>The number a new session's random sequence starts from.</summary>
    [Export]
    public uint RandomSeed { get; set; } = 1;

    /// <summary>Makes the mode a new session starts in.</summary>
    /// <exception cref="InvalidOperationException">No first mode is set, or its scene's root is not a mode.</exception>
    public virtual GameMode CreateFirstMode()
    {
        PackedScene scene = FirstMode ?? throw new InvalidOperationException("The game definition names no first mode.");
        Node root = scene.Instantiate();
        return root as GameMode
            ?? throw new InvalidOperationException($"The first mode's scene root is a {root.GetType().Name}, not a GameMode.");
    }

    /// <summary>
    /// Makes the sequence a new session's random numbers come from. The engine's plain sequence unless the game needs a
    /// particular one.
    /// </summary>
    public virtual IRandomSequence CreateRandomSequence() => new PlainRandomSequence(RandomSeed);
}
