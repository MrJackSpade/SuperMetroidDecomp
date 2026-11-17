using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class BoyonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal BoyonBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Boyon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BoyonEntity(this, place, common);

    /// <summary>Carries out one cue of Boyon's animations.</summary>
    private BoyonSegment? ApplyBoyonCue(RoomEnemyPlace place, BoyonEnemyState state, BoyonCue cue)
    {
        switch (cue.Kind)
        {
            case BoyonCueKind.StartBounce:
                StartBoyonBounce(state);
                break;
            case BoyonCueKind.ProcessOffScreen:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                break;
            case BoyonCueKind.StopProcessingOffScreen:
                place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoyonCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>The enemy-library sound the start-bounce cue queues.</summary>
    private static readonly SoundEffectId BoyonBounceSound =
        EnemySoundEffectDefinitions.BoyonBounceSound;

    /// <summary>Initializes the Boyon.</summary>
    internal static void InitializeBoyon(RoomEnemyPlace place)
    {
        BoyonSpawnSettings settings = place.SpawnSettings<BoyonSpawnSettings>();

        // Initialization writes the ordinary empty frame before selecting the
        // idle animation. RoomEnemySystem's load tail intentionally restores the same transient
        // empty frame; the first processed animation frame installs real art.
        place.Frame = EnemyFrameId.Empty;

        var state = new BoyonEnemyState
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = settings.SpeedMultiplier,
            JumpHeight = SignedSixteenSixteen.FromWholePixels(settings.JumpHeight),
            ArcDistance = SignedSixteenSixteen.Zero,
            CurvePlace = BoyonCurvePlace.Top,
            Bouncing = false,
            BaselineCurvePlace = BoyonCurvePlace.Top,
            BounceDisabled = false,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        place.SetSpeciesState(state);
        SetBoyonAnimation(place, state, BoyonSegment.Idle);
    }

    /// <summary>Runs the Boyon's main AI.</summary>
    internal static void RunBoyonMain(RoomEnemyPlace place, BoyonEnemyState state, SamusState samus)
    {
        // The game spends Boyon's first active AI frame deriving the arc's baseline and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (!state.BounceSpeedCalculated)
        {
            DeriveBoyonArcBaseline(state);
            state.BounceSpeedCalculated = true;
            return;
        }

        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(place, samus);
        if (samusIsClose)
        {
            // Proximity continuously permits another bounce. The separate Bouncing latch
            // prevents restarting the animation every frame during the current arc.
            state.BounceDisabled = false;
            state.IdleDisabled = false;
            if (!state.Bouncing)
            {
                state.Bouncing = true;
                SetBoyonAnimation(place, state, BoyonSegment.Bouncing);
            }
        }
        else if (state.BounceDisabled)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (!state.IdleDisabled)
            {
                state.IdleDisabled = true;
                SetBoyonAnimation(place, state, BoyonSegment.Idle);
            }
            return;
        }

        switch (state.BounceMovement)
        {
            case BoyonBounceMovement.Falling:
                MoveBoyonFalling(place, state);
                break;
            case BoyonBounceMovement.Rising:
                MoveBoyonRising(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Boyon has no behaviour for bounce movement {state.BounceMovement}.");
        }
    }

    /// <summary>
    /// Derives the arc's baseline: from the place it stands, the Boyon moves down its curve one entry at a time, adding
    /// each entry's scaled sample to the arc's distance, until the distance reaches the jump height; the place it then
    /// stands at is where every fall ends. Each entry past the top adds at least one 256th of a pixel, so the derivation
    /// ends; the state holds no multiplier below one.
    /// </summary>
    internal static void DeriveBoyonArcBaseline(BoyonEnemyState state)
    {
        do
        {
            state.Speed = MultiplyBoyonCurveEntry(state);
            state.AccumulateArcDistance(state.Speed);
            state.FallOneEntry();
        }
        while (!state.HasReachedJumpHeight);

        state.BaselineCurvePlace = state.CurvePlace;
        state.BounceDisabled = true;
        state.IdleDisabled = true;
    }

    /// <summary>Runs falling movement.</summary>
    private static void MoveBoyonFalling(RoomEnemyPlace place, BoyonEnemyState state)
    {
        state.FallOneEntry();
        state.Speed = MultiplyBoyonCurveEntry(state);

        // Taking the product's whole pixels discards its fraction. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's whole pixels.
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + state.Speed.Whole);
        if (state.CurvePlace < state.BaselineCurvePlace)
            return;

        state.BounceMovement = BoyonBounceMovement.Rising;
        state.BounceDisabled = true;
        state.Bouncing = false;
    }

    /// <summary>Runs rising movement.</summary>
    private static void MoveBoyonRising(RoomEnemyPlace place, BoyonEnemyState state)
    {
        state.Speed = MultiplyBoyonCurveEntry(state);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - state.Speed.Whole);
        if (state.RiseOneEntry())
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// The Boyon's speed at its place: the curve sample there scaled by its multiplier, in 256ths of a pixel an update.
    /// Entries after the stored curve use the explicit saturated sample rather than reading past it.
    /// </summary>
    internal static UnsignedSixteenSixteen MultiplyBoyonCurveEntry(BoyonEnemyState state) =>
        UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(BoyonBounceCurve.Multiply(state.CurvePlace, state.SpeedMultiplier));

    /// <summary>Whether Samus lies within Boyon's authored horizontal reach, in pixel columns, of the enemy.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(RoomEnemyPlace place, SamusState samus)
    {
        int distance = CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole);
        return CoordinateAxis.Precedes(distance, place.SpawnSettings<BoyonSpawnSettings>().TriggerDistance);
    }

    /// <summary>Starts the idle or bounce animation and restarts its frame and auxiliary timers.</summary>
    private static void SetBoyonAnimation(RoomEnemyPlace place, BoyonEnemyState state, BoyonSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, BoyonAnimations.StartingWith(segment));
    }

    /// <summary>The bounce cue: permit the arc and request the bounce sound.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        _audio.QueueEnemySound(BoyonBounceSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Boyon's idle and bouncing loops (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, BoyonAnimations.Segments,
            static (BoyonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBoyonCue(place, state, cue) is { } target
                ? new AnimationContinuation<BoyonSegment>(BoyonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
