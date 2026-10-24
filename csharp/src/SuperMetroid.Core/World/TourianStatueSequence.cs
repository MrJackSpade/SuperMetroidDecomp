using SuperMetroid.Core.Assets;
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
/// Owns the four bank-$87 statue tile programs and the bank-$88 descending BG2
/// pre-instructions. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueSequence
{

    /// <summary>Receives the black clear and installed grey-color palette writes from tile instructions.</summary>
    private readonly GameplayColours _cgram;
    /// <summary>Holds statue busy/released bits, effect spawning, vertical offset, and descent earthquake state.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Supplies equipped items to palette FX created during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Identifies the statue room setup and controls its two logical scroll cells.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Feeds water height and authored earthquake sound into the descending statue effects.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Creates the palette effect selected by a statue animated-tile instruction.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Tracks boss and statue events, area boss bits, RNG, and the final Tourian unlock event.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>The unlock option skips tile playback and starts the statue at its completed offset.</summary>
    private readonly TesterTools _tester;
    /// <summary>Pauses quarter-pixel descent while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;

    /// <summary>Builds one room's statue sequence over the session services that run its tile and BG2 descent programs.</summary>
    /// <param name="dependencies">The session owners the sequence reads and writes.</param>
    /// <param name="roomLayer3Fx">The room's layer-3 FX, whose earthquake sound the descent publishes.</param>
    /// <param name="roomPaletteFx">The room's palette FX.</param>
    /// <param name="plms">The room's PLM system, which receives the Tourian access PLM when the statues are already open.</param>
    /// <param name="enemies">The room's enemies, whose statue actors the sequence drives.</param>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies, RoomLayer3FxState roomLayer3Fx,
        RoomPaletteFxSystem roomPaletteFx, RoomPlmSystem plms, RoomEnemySystem enemies)
    {

        _cgram = dependencies.Cgram;
        _enemies = Ensure.NotNull(enemies);
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomLayer3Fx = Ensure.NotNull(roomLayer3Fx);
        _roomPaletteFx = Ensure.NotNull(roomPaletteFx);
        _system = dependencies.System;
        _tester = dependencies.Tester;
        _freeze = dependencies.Freeze;
        Begin(Ensure.NotNull(plms));
    }

    /// <summary>One spawned statue animation's definition, program position, and frame timer.</summary>
    internal sealed class TileObject(TourianStatueAnimatedTileProgramDefinition definition)
    {
        /// <summary>Program identity and installed transfer dimensions for this statue.</summary>
        public TourianStatueAnimatedTileProgramDefinition Definition { get; } = definition;
        /// <summary>Where the statue's typed program resumes; null once the program has stopped and the object is deleted.</summary>
        public AnimationProgramCursor<TourianStatueTileProgram>? Cursor { get; private set; } =
            AnimationProgramCursor<TourianStatueTileProgram>.Start(TourianStatueTileProgram.Entry);
        /// <summary>Remaining duration of the current tile frame.</summary>
        public WrappingCountdown Timer { get; private set; } = WrappingCountdown.Start(1);

        /// <summary>Counts a live statue's frame down one tick and reports whether the frame has expired; a stopped statue never counts.</summary>
        internal bool CountDown()
        {
            if (Cursor is null)
                return false;
            Timer = Timer.Decrement().Remaining;
            return Timer.IsZero;
        }

        /// <summary>Resumes the program at <paramref name="cursor"/> after showing a picture for <paramref name="duration"/> ticks.</summary>
        internal void Show(AnimationProgramCursor<TourianStatueTileProgram> cursor, int duration)
        {
            Cursor = cursor;
            Timer = WrappingCountdown.Start(duration);
        }

        /// <summary>Ends the program, deleting the object.</summary>
        internal void Stop() => Cursor = null;
    }
    /// <summary>Kraid, Phantoon, Draygon, then Ridley order matches native slot traversal and busy-bit ownership.</summary>
    internal readonly List<TileObject> objects = [];
    /// <summary>Minus two waits for all grey events; nonnegative values count down the authored pre-descent hold.</summary>
    private int delay = -2;
    /// <summary>Accumulated positive 16.16 travel, exposed as a negative BG2 vertical offset.</summary>
    internal int Descent { get; set; }
    /// <summary>Whether the last loaded room uses <c>RunStatueUnlockingAnimations</c>; remains true after the descent finishes; the next room builds its own sequence.</summary>
    public bool Enabled { get; private set; }
    /// <summary>
    /// The whole word of native HDMAObject_Var1:Var0, which descends by $FFFF:C000 per
    /// frame. As a signed 16.16 value its whole word floors toward negative infinity, so
    /// the first quarter-pixel step already reads -1.
    /// </summary>
    public short VerticalOffset => unchecked((short)SnesSixteenSixteen.WholeOf(-Descent));
    /// <summary>BG2 offset accepted with the same NMI as the displayed OBJ buffer.</summary>
    public short DisplayedVerticalOffset { get; private set; }
    /// <summary>Captures the current BG2 position for presentation with the corresponding NMI OBJ buffer.</summary>
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>
    /// Restarts the room's statue sequence when the host tester opens Tourian in the room: the statues return to their
    /// starting delay and descent, then begin as a room load would.
    /// </summary>
    /// <param name="plms">The room's PLM system.</param>
    internal void RestartForTesterUnlock(RoomPlmSystem plms)
    {
        objects.Clear();
        delay = -2;
        Descent = 0;
        DisplayedVerticalOffset = 0;
        Begin(plms);
    }

    /// <summary>Begins the room's statue sequence, enabling the four statue tile programs only for the statue-unlocking room setup.</summary>
    /// <remarks>
    /// A prior unlock event or host unlock option starts at the fully descended offset, clears the access obstruction through its PLM, and enables vertical scrolling.
    /// Otherwise the sequence starts raised and constrains scrolling until the authored release and descent finish. Disabled rooms retain no tile objects.
    /// </remarks>
    private void Begin(RoomPlmSystem plms)
    {
        Enabled = _room.ActiveRoom?.State.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        SpawnAnimatedObjects();
        if (_tester.UnlockTourianEnabled || _system.HasEvent(EventNumber.TourianUnlocked))
        {
            Descent = SnesSixteenSixteen.FromWhole(TourianStatueRomData.DescentDistance);
            _enemies.TourianStatue.VerticalOffset = VerticalOffset;
            plms.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, TourianAccessFloor.Clear);
            EnableScrolling();
        }
        else
        {
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 1), RoomScrollState.RedBoundary);
        }
    }

    /// <summary>
    /// Translates the four LDY/JSL calls at $8F:91D7..91F2 directly. Native allocation
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
            if (!TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(
                    animatedObject, out TourianStatueAnimatedTileProgramDefinition? definition))
                throw new InvalidDataException(
                    $"Tourian statue animated-tile object {animatedObject} is not cataloged.");

            objects.Add(new(definition));
        }
    }

    /// <summary>
    /// Runs AnimtilesHandler in native spawn order: each live statue counts its frame down and, when it expires,
    /// advances its typed program to the next shown picture, running the program's cues on the way.
    /// </summary>
    [PpuLayoutBoundary("codec: animated-tile instruction CGRAM index operand")]
    public void StepTiles()
    {
        if (!Enabled) return;
        if (_tester.UnlockTourianEnabled) return;
        foreach (TileObject tile in objects)
        {
            if (!tile.CountDown() || tile.Cursor is not { } cursor) continue;
            // The statue programs load no loop counter; the program set still takes one.
            WrappingCountdown loopCounter = WrappingCountdown.Start(0);
            TourianStatueAnimatedTileProgramDefinition definition = tile.Definition;
            AnimationProgramAdvance<TourianStatueTilePicture> advance = definition.Program.AdvanceTo(
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
                    statue.AnimationState |= definition.AnimatingFlag;
                    return null;
                case TourianStatueTileCueKind.GoToIfGrey:
                    return _system.HasEvent(definition.GreyEvent) ? branch : null;
                case TourianStatueTileCueKind.GoToIfBossDefeated:
                    return _system.HasAnyBossBits(definition.BossArea, definition.DefeatedBossBits) ? branch : null;
                case TourianStatueTileCueKind.ClearAnimating:
                    statue.AnimationState &= ~definition.AnimatingFlag;
                    return null;
                case TourianStatueTileCueKind.GoToIfReleaseInProgress:
                    return (statue.AnimationState & TourianStatueAnimationState.ReleaseInProgress) != 0 ? branch : null;
                case TourianStatueTileCueKind.ClaimRelease:
                    statue.AnimationState |= TourianStatueAnimationState.ReleaseInProgress;
                    return null;
                case TourianStatueTileCueKind.ClearThreeColors:
                    definition.ClearedColours.In(_cgram).Fill(Bgr555.Black);
                    return null;
                case TourianStatueTileCueKind.SpawnEyeGlow:
                    statue.SpawnEyeGlow(definition.UnlockEffectParameter);
                    return null;
                case TourianStatueTileCueKind.SpawnSoul:
                    statue.SpawnSoul(definition.UnlockEffectParameter);
                    return null;
                case TourianStatueTileCueKind.SpawnGreyPaletteFx:
                    _roomPaletteFx.SpawnDefinition(definition.PaletteFx, _player.IntroducedSamus.EquippedItems);
                    return null;
                case TourianStatueTileCueKind.SetGreyEvent:
                    _system.SetEvent(definition.GreyEvent);
                    return null;
                case TourianStatueTileCueKind.FinishRelease:
                    statue.AnimationState &= ~(TourianStatueAnimationState.ReleaseInProgress | definition.AnimatingFlag);
                    return null;
                case TourianStatueTileCueKind.ApplyGreyColors:
                    (_enemies.AreaArtwork?.TourianStatueColors ?? throw new InvalidOperationException(
                        "Tourian statue animation requires installed grey colors."))
                        .ApplyGrey(definition.GreyColours.In(_cgram));
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TourianStatueTileCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>$88:DBD7/DC23/DC69/DCBA, including the authored delay and quarter-pixel descent.</summary>
    public void StepDescent(RoomPlmSystem plms)
    {
        if (!Enabled) return;
        _enemies.TourianStatue.WaterSurface = new(0, _roomLayer3Fx.Surface.Y);
        if (_tester.UnlockTourianEnabled || _system.HasEvent(EventNumber.TourianUnlocked)) { EnableScrolling(); return; }
        if (delay == -2)
        {
            if (!_system.HasEvent(EventNumber.PhantoonStatueGrey) || !_system.HasEvent(EventNumber.RidleyStatueGrey)
                || !_system.HasEvent(EventNumber.DraygonStatueGrey) || !_system.HasEvent(EventNumber.KraidStatueGrey)) return;
            _enemies.TourianStatue.AnimationState |= TourianStatueAnimationState.AllReleased;
            if ((_enemies.TourianStatue.AnimationState & TourianStatueAnimationState.ReleaseInProgress) != 0) return;
            delay = TourianStatueRomData.DescentDelay;
            return;
        }
        _enemies.Earthquake.EarthquakeType = TourianStatueRomData.DescentEarthquakeType;
        _enemies.Earthquake.AddTimerBits(TourianStatueRomData.DescentEarthquakeTimer);
        _roomLayer3Fx.PublishStatueEarthquakeSound(
            _system.RandomNumber, _system.MainGameLoopCarry);
        if (delay >= 0)
        {
            if (--delay < 0) _enemies.TourianStatue.SpawnDescentDust();
            return;
        }
        if (_freeze.TimeIsFrozen) return;
        Descent += TourianStatueRomData.DescentStep;
        _enemies.TourianStatue.VerticalOffset = VerticalOffset;
        // $88:DC90 compares the whole word against $FF10, which it first reaches
        // three quarter-steps before a full 240 pixels.
        if (VerticalOffset == -TourianStatueRomData.DescentDistance)
        {
            plms.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, TourianAccessFloor.Crumble);
            _system.SetEvent(EventNumber.TourianUnlocked);
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
