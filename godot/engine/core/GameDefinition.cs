using Engine.Ui;
using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Core;

/// <summary>
/// The game the engine is running. It is the one thing the engine is pointed at, by the project setting
/// "engine/game_definition", and everything the engine knows about a particular game it learns from here.
/// </summary>
[GlobalClass]
public abstract partial class GameDefinition : Resource
{
    /// <summary>
    /// The names of the actions this game adds to the engine's own. Each is also the name of an action in Godot's input
    /// map, where the player's buttons are bound to it.
    /// </summary>
    [Export]
    public string[] Actions { get; set; } = [];

    /// <summary>
    /// The buttons this game's own controls are bound to until the player chooses otherwise, by the control's name
    /// and written as <see cref="Options"/> writes bindings. A control is a button of the game's own controller; an
    /// action the game puts on none of them is on its own, of the action's name.
    /// </summary>
    public abstract Dictionary<string, List<string>> StandardBindings { get; }

    /// <summary>
    /// The control each action is on until the player chooses otherwise, by the action's name, for the actions this
    /// game puts on a control other than their own; none for a game whose every action is on its own.
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> StandardControls { get; }

    /// <summary>Every action there is: the engine's own and this game's.</summary>
    public IReadOnlyList<Engine.Input.GameAction> AllActions =>
        [.. Engine.Input.GameAction.Standard, .. Actions.Select(name => new Engine.Input.GameAction(name))];

    /// <summary>
    /// How an exported game that starts with no assets has them made from a file the player supplies
    /// (<see cref="AssetsSetup"/>); null for a game that only says where its assets belong. None, unless the game says.
    /// </summary>
    public virtual Content.AssetsMaker? AssetsMaker => null;

    /// <summary>Makes the mode a new session starts in: the screen the game starts on.</summary>
    public abstract GameMode CreateFirstMode();

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

    /// <summary>The data file, within the assets tree, of the font the game's words are drawn in; empty for a game that draws no words.</summary>
    [Export]
    public string FontFile { get; set; } = "";

    /// <summary>
    /// The data file, within the assets tree, of how the game's messages are shown: each as a picture in a window
    /// that opens and closes (<see cref="Ui.MessageBoxStyle"/>). A message asked for with none named is refused.
    /// </summary>
    [Export]
    public string MessageBoxFile { get; set; } = "";

    /// <summary>The folder, within the assets tree, of the game's sounds: one file per sound, named for it.</summary>
    [Export]
    public string SoundFolder { get; set; } = "sounds";

    /// <summary>The folder, within the assets tree, of the game's music: one file per piece, named for it.</summary>
    [Export]
    public string MusicFolder { get; set; } = "music";

    /// <summary>Makes the mode laid over play when the player pauses.</summary>
    public abstract GameMode CreatePauseMode(PlayingMode playing);

    /// <summary>
    /// Whether the pause may be asked for on this update of play in <paramref name="room"/>, over and above what play
    /// itself asks of the player.
    /// </summary>
    public abstract bool PauseMayBeAsked(GameSession session, World.Room room);

    /// <summary>Makes the mode a new game begins in: play in the first room, or whatever the game shows first.</summary>
    public abstract GameMode CreateModeForNewGame();

    /// <summary>
    /// Makes the mode that plays the player's death. It takes control on the update the player begins to die, and is
    /// given the play it takes over from, which it may go on playing beneath itself.
    /// </summary>
    public abstract GameMode CreateDeathMode(PlayingMode playing);

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

    /// <summary>
    /// How many pixels short of a screen's bottom the view's bottom comes to rest in a screen that does not align the
    /// view to its top. None, plainly.
    /// </summary>
    [Export]
    public int ViewRestsShortBy { get; set; }

    /// <summary>How many pixels more than its speed the view drifts each update out of, or back from, a screen closed to it.</summary>
    [Export]
    public int ClosedScreenDrift { get; set; } = 2;

    /// <summary>How many levels the screen beneath play goes from fully lit to black in (<see cref="Ui.ScreenDarkness"/>).</summary>
    [Export]
    public int ScreenDarknessLevels { get; set; } = 15;

    /// <summary>
    /// How far to either side of the view an enemy's middle may lie, beyond the enemy's own half-width, and the
    /// enemy still take its turn, in pixels.
    /// </summary>
    [Export]
    public int EnemyTurnMarginAcross { get; set; } = 64;

    /// <summary>How far above the view an enemy's middle may lie and the enemy still take its turn, in pixels.</summary>
    [Export]
    public int EnemyTurnMarginAbove { get; set; } = 64;

    /// <summary>How far below the view an enemy's middle may lie and the enemy still take its turn, in pixels.</summary>
    [Export]
    public int EnemyTurnMarginBelow { get; set; } = 64;

    /// <summary>
    /// How many places a room keeps for what its enemies launch and leave behind, their shots, the remains of the
    /// destroyed and the pickups dropped: each holds one, a thing takes the highest free and is not made when all are
    /// held, and they take their turns from the highest place down. Nothing for a room that keeps no count of them.
    /// </summary>
    [Export]
    public int EnemyPlaces { get; set; }

    /// <summary>
    /// The most updates of play the play time counts, where it stops and stays; nothing for a play time that counts
    /// on without end.
    /// </summary>
    [Export]
    public long MostPlayUpdates { get; set; }

    /// <summary>
    /// Whether remains holding the place numbered <paramref name="place"/>, counted from none, leave the drop they
    /// have rolled, in a room that keeps a count of such places.
    /// </summary>
    public abstract bool RemainsLeaveDrop(int place);

    /// <summary>How many updates the session's clock counts before it comes round to zero; nothing for a clock that never does.</summary>
    [Export]
    public int ClockPeriod { get; set; }

    /// <summary>How many updates the clock's short count counts before it comes round to zero; nothing for one that never does.</summary>
    [Export]
    public int ShortClockPeriod { get; set; }

    /// <summary>
    /// How many updates of an enemy's hurt flash are still to come when the hurt stops holding its behaviour. None:
    /// the hurt lasts as long as the flash.
    /// </summary>
    [Export]
    public int EnemyHurtEndsAtFlashLeft { get; set; }

    /// <summary>
    /// The side, in pixels, of the squares of a room, counted from its top left corner, in which a shot of the
    /// player's and a shot of the enemies' must both stand to meet, whatever their sizes.
    /// </summary>
    [Export]
    public int ShotMeetingSquare { get; set; }

    /// <summary>
    /// How many updates the place of a spent drop goes on being held, by something that shows nothing: a pickup
    /// taken or run out, a drop that came to nothing. What takes a destroyed thing's place arrives as it ends.
    /// </summary>
    [Export]
    public int SpentDropUpdates { get; set; }

    /// <summary>
    /// For how many updates in turn a hurt enemy shows its flash and then its own look, by the updates its flash
    /// has left. None: the flash shows on every update it runs.
    /// </summary>
    [Export]
    public int EnemyFlashUpdatesEachWay { get; set; }

    /// <summary>
    /// How many updates of a freeze are left when a frozen enemy's look stops showing steadily and begins to blink,
    /// and for how many updates in turn it then shows its frozen look and its own. No updates each way: it never
    /// blinks.
    /// </summary>
    [Export]
    public int FrozenLookSteadyUpdates { get; set; }

    /// <inheritdoc cref="FrozenLookSteadyUpdates"/>
    [Export]
    public int FrozenLookBlinkUpdatesEachWay { get; set; }

    /// <summary>
    /// Whether a status an enemy is in still lasts, apart from its time: a game may have something that ends a
    /// status early. Each enemy in a status asks on its turn.
    /// </summary>
    public abstract bool StatusLasts(GameSession session, string status);

    /// <summary>
    /// Makes what keeps how each screen of the room being played lets the view in, for <paramref name="session"/>.
    /// </summary>
    public abstract IScrollCells CreateScrollCells(GameSession session);

    /// <summary>
    /// Makes what decides how the sounds and music asked for come to be heard, which may read the game's data from
    /// <paramref name="content"/>.
    /// </summary>
    public abstract ISoundFlow CreateSoundFlow(ContentLibrary content);

    /// <summary>
    /// Makes the mode that takes the player from the room being played through <paramref name="exit"/> to the room it
    /// leads to, and hands play back there. The engine notices the player in a way out and knows where it leads and
    /// where they arrive; how a game passes between its rooms is the game's own, and the engine has none.
    /// </summary>
    public abstract GameMode CreateRoomTransition(PlayingMode playing, RoomExit exit);

    /// <summary>Makes the session-long services this game adds, one of each.</summary>
    public abstract IReadOnlyList<ISessionService> CreateServices();

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
    /// Makes the sequence a new session's random numbers come from.
    /// </summary>
    public abstract IRandomSequence CreateRandomSequence();

    /// <summary>
    /// Makes the way the player's speed is read against its limits.
    /// </summary>
    public abstract ISpeedReadings CreateSpeedReadings();

    /// <summary>
    /// How the player's body is moved through a room on this update of <paramref name="session"/>, given
    /// <paramref name="plain"/>, the engine's plain moves: those, or the same with the game's own way of trying or
    /// stopping its player's moves.
    /// </summary>
    public abstract Collision.MoveOptions BodyMoves(GameSession session, Collision.MoveOptions plain);
}
