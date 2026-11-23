using Engine.Ui;
using Engine.Actors;
using Engine.Shared;
using Engine.World;
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

    /// <summary>
    /// The buttons this game's own actions are bound to until the player chooses otherwise, by the action's name and
    /// written as <see cref="Options"/> writes bindings. None, unless the game says.
    /// </summary>
    public virtual Dictionary<string, List<string>> StandardBindings => [];

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

    /// <summary>The scene of the player. Its root is a <see cref="Player"/>.</summary>
    [Export]
    public PackedScene? PlayerScene { get; set; }

    /// <summary>The scene of the room a new game starts in. Its root is a <see cref="Room"/>.</summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string FirstRoom { get; set; } = "";

    /// <summary>The name of the entry point in that room where the player starts.</summary>
    [Export]
    public string FirstEntryPoint { get; set; } = "";

    /// <summary>
    /// The scene of every room in the game. As the game loads, the names their placed things are remembered by are
    /// checked across all of them.
    /// </summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string[] Rooms { get; set; } = [];

    /// <summary>
    /// Checks the names things are remembered by across every room in <see cref="Rooms"/>: none is shared, and nothing
    /// that must be remembered is without one.
    /// </summary>
    /// <exception cref="InvalidOperationException">A name is shared, or something that must be remembered has none.</exception>
    public void CheckRememberedNames()
    {
        var names = new RememberedNames();
        foreach (string scene in Rooms)
        {
            Room room = CreateRoom(scene);
            names.Add(scene, room);
            room.Free();
        }

        names.Require();
    }

    /// <summary>How many save slots the game has.</summary>
    [Export]
    public int SaveSlots { get; set; } = 3;

    /// <summary>The folder, within the assets tree, of the game's words: one file per language, named for it.</summary>
    [Export]
    public string TextFolder { get; set; } = "text";

    /// <summary>The folder, within the assets tree, of the game's sounds: one file per sound, named for it.</summary>
    [Export]
    public string SoundFolder { get; set; } = "sounds";

    /// <summary>The folder, within the assets tree, of the game's music: one file per piece, named for it.</summary>
    [Export]
    public string MusicFolder { get; set; } = "music";

    /// <summary>How many updates the change from one room to the next takes.</summary>
    [Export]
    public int TransitionUpdates { get; set; } = 30;

    /// <summary>Makes the mode laid over play when the player pauses.</summary>
    public virtual GameMode CreatePauseMode(PlayingMode playing) => new PausedMode(playing);

    /// <summary>Makes the mode that follows the player's death. The engine's reloads the session's save slot, or starts a new game.</summary>
    public virtual GameMode CreateModeAfterDeath() => new LoadingMode();

    /// <summary>The scene of what is shown over play. Its root is a <see cref="GameHud"/>. None for a game that shows nothing.</summary>
    [Export]
    public PackedScene? Hud { get; set; }

    /// <summary>Makes what is shown over play, or null when the game shows nothing.</summary>
    /// <exception cref="InvalidOperationException">The scene's root is not a HUD.</exception>
    public virtual GameHud? CreateHud()
    {
        if (Hud is null)
            return null;
        Node root = Hud.Instantiate();
        return root as GameHud
            ?? throw new InvalidOperationException($"The HUD scene's root is a {root.GetType().Name}, not a GameHud.");
    }

    /// <summary>Makes the session-long services this game adds, one of each. None, unless the game says.</summary>
    public virtual IReadOnlyList<ISessionService> CreateServices() => [];

    /// <summary>Makes the session's player.</summary>
    /// <exception cref="InvalidOperationException">No player scene is set, or its root is not a player.</exception>
    public virtual Player CreatePlayer()
    {
        PackedScene scene = PlayerScene ?? throw new InvalidOperationException("The game definition names no player scene.");
        Node root = scene.Instantiate();
        return root as Player
            ?? throw new InvalidOperationException($"The player scene's root is a {root.GetType().Name}, not a Player.");
    }

    /// <summary>Makes the room at <paramref name="scenePath"/>.</summary>
    /// <exception cref="InvalidOperationException">The scene does not exist, or its root is not a room.</exception>
    public virtual Room CreateRoom(string scenePath)
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(scenePath)
            ?? throw new InvalidOperationException($"There is no room scene at \"{scenePath}\".");
        Node root = scene.Instantiate();
        return root as Room
            ?? throw new InvalidOperationException($"The scene \"{scenePath}\" has a {root.GetType().Name} at its root, not a Room.");
    }

    /// <summary>
    /// Makes the sequence a new session's random numbers come from. The engine's plain sequence unless the game needs a
    /// particular one.
    /// </summary>
    public virtual IRandomSequence CreateRandomSequence() => new PlainRandomSequence(RandomSeed);
}
