using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// Owns the four statue animations and the statues' descent into the floor, each update. Boss victories and grey-statue
/// events stay separate: visiting the room plays each release before opening the elevator route.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueSequence : IDisplayLatch
{

    /// <summary>The room's earthquake, which the descent shakes.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>
    /// The statue room the sequence lowers the statue in (#2382), which its statues' decorations follow down and its unlock
    /// effects splash in, and whose statues' shared animation state (which statues animate, which one is releasing, whether
    /// all have released) the statue animations set (#2486).
    /// </summary>
    private TourianStatueRoom Room { get; }
    /// <summary>The entrance statue and the unlock effects in the statue room, which the statue animations drive.</summary>
    private readonly ITourianStatueActors _statue;
    /// <summary>Supplies equipped items to colour effects started during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The room's level, where the way into Tourian opens.</summary>
    private readonly RoomLevelData _level;
    /// <summary>The room's scroll grid, whose two screens the statues hold until the way opens.</summary>
    private readonly RoomScrollGrid _scrolls;
    /// <summary>The frame each of the room's animations shows, which the statues' block animations select into.</summary>
    private readonly ArtworkFrameSelections _artworkFrames;
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
    /// <param name="room">The statue room's parts the sequence acts on, as the room's load has built them (#2486).</param>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies, TourianStatueSequenceRoom room)
    {
        Ensure.NotNull(room);
        _earthquake = Ensure.NotNull(room.Earthquake);
        Room = Ensure.NotNull(room.StatueRoom);
        _statue = Ensure.NotNull(room.Statue);
        _player = dependencies.Player;
        _level = Ensure.NotNull(room.Level);
        _scrolls = Ensure.NotNull(room.Scrolls);
        _artworkFrames = Ensure.NotNull(room.ArtworkFrames);
        _roomFxOverlay = Ensure.NotNull(room.FxOverlay);
        _roomColourEffects = Ensure.NotNull(room.ColourEffects);
        _system = dependencies.System;
        _assists = dependencies.Assists;
        _freeze = dependencies.Freeze;
        Begin(Ensure.NotNull(room.RoomObjects));
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
    /// The pre-descent hold of the <see cref="TourianStatueDescentPhase.Holding"/> phase, started as the phase begins: the
    /// update it ends on raises the dust. The hold shows no picture, so it is the phase's own wait, not an animation.
    /// </summary>
    private readonly UpdateCountdown _hold = UpdateCountdown.Ended();
    /// <summary>The statue room's statue offset: minus the whole pixels of its descent (<see cref="TourianStatueRoom.StatueOffset"/>).</summary>
    private int VerticalOffset => Room.StatueOffset;
    /// <summary>How far the statues have sunk, as the displayed frame shows it.</summary>
    public int DisplayedVerticalOffset { get; private set; }
    /// <summary>Latches how far the statues have sunk for the displayed frame.</summary>
    void IDisplayLatch.LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>
    /// Restarts the room's statue sequence when the statues newly stand open while Samus is in the room: the statues
    /// return to awaiting their releases with no descent, then begin as a room load would.
    /// </summary>
    /// <param name="roomObjects">The room's room-object system.</param>
    internal void RestartWithStatuesOpen(RoomObjectSystem roomObjects)
    {
        _statues.Clear();
        _descentPhase = TourianStatueDescentPhase.AwaitingReleases;
        _hold.End();
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
            roomObjects.Tourian.SpawnTourianAccess(_level, TourianAccessFloor.Clear);
            EnableScrolling();
        }
        else
        {
            _scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.UpperScrollCell, RoomScrollState.OpenAlignedToTop);
            _scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.LowerScrollCell, RoomScrollState.Closed);
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
            var target = new StatueCueTarget(this, definition, animations);
            if (statueAnimation.Player.StepToNewFrame(cue => cue.Run(target)) is { } picture)
                _artworkFrames.Select(definition.Picture(picture));
        }
    }

    /// <summary>
    /// One statue as its animation's cues act on it (#2571): each cue carries out its effect with the statue's values, and a
    /// cue whose condition holds returns the animation the statue starts.
    /// </summary>
    /// <param name="sequence">The statue sequence.</param>
    /// <param name="definition">The statue's definition.</param>
    /// <param name="animations">The statue's animations.</param>
    private sealed class StatueCueTarget(TourianStatueSequence sequence, TourianStatueAnimationDefinition definition,
        TourianStatueBlockAnimations animations) : ITourianStatueCueTarget
    {
        /// <inheritdoc/>
        public void MarkAnimating() => sequence._statue.Animation.MarkAnimating(TourianGreyStatueSets.Alone[definition.Statue]);

        /// <inheritdoc/>
        public AnimationContinuation<TourianStatueBlockSegment>? MayAlreadyBeGrey() =>
            sequence._system.HasEvent(definition.GreyEvent) ? Start(AlreadyGreyAnimation) : null;

        /// <inheritdoc/>
        public AnimationContinuation<TourianStatueBlockSegment>? MayBeReleased() =>
            sequence._system.HasDefeatedAnyOf(definition.BossArea, definition.DefeatedBosses) ? Start(ReleaseAnimation) : null;

        /// <inheritdoc/>
        public void ClearAnimating() => sequence._statue.Animation.StopAnimating(TourianGreyStatueSets.Alone[definition.Statue]);

        /// <inheritdoc/>
        public AnimationContinuation<TourianStatueBlockSegment>? MayWaitItsTurn() =>
            sequence._statue.Animation.ReleaseInProgress ? Start(WaitingAnimation) : null;

        /// <inheritdoc/>
        public void ClaimRelease() => sequence._statue.Animation.ClaimRelease();

        /// <inheritdoc/>
        public void BlackOutEyes() =>
            // The statue shows its eye colours blacked out, then its grey, as the shades the import pre-colours (#1512).
            sequence._statue.ShowStatueLook(definition.ClearedShade);

        /// <inheritdoc/>
        public void SpawnEyeGlow() => sequence._statue.SpawnEyeGlow(definition.Eye);

        /// <inheritdoc/>
        public void SpawnSoul() => sequence._statue.SpawnSoul(definition.Eye);

        /// <inheritdoc/>
        public void StartGreyOut()
        {
            // A statue drawn in the room's background greys out on the room's pictures; a sprite statue greys out on its
            // sprite, which the grey-out hands its frames to (#2288).
            SamusEquipmentFlags equipped = sequence._player.IntroducedSamus.EquippedItems;
            if (definition.GreyedSprite is { } sprite)
                sequence._roomColourEffects.StartShownOn(definition.ColourEffect, sequence._statue.SpriteStatueGreyOut(sprite), equipped);
            else
                sequence._roomColourEffects.Start(definition.ColourEffect, equipped);
        }

        /// <inheritdoc/>
        public void SetGreyEvent() => sequence._system.SetEvent(definition.GreyEvent);

        /// <inheritdoc/>
        public void FinishRelease() => sequence._statue.Animation.FinishRelease(TourianGreyStatueSets.Alone[definition.Statue]);

        /// <inheritdoc/>
        public void TurnGrey() => sequence._statue.ShowStatueLook(definition.GreyShade);

        /// <summary>The statue's animation <paramref name="animation"/>, from its start.</summary>
        private AnimationContinuation<TourianStatueBlockSegment> Start(TourianStatueBlockSegment animation) =>
            new(animations.StartingWith(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>The animation a statue that is already grey starts, finishing grey (#2189).</summary>
    private const TourianStatueBlockSegment AlreadyGreyAnimation = TourianStatueBlockSegment.AlreadyGrey;

    /// <summary>The animation a statue whose area boss is dead starts, its release (#2189).</summary>
    private const TourianStatueBlockSegment ReleaseAnimation = TourianStatueBlockSegment.Release;

    /// <summary>The animation a statue waiting its turn to be released starts, oscillating again (#2189).</summary>
    private const TourianStatueBlockSegment WaitingAnimation = TourianStatueBlockSegment.Oscillate;

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
            _statue.Animation.LatchAllReleased();
            if (_statue.Animation.ReleaseInProgress) return;
            _descentPhase = TourianStatueDescentPhase.Holding;
            _hold.Restart(TourianStatueSequenceDefinitions.DescentDelay);
            return;
        }
        _earthquake.Sustain(TourianStatueSequenceDefinitions.DescentEarthquake);
        _roomFxOverlay.PublishStatueEarthquakeSound(
            _system.RandomNumber, _system.EarthquakeSoundDelay);
        if (_descentPhase == TourianStatueDescentPhase.Holding)
        {
            if (_hold.Count())
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
            roomObjects.Tourian.SpawnTourianAccess(_level, TourianAccessFloor.Crumble);
            _system.SetEvent(StoryEvent.TourianUnlocked);
        }
    }

    /// <summary>Marks the entrance statue finished and makes both access-room scroll cells freely traversable.</summary>
    private void EnableScrolling()
    {
        _statue.Animation.FinishSinking();
        _scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.UpperScrollCell, RoomScrollState.Open);
        _scrolls.SetRoomScreen(TourianStatueSequenceDefinitions.LowerScrollCell, RoomScrollState.Open);
    }
}
