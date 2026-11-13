using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    private BoyonProgram? ApplyBoyonCue(RoomEnemySlot slot, BoyonEnemyState state, BoyonCue cue)
    {
        switch (cue.Kind)
        {
            case BoyonCueKind.Return:
                // The return cue has no effect; keeping it documents the idle-list seam.
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
        // idle list. RoomEnemySystem's load tail intentionally restores the same transient
        // empty map; the first processed instruction frame installs real art.
        slot.Frame = EnemyFrameId.Empty;

        var state = new BoyonEnemyState
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = settings.SpeedMultiplier,
            JumpHeight = settings.JumpHeight,
            DistanceAccumulator = 0,
            SpeedTableIndex = 0,
            Bouncing = false,
            InitialBounceSpeedTableIndex = 0,
            BounceDisabled = false,
            SpeedMultiplierMirror = 0,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        slot.SetSpeciesState(state);
        SetBoyonProgram(slot, state, BoyonProgram.Idle);
    }

    /// <summary>Runs the Boyon's main AI.</summary>
    internal static void RunBoyonMain(RoomEnemySlot slot, BoyonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Boyon proximity AI requires the active Samus actor.");

        // The game spends Boyon's first active AI frame deriving the curve index and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (!state.BounceSpeedCalculated)
        {
            CalculateInitialBoyonBounceSpeed(state);
            state.BounceSpeedCalculated = true;
            return;
        }

        state.SpeedMultiplierMirror = state.SpeedMultiplier;
        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(slot, samus);
        if (samusIsClose)
        {
            // Proximity continuously permits another bounce. The separate Bouncing latch
            // prevents resetting the animation list every frame during the current arc.
            state.BounceDisabled = false;
            state.IdleDisabled = false;
            if (!state.Bouncing)
            {
                state.Bouncing = true;
                SetBoyonProgram(slot, state, BoyonProgram.Bouncing);
            }
        }
        else if (state.BounceDisabled)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (!state.IdleDisabled)
            {
                state.IdleDisabled = true;
                SetBoyonProgram(slot, state, BoyonProgram.Idle);
            }
            return;
        }

        // The speed multiplier is restored from its copy before the bounce moves, so an animation
        // command during this frame cannot change the current displacement.
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
                    $"Boyon movement {state.BounceMovement} is not translated.");
        }
    }

    /// <summary>Runs the one-time loop.</summary>
    private static void CalculateInitialBoyonBounceSpeed(BoyonEnemyState state)
    {
        // Every iteration multiplies two values of 0 through 255,
        // adds the product, wrapping past 65535, then compares by the result's sign.
        // The curve saturates at 255 once its 23 stored samples are exhausted.
        for (int guard = 0; guard < BoyonEnemyState.LargestSpeedTableIndex; guard++)
        {
            state.Speed = MultiplyBoyonCurveEntry(state);
            state.AccumulateBounceDistance(state.Speed);
            state.AdvanceSpeedTableIndex();

            if (state.HasReachedJumpHeight)
            {
                state.InitialBounceSpeedTableIndex = state.SpeedTableIndex;
                state.BounceDisabled = true;
                state.IdleDisabled = true;
                return;
            }
        }

        throw new InvalidDataException(
            "Boyon initial bounce-speed calculation did not reach its authored jump height.");
    }

    /// <summary>Runs falling movement.</summary>
    private static void MoveBoyonFalling(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.AdvanceSpeedTableIndex();
        state.Speed = MultiplyBoyonCurveEntry(state);

        // Taking the product's whole pixels discards its fraction. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's whole pixels.
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + (EightEight.WholeOf(state.Speed)));
        if (WrappingCounter.IsBehind(state.SpeedTableIndex, state.InitialBounceSpeedTableIndex))
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
        if (state.RetreatSpeedTableIndex())
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// The curve sample at the current index scaled by the multiplier. Indexes after the stored curve use the explicit
    /// saturated sample rather than reading past it.
    /// </summary>
    internal static int MultiplyBoyonCurveEntry(BoyonEnemyState state) =>
        BoyonBounceCurve.Multiply(state.SpeedTableIndex, state.SpeedMultiplier);

    /// <summary>Whether Samus lies within Boyon's authored horizontal reach, in pixel columns, of the enemy.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(RoomEnemySlot slot, SamusState samus)
    {
        int distance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
        return CoordinateAxis.Precedes(distance, slot.SpawnSettings<BoyonSpawnSettings>().TriggerDistance);
    }

    /// <summary>Starts the idle or bounce animation and restarts its instruction and auxiliary timers.</summary>
    private static void SetBoyonProgram(RoomEnemySlot slot, BoyonEnemyState state, BoyonProgram program)
    {
        state.Playback = new(BoyonAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>The bounce cue: permit the arc and request the bounce sound.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        _audio.QueueEnemySound(BoyonBounceSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Boyon's idle and bouncing loops as typed programs (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BoyonAnimations.Segments, BoyonFrameDefinitions.ShownFrames.Of,
            static (BoyonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBoyonCue(slot, state, cue) is { } target
                ? new AnimationContinuation<BoyonProgram>(BoyonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
