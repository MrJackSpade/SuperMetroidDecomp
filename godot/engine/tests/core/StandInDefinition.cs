using Engine.Core;

namespace Engine.Tests.Core;

/// <summary>The engine's stand-in game for its own checks: it starts in a <see cref="StandInMode"/>.</summary>
public partial class StandInDefinition : GameDefinition
{
    /// <summary>How many updates the first mode runs before handing over.</summary>
    public int UpdatesBeforeHandOver { get; set; } = 3;

    /// <inheritdoc/>
    public override GameMode CreateFirstMode() => new StandInMode { Name = "First", UpdatesBeforeHandOver = UpdatesBeforeHandOver };

    /// <inheritdoc/>
    public override Engine.Actors.Player CreatePlayer() => new StandInPlayer { Name = "Player" };

    /// <summary>The number the stand-in game's random sequence starts from.</summary>
    public uint RandomSeed { get; set; } = 1;

    /// <inheritdoc/>
    public override GameMode CreatePauseMode(PlayingMode playing) => new PausedMode(playing);

    /// <inheritdoc/>
    public override Engine.World.IScrollCells CreateScrollCells(GameSession session) => new RoomScrollCells();

    /// <inheritdoc/>
    public override ISoundFlow CreateSoundFlow(Engine.Content.ContentLibrary content) => new ImmediateSoundFlow();

    /// <inheritdoc/>
    public override Engine.Shared.IRandomSequence CreateRandomSequence() => new PlainRandomSequence(RandomSeed);

    /// <inheritdoc/>
    public override Engine.Actors.ISpeedReadings CreateSpeedReadings() => new PlainSpeedReadings();

    /// <inheritdoc/>
    public override Dictionary<string, List<string>> StandardBindings => [];

    /// <inheritdoc/>
    public override bool PauseMayBeAsked(GameSession session, Engine.World.Room room) => true;

    /// <inheritdoc/>
    public override GameMode CreateModeForNewGame() => new PlayingMode();

    /// <inheritdoc/>
    public override GameMode CreateDeathMode(PlayingMode playing) => new LoadingMode();

    /// <inheritdoc/>
    public override bool RemainsLeaveDrop(int place) => true;

    /// <inheritdoc/>
    public override bool StatusLasts(GameSession session, string status) => true;

    /// <inheritdoc/>
    public override IReadOnlyList<ISessionService> CreateServices() => [];

    /// <inheritdoc/>
    public override Engine.Collision.MoveOptions BodyMoves(GameSession session, Engine.Collision.MoveOptions plain) => plain;

    /// <summary>The stand-in game has no passage between its rooms: the engine carries none, and its checks try none.</summary>
    public override GameMode CreateRoomTransition(PlayingMode playing, Engine.World.RoomExit exit) =>
        throw new InvalidOperationException("The stand-in game has no way from one room to the next.");
}
