using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BoyonEntity(this, slot, common);

    /// <summary>Carries out one cue of Boyon's animations.</summary>
    private BoyonSegment? ApplyBoyonCue(RoomEnemySlot slot, BoyonEnemyState state, BoyonCue cue)
    {
        switch (cue.Kind)
        {
            case BoyonCueKind.Return:
                // The return cue has no effect; keeping it documents the idle-animation seam.
                break;
            case BoyonCueKind.StartBounce:
                StartBoyonBounce(state);
                break;
            case BoyonCueKind.ProcessOffScreen:
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
                break;
            case BoyonCueKind.StopProcessingOffScreen:
                slot.Processing = slot.Processing.Without(EnemyProcessing.OffScreen);
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
    internal static void InitializeBoyon(RoomEnemySlot slot)
    {
        BoyonSpawnSettings settings = slot.SpawnSettings<BoyonSpawnSettings>();

        // Initialization writes the ordinary empty frame before selecting the
        // idle animation. RoomEnemySystem's load tail intentionally restores the same transient
        // empty frame; the first processed animation frame installs real art.
        slot.Frame = EnemyFrameId.Empty;

        var state = new BoyonEnemyState
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = settings.SpeedMultiplier,
            JumpHeight = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(settings.JumpHeight),
            ArcDistance = SignedSixteenSixteen.Zero,
            CurvePlace = BoyonCurvePlace.Top,
            Bouncing = false,
            BaselineCurvePlace = BoyonCurvePlace.Top,
            BounceDisabled = false,
            SpeedMultiplierMirror = 0,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        slot.SetSpeciesState(state);
        SetBoyonAnimation(slot, state, BoyonSegment.Idle);
    }

    /// <summary>Runs the Boyon's main AI.</summary>
    internal static void RunBoyonMain(RoomEnemySlot slot, BoyonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Boyon proximity AI requires the active Samus actor.");

        // The game spends Boyon's first active AI frame deriving the arc's baseline and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (!state.BounceSpeedCalculated)
        {
            DeriveBoyonArcBaseline(state);
            state.BounceSpeedCalculated = true;
            return;
        }

        state.SpeedMultiplierMirror = state.SpeedMultiplier;
        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(slot, samus);
        if (samusIsClose)
        {
            // Proximity continuously permits another bounce. The separate Bouncing latch
            // prevents restarting the animation every frame during the current arc.
            state.BounceDisabled = false;
            state.IdleDisabled = false;
            if (!state.Bouncing)
            {
                state.Bouncing = true;
                SetBoyonAnimation(slot, state, BoyonSegment.Bouncing);
            }
        }
        else if (state.BounceDisabled)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (!state.IdleDisabled)
            {
                state.IdleDisabled = true;
                SetBoyonAnimation(slot, state, BoyonSegment.Idle);
            }
            return;
        }

        // The speed multiplier is restored from its copy before the bounce moves, so an animation
        // cue during this frame cannot change the current displacement.
        state.SpeedMultiplier = state.SpeedMultiplierMirror;
        switch (state.BounceMovement)
        {
            case BoyonBounceMovement.Falling:
                MoveBoyonFalling(slot, state);
                break;
            case BoyonBounceMovement.Rising:
                MoveBoyonRising(slot, state);
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
    /// ends; a multiplier outside 1 through <see cref="BoyonSpeedDefinitions.LargestArcMultiplier"/> is refused.
    /// </summary>
    internal static void DeriveBoyonArcBaseline(BoyonEnemyState state)
    {
        _ = Ensure.BetweenInclusive(1, BoyonSpeedDefinitions.LargestArcMultiplier, state.SpeedMultiplier);
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
    private static void MoveBoyonFalling(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.FallOneEntry();
        state.Speed = MultiplyBoyonCurveEntry(state);

        // Taking the product's whole pixels discards its fraction. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's whole pixels.
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + (EightEight.WholeOf(state.Speed)));
        if (state.CurvePlace < state.BaselineCurvePlace)
            return;

        state.BounceMovement = BoyonBounceMovement.Rising;
        state.BounceDisabled = true;
        state.Bouncing = false;
    }

    /// <summary>Runs rising movement.</summary>
    private static void MoveBoyonRising(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.Speed = MultiplyBoyonCurveEntry(state);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - (EightEight.WholeOf(state.Speed)));
        if (state.RiseOneEntry())
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// The curve sample at the Boyon's place scaled by its multiplier. Entries after the stored curve use the explicit
    /// saturated sample rather than reading past it.
    /// </summary>
    internal static int MultiplyBoyonCurveEntry(BoyonEnemyState state) =>
        BoyonBounceCurve.Multiply(state.CurvePlace, state.SpeedMultiplier);

    /// <summary>Whether Samus lies within Boyon's authored horizontal reach, in pixel columns, of the enemy.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(RoomEnemySlot slot, SamusState samus)
    {
        int distance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
        return CoordinateAxis.Precedes(distance, slot.SpawnSettings<BoyonSpawnSettings>().TriggerDistance);
    }

    /// <summary>Starts the idle or bounce animation and restarts its frame and auxiliary timers.</summary>
    private static void SetBoyonAnimation(RoomEnemySlot slot, BoyonEnemyState state, BoyonSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, BoyonAnimations.StartingWith(segment));
    }

    /// <summary>The bounce cue: permit the arc and request the bounce sound.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        _audio.QueueEnemySound(BoyonBounceSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Boyon's idle and bouncing loops (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, BoyonAnimations.Segments,
            static (BoyonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBoyonCue(slot, state, cue) is { } target
                ? new AnimationContinuation<BoyonSegment>(BoyonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
