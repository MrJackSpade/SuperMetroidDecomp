using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Owns the four statue tile programs and the descending background
/// pre-instructions. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueSequence
{

    /// <summary>Holds statue busy/released bits, effect spawning, vertical offset, and descent earthquake state.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Supplies equipped items to palette FX created during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Identifies the statue room setup and controls its two logical scroll cells.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Feeds water height and authored earthquake sound into the descending statue effects.</summary>
    private readonly RoomFxOverlayState _roomFxOverlay;
    /// <summary>Creates the palette effect selected by a statue animated-tile instruction.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Tracks boss and statue events, area boss bits, RNG, and the final Tourian unlock event.</summary>
    private readonly SharedGameState _system;
    /// <summary>The open-statues assist skips tile playback and starts the statue at its completed offset.</summary>
    private readonly ActiveAssists _assists;
    /// <summary>Pauses quarter-pixel descent while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;

    /// <summary>Builds one room's statue sequence over the session services that run its tile and the background descent programs.</summary>
    /// <param name="dependencies">The session owners the sequence reads and writes.</param>
    /// <param name="roomFxOverlay">The room's effect overlay FX, whose earthquake sound the descent publishes.</param>
    /// <param name="roomPaletteFx">The room's palette FX.</param>
    /// <param name="roomObjects">The room's PLM system, which receives the Tourian access PLM when the statues are already open.</param>
    /// <param name="enemies">The room's enemies, whose statue actors the sequence drives.</param>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies, RoomFxOverlayState roomFxOverlay,
        RoomPaletteFxSystem roomPaletteFx, RoomObjectSystem roomObjects, RoomEnemySystem enemies)
    {

        _enemies = Ensure.NotNull(enemies);
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomFxOverlay = Ensure.NotNull(roomFxOverlay);
        _roomPaletteFx = Ensure.NotNull(roomPaletteFx);
        _system = dependencies.System;
        _assists = dependencies.Assists;
        _freeze = dependencies.Freeze;
        Begin(Ensure.NotNull(roomObjects));
    }

    /// <summary>One spawned statue animation's definition, program position, and frame timer.</summary>
    internal sealed class TileObject(TourianStatueAnimatedTileProgramDefinition definition)
    {
        /// <summary>Program identity and installed transfer dimensions for this statue.</summary>
        public TourianStatueAnimatedTileProgramDefinition Definition { get; } = definition;
        /// <summary>The statue's typed animated-tile programs, built once for its first-frame duration.</summary>
        public AnimationProgramSet<TourianStatueTileProgram, TourianStatueTilePicture, TourianStatueTileCue> Program { get; } =
            TourianStatueTileAnimations.Create(definition.FirstFrameDuration);
        /// <summary>Where the statue's typed program resumes; null once the program has stopped and the object is deleted.</summary>
        public AnimationProgramCursor<TourianStatueTileProgram>? Cursor { get; private set; } =
            AnimationProgramCursor<TourianStatueTileProgram>.Start(TourianStatueTileProgram.Entry);
        /// <summary>Remaining duration of the current tile frame.</summary>
        public int Timer { get; private set; } = 1;

        /// <summary>Counts a live statue's frame down one tick and reports whether the frame has expired; a stopped statue never counts.</summary>
        internal bool CountDown()
        {
            if (Cursor is null)
                return false;
            return --Timer == 0;
        }

        /// <summary>Resumes the program at <paramref name="cursor"/> after showing a picture for <paramref name="duration"/> ticks.</summary>
        internal void Show(AnimationProgramCursor<TourianStatueTileProgram> cursor, int duration)
        {
            Cursor = cursor;
            Timer = duration;
        }

        /// <summary>Ends the program, deleting the object.</summary>
        internal void Stop() => Cursor = null;
    }
    /// <summary>Kraid, Phantoon, Draygon, then Ridley order matches slot traversal and busy-bit ownership.</summary>
    internal readonly List<TileObject> objects = [];
    /// <summary>Minus two waits for all grey events; nonnegative values count down the authored pre-descent hold.</summary>
    private int delay = -2;
    /// <summary>Accumulated positive 16.16 travel, exposed as a negative background vertical offset.</summary>
    internal int Descent { get; set; }
    /// <summary>Whether the last loaded room uses <c>RunStatueUnlockingAnimations</c>; remains true after the descent finishes; the next room builds its own sequence.</summary>
    public bool Enabled { get; private set; }
    /// <summary>
    /// The whole part of the statues' 16.16 descent, which descends by a quarter pixel per
    /// frame. As a signed 16.16 value its whole word floors toward negative infinity, so
    /// the first quarter-pixel step already reads -1. The descent stops at its authored distance, so the offset stays
    /// between minus that distance and zero.
    /// </summary>
    public int VerticalOffset => CoordinateAxis.AsSigned(SixteenSixteen.WholeOf(-Descent));
    /// <summary>Background offset latched at the same frame boundary as the displayed sprite frame.</summary>
    public int DisplayedVerticalOffset { get; private set; }
    /// <summary>Captures the current background position for presentation with the corresponding sprite frame.</summary>
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>
    /// Restarts the room's statue sequence when the statues newly stand open while Samus is in the room: the statues
    /// return to their starting delay and descent, then begin as a room load would.
    /// </summary>
    /// <param name="roomObjects">The room's PLM system.</param>
    internal void RestartWithStatuesOpen(RoomObjectSystem roomObjects)
    {
        objects.Clear();
        delay = -2;
        Descent = 0;
        DisplayedVerticalOffset = 0;
        Begin(roomObjects);
    }

    /// <summary>Begins the room's statue sequence, enabling the four statue tile programs only for the statue-unlocking room setup.</summary>
    /// <remarks>
    /// A prior unlock event or the open-statues assist starts at the fully descended offset, clears the access obstruction through its PLM, and enables vertical scrolling.
    /// Otherwise the sequence starts raised and constrains scrolling until the authored release and descent finish. Disabled rooms retain no tile objects.
    /// </remarks>
    private void Begin(RoomObjectSystem roomObjects)
    {
        Enabled = _room.ActiveRoom?.State.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        SpawnAnimatedObjects();
        if (_assists.Current.TourianStatuesOpen || _system.HasEvent(StoryEvent.TourianUnlocked))
        {
            Descent = SixteenSixteen.FromWhole(TourianStatueSequenceDefinitions.DescentDistance);
            _enemies.TourianStatue.VerticalOffset = VerticalOffset;
            roomObjects.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, TourianAccessFloor.Clear);
            EnableScrolling();
        }
        else
        {
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 1), RoomScrollState.RedBoundary);
        }
    }

    /// <summary>
    /// Spawns the four statue objects in order. Allocation
    /// and handler traversal both descend through slots, so insertion order also
    /// preserves execution order and which statue can acquire the shared busy bit.
    /// </summary>
    internal void SpawnAnimatedObjects()
    {
        Spawn(AnimatedTileObject.TourianStatueKraid);
        Spawn(AnimatedTileObject.TourianStatuePhantoon);
        Spawn(AnimatedTileObject.TourianStatueDraygon);
        Spawn(AnimatedTileObject.TourianStatueRidley);

        // Resolve each statue's definition and start it at its program entry.
        void Spawn(AnimatedTileObject animatedObject)
        {
            if (!TourianStatueAnimatedTiles.TryResolveObjectHeader(
                    animatedObject, out TourianStatueAnimatedTileProgramDefinition? definition))
                throw new InvalidDataException(
                    $"Tourian statue animated-tile object {animatedObject} is not cataloged.");

            objects.Add(new(definition));
        }
    }

    /// <summary>
    /// Runs AnimtilesHandler in spawn order: each live statue counts its frame down and, when it expires,
    /// advances its typed program to the next shown picture, running the program's cues on the way.
    /// </summary>
    public void StepTiles()
    {
        if (!Enabled) return;
        if (_assists.Current.TourianStatuesOpen) return;
        foreach (TileObject tile in objects)
        {
            if (!tile.CountDown() || tile.Cursor is not { } cursor) continue;
            // The statue programs load no loop counter; the program set still takes one.
            WrappingCountdown loopCounter = WrappingCountdown.Start(0);
            TourianStatueAnimatedTileProgramDefinition definition = tile.Definition;
            AnimationProgramAdvance<TourianStatueTilePicture> advance = tile.Program.AdvanceTo(
                ref cursor, ref loopCounter, (cue, _) => RunCue(definition, cue));
            switch (advance)
            {
                case AnimationProgramAdvance<TourianStatueTilePicture>.Show shown:
                    tile.Show(cursor, shown.Duration);
                    _room.LoadedScope.ArtworkFrames.Select(AnimatedTileFrames.Picture(definition.AnimatedObject, shown.Frame));
                    break;
                case AnimationProgramAdvance<TourianStatueTilePicture>.Stop:
                    tile.Stop();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Tourian statue {definition.AnimatedObject} program ended an advance with unauthored {advance}.");
            }
        }

        // Runs one cue with the statue's operands; a branching cue whose condition holds returns where the program continues.
        AnimationProgramCursor<TourianStatueTileProgram>? RunCue(
            TourianStatueAnimatedTileProgramDefinition definition, TourianStatueTileCue cue)
        {
            ITourianStatueActors statue = _enemies.TourianStatue;
            AnimationProgramCursor<TourianStatueTileProgram> branch =
                AnimationProgramCursor<TourianStatueTileProgram>.Start(cue.Target);
            switch (cue.Kind)
            {
                case TourianStatueTileCueKind.MarkAnimating:
                    statue.Animation.Animating |= definition.Statue;
                    return null;
                case TourianStatueTileCueKind.GoToIfGrey:
                    return _system.HasEvent(definition.GreyEvent) ? branch : null;
                case TourianStatueTileCueKind.GoToIfBossDefeated:
                    return _system.HasAnyBossBits(definition.BossArea, definition.DefeatedBossBits) ? branch : null;
                case TourianStatueTileCueKind.ClearAnimating:
                    statue.Animation.Animating &= ~definition.Statue;
                    return null;
                case TourianStatueTileCueKind.GoToIfReleaseInProgress:
                    return statue.Animation.ReleaseInProgress ? branch : null;
                case TourianStatueTileCueKind.ClaimRelease:
                    statue.Animation.ReleaseInProgress = true;
                    return null;
                case TourianStatueTileCueKind.ClearThreeColors:
                    // The statue shows its eye colours blacked out, then its grey, as the shades the import pre-colours (#1512).
                    _enemies.TourianStatue.ShowStatueLook(definition.ClearedShade);
                    return null;
                case TourianStatueTileCueKind.SpawnEyeGlow:
                    statue.SpawnEyeGlow(definition.Eye);
                    return null;
                case TourianStatueTileCueKind.SpawnSoul:
                    statue.SpawnSoul(definition.Eye);
                    return null;
                case TourianStatueTileCueKind.SpawnGreyPaletteFx:
                    _roomPaletteFx.SpawnDefinition(definition.PaletteFx, _player.IntroducedSamus.EquippedItems);
                    return null;
                case TourianStatueTileCueKind.SetGreyEvent:
                    _system.SetEvent(definition.GreyEvent);
                    return null;
                case TourianStatueTileCueKind.FinishRelease:
                    statue.Animation.ReleaseInProgress = false;
                    statue.Animation.Animating &= ~definition.Statue;
                    return null;
                case TourianStatueTileCueKind.ApplyGreyColors:
                    _enemies.TourianStatue.ShowStatueLook(definition.GreyShade);
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TourianStatueTileCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>Including the authored delay and quarter-pixel descent.</summary>
    public void StepDescent(RoomObjectSystem roomObjects)
    {
        if (!Enabled) return;
        _enemies.TourianStatue.WaterSurface = new(0, CoordinateAxis.Wrap(_roomFxOverlay.Surface
            ?? throw new InvalidOperationException("The Tourian statues' room effect names no water surface.")));
        if (_assists.Current.TourianStatuesOpen || _system.HasEvent(StoryEvent.TourianUnlocked)) { EnableScrolling(); return; }
        if (delay == -2)
        {
            if (!_system.HasEvent(StoryEvent.PhantoonStatueGrey) || !_system.HasEvent(StoryEvent.RidleyStatueGrey)
                || !_system.HasEvent(StoryEvent.DraygonStatueGrey) || !_system.HasEvent(StoryEvent.KraidStatueGrey)) return;
            _enemies.TourianStatue.Animation.AllReleased = true;
            if (_enemies.TourianStatue.Animation.ReleaseInProgress) return;
            delay = TourianStatueSequenceDefinitions.DescentDelay;
            return;
        }
        _enemies.Earthquake.EarthquakeType = TourianStatueSequenceDefinitions.DescentEarthquakeType;
        _enemies.Earthquake.AddTimerBits(TourianStatueSequenceDefinitions.DescentEarthquakeTimer);
        _roomFxOverlay.PublishStatueEarthquakeSound(
            _system.RandomNumber, _system.EarthquakeSoundDelay);
        if (delay >= 0)
        {
            if (--delay < 0) _enemies.TourianStatue.SpawnDescentDust();
            return;
        }
        if (_freeze.TimeIsFrozen) return;
        Descent += TourianStatueSequenceDefinitions.DescentStep;
        _enemies.TourianStatue.VerticalOffset = VerticalOffset;
        // The statue's descent compares the whole position against its end value, which it first reaches
        // three quarter-steps before a full 240 pixels.
        if (VerticalOffset == -TourianStatueSequenceDefinitions.DescentDistance)
        {
            roomObjects.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, TourianAccessFloor.Crumble);
            _system.SetEvent(StoryEvent.TourianUnlocked);
        }
    }

    /// <summary>Marks the entrance statue finished and makes both access-room scroll cells freely traversable.</summary>
    private void EnableScrolling()
    {
        _enemies.TourianStatue.Finished = true;
        _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Green);
        _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 1), RoomScrollState.Green);
    }
}
