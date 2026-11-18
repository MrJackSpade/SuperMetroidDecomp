using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.World;

/// <summary>
/// Owns the four statue animations and the descending background's
/// per-frame steps. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueSequence
{

    /// <summary>
    /// The room's enemies: the statues' shared animation state (which statues animate, which one is releasing, whether all
    /// have released), the unlock's effects and the room's earthquake the descent shakes.
    /// </summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>
    /// The statue room the sequence lowers the statue in (#2382), which its statues' decorations follow down and its unlock
    /// effects splash in.
    /// </summary>
    internal TourianStatueRoom Room { get; }
    /// <summary>The entrance statue and the unlock effects in the statue room, which the statue animations drive.</summary>
    private readonly ITourianStatueActors _statue;
    /// <summary>Supplies equipped items to colour effects started during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Identifies the statue room setup and controls its two logical scroll cells.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Publishes the earthquake sound of the statues' descent.</summary>
    private readonly RoomFxOverlayState _roomFxOverlay;
    /// <summary>Creates the colour effect selected by a statue animation cue.</summary>
    private readonly ColourEffectSystem _roomColourEffects;
    /// <summary>Tracks boss and statue events, the area's defeated bosses, RNG, and the final Tourian unlock event.</summary>
    private readonly SharedGameState _system;
    /// <summary>The open-statues assist skips the statue animation and starts the statue at its completed offset.</summary>
    private readonly ActiveAssists _assists;
    /// <summary>Pauses quarter-pixel descent while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;

    /// <summary>Builds one room's statue sequence over the session services that run its statue and background descent animations.</summary>
    /// <param name="dependencies">The session owners the sequence reads and writes.</param>
    /// <param name="statueRoom">The statue room, whose statue the sequence lowers.</param>
    /// <param name="roomFxOverlay">The room's effect overlay FX, whose earthquake sound the descent publishes.</param>
    /// <param name="roomColourEffects">The room's colour effects.</param>
    /// <param name="roomObjects">The room's room-object system, which receives the Tourian access room object when the statues are already open.</param>
    /// <param name="enemies">The room's enemies, whose statue actors the sequence drives.</param>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies, TourianStatueRoom statueRoom,
        RoomFxOverlayState roomFxOverlay, ColourEffectSystem roomColourEffects, RoomObjectSystem roomObjects, RoomEnemySystem enemies)
    {

        _enemies = Ensure.NotNull(enemies);
        Room = Ensure.NotNull(statueRoom);
        _statue = enemies.TourianStatueIn(statueRoom);
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomFxOverlay = Ensure.NotNull(roomFxOverlay);
        _roomColourEffects = Ensure.NotNull(roomColourEffects);
        _system = dependencies.System;
        _assists = dependencies.Assists;
        _freeze = dependencies.Freeze;
        Begin(Ensure.NotNull(roomObjects));
    }

    /// <summary>One spawned statue's definition and its animation, played on the shared animation player (#2487).</summary>
    internal sealed class StatueBlockAnimation
    {
        /// <summary>
        /// Starts the statue of <paramref name="definition"/> at its entry animation, showing nothing until its first update,
        /// which plays the entry's events and shows its first picture.
        /// </summary>
        public StatueBlockAnimation(TourianStatueAnimationDefinition definition)
        {
            Definition = definition;
            Animations = new TourianStatueBlockAnimations(definition.FirstFrameDuration);
            Player = AnimationPlayer<TourianStatueBlockSegment, TourianStatueBlockPicture, TourianStatueBlockCue>.Starting(
                Animations.Segments, Animations.StartingWith(TourianStatueBlockSegment.Entry));
        }

        /// <summary>The statue's animation and the values its events act on.</summary>
        public TourianStatueAnimationDefinition Definition { get; }
        /// <summary>The statue's typed animations, built once for its first-frame duration.</summary>
        public TourianStatueBlockAnimations Animations { get; }
        /// <summary>
        /// The statue's animation playing: each picture for its updates, then on to the next; once a release or the
        /// already-grey finish ends it, it has stopped and counts no more.
        /// </summary>
        internal AnimationPlayer<TourianStatueBlockSegment, TourianStatueBlockPicture, TourianStatueBlockCue> Player { get; }
    }
    /// <summary>
    /// The four statue animations in Kraid, Phantoon, Draygon, Ridley order, the order they run in each update and so the
    /// order in which a waiting statue can claim the one release that plays at a time.
    /// </summary>
    private readonly List<StatueBlockAnimation> _statues = [];
    /// <summary>Whether the statues await their releases, hold before lowering, or lower.</summary>
    private TourianStatueDescentPhase _descentPhase = TourianStatueDescentPhase.AwaitingReleases;
    /// <summary>
    /// Updates the pre-descent hold has left while <see cref="_descentPhase"/> is <see cref="TourianStatueDescentPhase.Holding"/>;
    /// the update that leaves it none raises the dust. The hold shows no picture, so it is the descent's own count of the
    /// hold's updates, not an animation.
    /// </summary>
    private int _holdUpdatesLeft;
    /// <summary>The statue room's statue offset: minus the whole pixels of its descent (<see cref="TourianStatueRoom.StatueOffset"/>).</summary>
    public int VerticalOffset => Room.StatueOffset;
    /// <summary>Background offset latched at the same frame boundary as the displayed sprite frame.</summary>
    public int DisplayedVerticalOffset { get; private set; }
    /// <summary>Captures the current background position for presentation with the corresponding sprite frame.</summary>
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>
    /// Restarts the room's statue sequence when the statues newly stand open while Samus is in the room: the statues
    /// return to awaiting their releases with no descent, then begin as a room load would.
    /// </summary>
    /// <param name="roomObjects">The room's room-object system.</param>
    internal void RestartWithStatuesOpen(RoomObjectSystem roomObjects)
    {
        _statues.Clear();
        _descentPhase = TourianStatueDescentPhase.AwaitingReleases;
        _holdUpdatesLeft = 0;
        Room.Descent = SignedSixteenSixteen.Zero;
        DisplayedVerticalOffset = 0;
        Begin(roomObjects);
    }

    /// <summary>Begins the room's statue sequence with its four statue animations; only the statue room, whose setup unlocks the statues, builds one (#2287).</summary>
    /// <remarks>
    /// A prior unlock event or the open-statues assist starts at the fully descended offset, clears the access obstruction through its room object, and enables vertical scrolling.
    /// Otherwise the sequence starts raised and constrains scrolling until the authored release and descent finish.
    /// </remarks>
    private void Begin(RoomObjectSystem roomObjects)
    {
        SpawnStatueAnimations();
        if (_assists.Current.TourianStatuesOpen || _system.HasEvent(StoryEvent.TourianUnlocked))
        {
            Room.Descent = SignedSixteenSixteen.FromWholePixels(TourianStatueSequenceDefinitions.DescentDistance);
            roomObjects.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, TourianAccessFloor.Clear);
            EnableScrolling();
        }
        else
        {
            _room.LoadedCamera.Scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.UpperScrollCell, RoomScrollState.OpenAlignedToTop);
            _room.LoadedCamera.Scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.LowerScrollCell, RoomScrollState.Closed);
        }
    }

    /// <summary>
    /// Spawns the four statue animations in order. They run in the order they were spawned, so the spawn order also
    /// decides which waiting statue claims the one release that plays at a time.
    /// </summary>
    private void SpawnStatueAnimations()
    {
        Spawn(TourianStatue.Kraid);
        Spawn(TourianStatue.Phantoon);
        Spawn(TourianStatue.Draygon);
        Spawn(TourianStatue.Ridley);

        // Resolve each statue's definition and start it at its animation entry.
        void Spawn(TourianStatue statue)
        {
            if (!TourianStatueAnimationDefinitions.Statues.TryGetValue(statue, out TourianStatueAnimationDefinition? definition))
                throw new InvalidDataException($"Tourian statue {statue} is not cataloged.");

            _statues.Add(new(definition));
        }
    }

    /// <summary>
    /// Runs the statue animations in spawn order: each statue still playing counts its picture's updates down and, when
    /// they run out, plays its typed animation on to the next picture, carrying out the animation's events on the way.
    /// </summary>
    public void StepBlockAnimations()
    {
        if (_assists.Current.TourianStatuesOpen) return;
        foreach (StatueBlockAnimation statueAnimation in _statues)
        {
            TourianStatueAnimationDefinition definition = statueAnimation.Definition;
            TourianStatueBlockAnimations animations = statueAnimation.Animations;
            if (statueAnimation.Player.StepToNewFrame(cue => RunCue(definition, animations, cue)) is { } picture)
                _room.LoadedScope.ArtworkFrames.Select(definition.Picture(picture));
        }

        // Carries out one event with the statue's values; an event whose condition holds returns the animation the statue starts.
        AnimationContinuation<TourianStatueBlockSegment>? RunCue(
            TourianStatueAnimationDefinition definition, TourianStatueBlockAnimations animations, TourianStatueBlockCue cue)
        {
            ITourianStatueActors statue = _statue;
            switch (cue.Kind)
            {
                case TourianStatueBlockCueKind.MarkAnimating:
                    statue.Animation.Animating |= TourianGreyStatueSets.Alone[definition.Statue];
                    return null;
                case TourianStatueBlockCueKind.MayAlreadyBeGrey:
                    return _system.HasEvent(definition.GreyEvent) ? Start(animations, AlreadyGreyAnimation) : null;
                case TourianStatueBlockCueKind.MayBeReleased:
                    return _system.HasDefeatedAnyOf(definition.BossArea, definition.DefeatedBosses) ? Start(animations, ReleaseAnimation) : null;
                case TourianStatueBlockCueKind.ClearAnimating:
                    statue.Animation.Animating &= ~TourianGreyStatueSets.Alone[definition.Statue];
                    return null;
                case TourianStatueBlockCueKind.MayWaitItsTurn:
                    return statue.Animation.ReleaseInProgress ? Start(animations, WaitingAnimation) : null;
                case TourianStatueBlockCueKind.ClaimRelease:
                    statue.Animation.ReleaseInProgress = true;
                    return null;
                case TourianStatueBlockCueKind.BlackOutEyes:
                    // The statue shows its eye colours blacked out, then its grey, as the shades the import pre-colours (#1512).
                    _statue.ShowStatueLook(definition.ClearedShade);
                    return null;
                case TourianStatueBlockCueKind.SpawnEyeGlow:
                    statue.SpawnEyeGlow(definition.Eye);
                    return null;
                case TourianStatueBlockCueKind.SpawnSoul:
                    statue.SpawnSoul(definition.Eye);
                    return null;
                case TourianStatueBlockCueKind.SpawnGreyColourEffect:
                    StartGreyOut(definition, statue);
                    return null;
                case TourianStatueBlockCueKind.SetGreyEvent:
                    _system.SetEvent(definition.GreyEvent);
                    return null;
                case TourianStatueBlockCueKind.FinishRelease:
                    statue.Animation.ReleaseInProgress = false;
                    statue.Animation.Animating &= ~TourianGreyStatueSets.Alone[definition.Statue];
                    return null;
                case TourianStatueBlockCueKind.TurnGrey:
                    _statue.ShowStatueLook(definition.GreyShade);
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TourianStatueBlockCueKind)} {cue.Kind}.");
            }
        }

        // A statue drawn in the room's background greys out on the room's pictures; a sprite statue greys out on its sprite,
        // which the grey-out hands its frames to (#2288).
        void StartGreyOut(TourianStatueAnimationDefinition definition, ITourianStatueActors statue)
        {
            SamusEquipmentFlags equipped = _player.IntroducedSamus.EquippedItems;
            if (definition.GreyedSprite is { } sprite)
                _roomColourEffects.StartShownOn(definition.ColourEffect, statue.SpriteStatueGreyOut(sprite), equipped);
            else
                _roomColourEffects.Start(definition.ColourEffect, equipped);
        }

        static AnimationContinuation<TourianStatueBlockSegment> Start(TourianStatueBlockAnimations animations, TourianStatueBlockSegment animation) =>
            new(animations.StartingWith(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>The animation a statue that is already grey starts, finishing grey (#2189).</summary>
    internal const TourianStatueBlockSegment AlreadyGreyAnimation = TourianStatueBlockSegment.AlreadyGrey;

    /// <summary>The animation a statue whose area boss is dead starts, its release (#2189).</summary>
    internal const TourianStatueBlockSegment ReleaseAnimation = TourianStatueBlockSegment.Release;

    /// <summary>The animation a statue waiting its turn to be released starts, oscillating again (#2189).</summary>
    internal const TourianStatueBlockSegment WaitingAnimation = TourianStatueBlockSegment.Oscillate;

    /// <summary>
    /// Runs the statues' descent for one update: once all four statues have released, the authored hold with its
    /// earthquake, then the quarter-pixel descent that opens the way into Tourian when it reaches its distance.
    /// </summary>
    public void StepDescent(RoomObjectSystem roomObjects)
    {
        if (_assists.Current.TourianStatuesOpen || _system.HasEvent(StoryEvent.TourianUnlocked)) { EnableScrolling(); return; }
        if (_descentPhase == TourianStatueDescentPhase.AwaitingReleases)
        {
            if (!_system.HasEvent(StoryEvent.PhantoonStatueGrey) || !_system.HasEvent(StoryEvent.RidleyStatueGrey)
                || !_system.HasEvent(StoryEvent.DraygonStatueGrey) || !_system.HasEvent(StoryEvent.KraidStatueGrey)) return;
            _statue.Animation.AllReleased = true;
            if (_statue.Animation.ReleaseInProgress) return;
            _descentPhase = TourianStatueDescentPhase.Holding;
            _holdUpdatesLeft = TourianStatueSequenceDefinitions.DescentDelay;
            return;
        }
        _enemies.Earthquake.Sustain(TourianStatueSequenceDefinitions.DescentEarthquake);
        _roomFxOverlay.PublishStatueEarthquakeSound(
            _system.RandomNumber, _system.EarthquakeSoundDelay);
        if (_descentPhase == TourianStatueDescentPhase.Holding)
        {
            if (--_holdUpdatesLeft == 0)
            {
                _statue.SpawnDescentDust();
                _descentPhase = TourianStatueDescentPhase.Lowering;
            }
            return;
        }
        if (_freeze.TimeIsFrozen) return;
        Room.Descent += TourianStatueSequenceDefinitions.DescentStep;
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
        _statue.Finished = true;
        _room.LoadedCamera.Scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.UpperScrollCell, RoomScrollState.Open);
        _room.LoadedCamera.Scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.LowerScrollCell, RoomScrollState.Open);
    }
}
