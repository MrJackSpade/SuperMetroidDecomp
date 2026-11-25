using Engine.Core;
using Engine.Shared;
using Game.Cinematics;
using Game.Compatibility;
using Godot;

namespace Game;

/// <summary>
/// Super Metroid, as the engine sees it: the one thing the engine is pointed at. It names this game's actions and their
/// buttons; the player's scene and everything else that is a setting is in its resource, <c>game_definition.tres</c>.
/// </summary>
[GlobalClass]
public partial class SuperMetroid : GameDefinition
{
    /// <summary>The definition, with this game's actions.</summary>
    public SuperMetroid() => Actions = [.. Data.Actions.All.Select(action => action.Name)];

    /// <inheritdoc/>
    public override Dictionary<string, List<string>> StandardBindings => Data.Actions.StandardBindings;

    /// <summary>The game starts on its title screen.</summary>
    public override GameMode CreateFirstMode() => FirstMode is null ? new Menus.TitleMode() : base.CreateFirstMode();

    /// <summary>Pausing brings up the map and equipment screens.</summary>
    public override GameMode CreatePauseMode(PlayingMode playing) => new PauseScreen(playing);

    /// <summary>The pause is not asked for on Ceres, nor while a power bomb's blast is under way, as on the cartridge.</summary>
    public override bool PauseMayBeAsked(GameSession session, Engine.World.Room room) =>
        room.Area != Data.Areas.Ceres && !Compatibility.CartridgeSounds.Of(session).PowerBombExploding;

    /// <summary>A new game begins with the opening.</summary>
    public override GameMode CreateModeForNewGame() => new Intro();

    /// <summary>Samus's death is played by its own mode, and the game over screen comes after it.</summary>
    public override GameMode CreateDeathMode(PlayingMode playing) => new DeathMode(playing);

    /// <summary>
    /// This game adds the escape countdown, the random number the cartridge drew every frame, which of the title's
    /// demos is due, the elevator's journey, what was held on the pause screen from one pause to the next, and the
    /// count of steps the cartridge's gradual fades share.
    /// </summary>
    public override IReadOnlyList<ISessionService> CreateServices() =>
    [
        new Escape(),
        new Compatibility.FrameRandom(_random ?? throw new InvalidOperationException("The session's services were asked for before its random numbers.")),
        new Menus.DemoSchedule(),
        new ElevatorJourney(),
        new PauseHeldButtons(),
        new Compatibility.SharedFadeCount(),
    ];

    /// <summary>The sequence of random numbers made for the session, which the service that draws each update's number also stirs.</summary>
    private Compatibility.CartridgeRandom? _random;

    /// <summary>This game's outcomes follow the cartridge's own sequence of random numbers.</summary>
    public override IRandomSequence CreateRandomSequence() => _random = new Compatibility.CartridgeRandom();

    /// <summary>An enemy stays frozen only while the ice beam is switched on: switched off, it thaws on its next turn.</summary>
    public override bool StatusLasts(GameSession session, string status) =>
        status != Engine.Actors.Enemy.Frozen || session.Player.Inventory.IsOn(Data.Equipment.IceBeam);

    /// <summary>Sounds and music wait their turn as the cartridge's do.</summary>
    public override ISoundFlow CreateSoundFlow(Engine.Content.ContentLibrary content) => new CartridgeSounds(content);

    /// <summary>A door that scrolls is passed through as the cartridge passes Samus through it; any other way out is the engine's plain change of rooms.</summary>
    public override GameMode CreateRoomTransition(PlayingMode playing, Engine.World.RoomExit exit) =>
        exit is Door door ? new DoorTransitionMode(playing, door) : base.CreateRoomTransition(playing, exit);

    /// <summary>The room's scroll cells are the cartridge's fifty, which outlive each room.</summary>
    public override Engine.World.IScrollCells CreateScrollCells(GameSession session) => new Compatibility.KeptScrollCells(session);

    /// <summary>The cartridge's death explosion in the first of the enemies' places never becomes a pickup, though its drop is rolled.</summary>
    public override bool RemainsLeaveDrop(int place) => place != 0;

    /// <summary>Samus's speed is read against its limits as the cartridge reads it.</summary>
    public override Engine.Actors.ISpeedReadings CreateSpeedReadings() => new Compatibility.CartridgeSpeedReadings();

    /// <summary>
    /// Samus's moves are tried as the cartridge tries them: only where each would end, so that she keeps her part of
    /// a pixel beside what she has not yet run into, rests at the far end of her pixel on the ground and against a
    /// wall to her right, passes what a fast move steps over, and is put on a slope's surface under her middle. The
    /// blocks under a move up or down are tried from her left on the clock's even updates and from her right on its
    /// odd ones, as the cartridge's scans alternate.
    /// </summary>
    public override Engine.Collision.MoveOptions BodyMoves(GameSession session, Engine.Collision.MoveOptions plain) =>
        plain with { Tries = Engine.Collision.WayTried.WhereItEnds, TriesFromRight = session.Clock.Updates % 2 != 0 };
}
