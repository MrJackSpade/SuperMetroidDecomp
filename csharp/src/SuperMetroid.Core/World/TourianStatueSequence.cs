using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Owns the four statue animations and the descending background's
/// per-frame steps. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueSequence
{

    /// <summary>Holds statue busy/released bits, effect spawning, vertical offset, and descent earthquake state.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Supplies equipped items to colour effects started during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Identifies the statue room setup and controls its two logical scroll cells.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Feeds water height and authored earthquake sound into the descending statue effects.</summary>
    private readonly RoomFxOverlayState _roomFxOverlay;
    /// <summary>Creates the palette effect selected by a statue animation cue.</summary>
    private readonly ColourEffectSystem _roomColourEffects;
    /// <summary>Tracks boss and statue events, the area's defeated bosses, RNG, and the final Tourian unlock event.</summary>
    private readonly SharedGameState _system;
    /// <summary>The open-statues assist skips the statue animation and starts the statue at its completed offset.</summary>
    private readonly ActiveAssists _assists;
    /// <summary>Pauses quarter-pixel descent while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;

    /// <summary>Builds one room's statue sequence over the session services that run its statue and background descent animations.</summary>
    /// <param name="dependencies">The session owners the sequence reads and writes.</param>
    /// <param name="roomFxOverlay">The room's effect overlay FX, whose earthquake sound the descent publishes.</param>
    /// <param name="roomColourEffects">The room's colour effects.</param>
    /// <param name="roomObjects">The room's room-object system, which receives the Tourian access room object when the statues are already open.</param>
    /// <param name="enemies">The room's enemies, whose statue actors the sequence drives.</param>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies, RoomFxOverlayState roomFxOverlay,
        ColourEffectSystem roomColourEffects, RoomObjectSystem roomObjects, RoomEnemySystem enemies)
    {

        _enemies = Ensure.NotNull(enemies);
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomFxOverlay = Ensure.NotNull(roomFxOverlay);
        _roomColourEffects = Ensure.NotNull(roomColourEffects);
        _system = dependencies.System;
        _assists = dependencies.Assists;
        _freeze = dependencies.Freeze;
        Begin(Ensure.NotNull(roomObjects));
    }

    /// <summary>One spawned statue animation's definition, animation playback, and frame timer.</summary>
    internal sealed class TileObject(TourianStatueAnimationDefinition definition)
    {
        /// <summary>The statue's animation and the values its events act on.</summary>
        public TourianStatueAnimationDefinition Definition { get; } = definition;
        /// <summary>The statue's typed animation segments, built once for its first-frame duration.</summary>
        public AnimationSegments<TourianStatueTileSegment, TourianStatueTilePicture, TourianStatueTileCue> Segments { get; } =
            TourianStatueTileAnimations.SegmentsFor(definition.FirstFrameDuration);
        /// <summary>The statue's animation and where it stands in it; null once the animation has ended and the object is deleted.</summary>
        public AnimationPlayback<TourianStatueTileSegment>? Playback { get; private set; } =
            new AnimationPlayback<TourianStatueTileSegment>(TourianStatueTileAnimations.StartingWith(TourianStatueTileSegment.Entry), AnimationPlace.StartOfAnimation);
        /// <summary>Remaining duration of the current statue frame.</summary>
        public int Timer { get; private set; } = 1;

        /// <summary>Counts a live statue's frame down one tick and reports whether the frame has expired; an ended statue never counts.</summary>
        internal bool CountDown()
        {
            if (Playback is null)
                return false;
            return --Timer == 0;
        }

        /// <summary>Resumes the animation at <paramref name="playback"/> after showing a picture for <paramref name="duration"/> ticks.</summary>
        internal void Show(AnimationPlayback<TourianStatueTileSegment> playback, int duration)
        {
            Playback = playback;
            Timer = duration;
        }

        /// <summary>Ends the animation, deleting the object.</summary>
        internal void Stop() => Playback = null;
    }
    /// <summary>Kraid, Phantoon, Draygon, then Ridley order matches slot traversal and busy-bit ownership.</summary>
    internal readonly List<TileObject> objects = [];
    /// <summary>Minus two waits for all grey events; nonnegative values count down the authored pre-descent hold.</summary>
    private int delay = -2;
    /// <summary>
    /// How far the statues have lowered, zero through the authored distance and the few quarter-steps that first reach it,
    /// exposed as a negative background vertical offset.
    /// </summary>
    internal SignedSixteenSixteen Descent { get; set; }
    /// <summary>Whether the last loaded room uses <c>RunStatueUnlockingAnimations</c>; remains true after the descent finishes; the next room builds its own sequence.</summary>
    public bool Enabled { get; private set; }
    /// <summary>
    /// The whole pixels of the statues' descent upward, which descends by a quarter pixel per frame. The upward distance's
    /// whole pixels round toward negative infinity, so the first quarter-pixel step already reads -1. The descent stops at
    /// its authored distance, so the offset stays between minus that distance and zero.
    /// </summary>
    public int VerticalOffset => (-Descent).WholePixels;
    /// <summary>Background offset latched at the same frame boundary as the displayed sprite frame.</summary>
    public int DisplayedVerticalOffset { get; private set; }
    /// <summary>Captures the current background position for presentation with the corresponding sprite frame.</summary>
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>
    /// Restarts the room's statue sequence when the statues newly stand open while Samus is in the room: the statues
    /// return to their starting delay and descent, then begin as a room load would.
    /// </summary>
    /// <param name="roomObjects">The room's room-object system.</param>
    internal void RestartWithStatuesOpen(RoomObjectSystem roomObjects)
    {
        objects.Clear();
        delay = -2;
        Descent = SignedSixteenSixteen.Zero;
        DisplayedVerticalOffset = 0;
        Begin(roomObjects);
    }

    /// <summary>Begins the room's statue sequence, enabling the four statue animations only for the statue-unlocking room setup.</summary>
    /// <remarks>
    /// A prior unlock event or the open-statues assist starts at the fully descended offset, clears the access obstruction through its room object, and enables vertical scrolling.
    /// Otherwise the sequence starts raised and constrains scrolling until the authored release and descent finish. Disabled rooms retain no statue animations.
    /// </remarks>
    private void Begin(RoomObjectSystem roomObjects)
    {
        Enabled = _room.ActiveRoom?.State.SetupBehavior == RoomSetupBehavior.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        SpawnAnimatedObjects();
        if (_assists.Current.TourianStatuesOpen || _system.HasEvent(StoryEvent.TourianUnlocked))
        {
            Descent = SignedSixteenSixteen.FromWholePixels(TourianStatueSequenceDefinitions.DescentDistance);
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
        Spawn(TourianGreyStatues.Kraid);
        Spawn(TourianGreyStatues.Phantoon);
        Spawn(TourianGreyStatues.Draygon);
        Spawn(TourianGreyStatues.Ridley);

        // Resolve each statue's definition and start it at its animation entry.
        void Spawn(TourianGreyStatues statue)
        {
            if (!TourianStatueAnimationDefinitions.Statues.TryGetValue(statue, out TourianStatueAnimationDefinition? definition))
                throw new InvalidDataException($"Tourian statue {statue} is not cataloged.");

            objects.Add(new(definition));
        }
    }

    /// <summary>
    /// Runs the statue animations in spawn order: each live statue counts its frame down and, when it expires,
    /// plays its typed animation on to the next shown picture, carrying out the animation's events on the way.
    /// </summary>
    public void StepTiles()
    {
        if (!Enabled) return;
        if (_assists.Current.TourianStatuesOpen) return;
        foreach (TileObject tile in objects)
        {
            if (!tile.CountDown() || tile.Playback is not { } playback) continue;
            // The statue animations play no counted phase; the play still takes a repeat counter.
            WrappingCountdown repeats = WrappingCountdown.Start(0);
            TourianStatueAnimationDefinition definition = tile.Definition;
            Animation<TourianStatueTileSegment> animation = playback.Animation;
            AnimationPlace place = playback.Place;
            AnimationShown<TourianStatueTileSegment, TourianStatueTilePicture> shown = tile.Segments.PlayToNextFrame(
                ref animation, ref place, ref repeats, cue => RunCue(definition, cue));
            switch (shown)
            {
                case AnimationShown<TourianStatueTileSegment, TourianStatueTilePicture>.Frame frame:
                    tile.Show(new AnimationPlayback<TourianStatueTileSegment>(animation, place), frame.Duration);
                    _room.LoadedScope.ArtworkFrames.Select(definition.Picture(frame.Shown));
                    break;
                case AnimationShown<TourianStatueTileSegment, TourianStatueTilePicture>.Ended:
                    tile.Stop();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Tourian statue {definition.Statue} animation ended a play with unauthored {shown}.");
            }
        }

        // Carries out one event with the statue's values; a branching event whose condition holds returns where the play continues.
        AnimationContinuation<TourianStatueTileSegment>? RunCue(
            TourianStatueAnimationDefinition definition, TourianStatueTileCue cue)
        {
            ITourianStatueActors statue = _enemies.TourianStatue;
            var branch = new AnimationContinuation<TourianStatueTileSegment>(
                TourianStatueTileAnimations.StartingWith(cue.Target), AnimationPlace.StartOfAnimation);
            switch (cue.Kind)
            {
                case TourianStatueTileCueKind.MarkAnimating:
                    statue.Animation.Animating |= definition.Statue;
                    return null;
                case TourianStatueTileCueKind.GoToIfGrey:
                    return _system.HasEvent(definition.GreyEvent) ? branch : null;
                case TourianStatueTileCueKind.GoToIfBossDefeated:
                    return _system.HasDefeatedAnyOf(definition.BossArea, definition.DefeatedBosses) ? branch : null;
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
                case TourianStatueTileCueKind.SpawnGreyColourEffect:
                    _roomColourEffects.Start(definition.ColourEffect, _player.IntroducedSamus.EquippedItems);
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
        _enemies.Earthquake.Sustain(TourianStatueSequenceDefinitions.DescentEarthquake);
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
