using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BoyonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' end-of-update sound requests.</summary>
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
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case BoyonCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoyonCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>The library-two sound the start-bounce cue queues.</summary>
    private static readonly SoundEffectId BoyonBounceSound =
        EnemySoundEffectDefinitions.BoyonBounceSound;

    /// <summary>Initializes the Boyon.</summary>
    internal static void InitializeBoyon(RoomEnemySlot slot)
    {
        BoyonSpawnSettings settings = slot.SpawnSettings<BoyonSpawnSettings>();
        int multiplierIndex = settings.SpeedMultiplierIndex;
        int jumpHeightIndex = settings.JumpHeightIndex;
        if (multiplierIndex >= 8 || jumpHeightIndex >= 9)
        {
            throw new InvalidDataException(
                $"Boyon speed multiplier {multiplierIndex} or jump height {jumpHeightIndex} indexes beyond its " +
                "eight multipliers or nine jump heights.");
        }

        // Initialization writes the ordinary empty frame before selecting the
        // idle list. RoomEnemySystem's load tail intentionally restores the same transient
        // empty map; the first processed instruction frame installs real art.
        slot.Frame = EnemyFrameId.Empty;

        var state = new BoyonEnemyState
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = BoyonBounceCurve.InitialMultiplier(multiplierIndex),
            JumpHeight = BoyonBounceCurve.InitialHeight(jumpHeightIndex),
            DistanceAccumulator = 0,
            SpeedTableIndex = 0,
            Bouncing = false,
            InitialBounceSpeedTableIndex = 0,
            BounceDisabled = false,
            SpeedMultiplierMirror = 0,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
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
        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(slot, state, samus);
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

        // The routine restores variable A from its mirror before indirect dispatch.
        // The speed is restored from its copy before the bounce moves, so an animation
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
        // Every iteration multiplies two low bytes,
        // adds the 16-bit product with wraparound, then compares by the result's sign.
        // The curve saturates at 255 once its 23 stored bytes are exhausted.
        for (int guard = 0; guard < ushort.MaxValue; guard++)
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

        // Taking the high byte discards the product's fractional byte. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's high byte.
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + (EightEight.WholeOf(state.Speed)));
        if (EnemyMath.IsNegative16(state.SpeedTableIndex - state.InitialBounceSpeedTableIndex))
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
    /// Recreates the 8×8 unsigned multiply for the current curve index. Values after
    /// the stored table use the explicit saturated byte rather than reading past the table.
    /// </summary>
    internal static int MultiplyBoyonCurveEntry(BoyonEnemyState state) =>
        BoyonBounceCurve.Multiply(state.SpeedTableIndex, state.SpeedMultiplier);

    /// <summary>Ports <c>IsSamusWithinAPixelColumnsOfEnemy</c> for Boyon's init1 radius.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(
        RoomEnemySlot slot,
        BoyonEnemyState state,
        SamusState samus)
    {
        _ = state; // Keeps the same signature as the family's other trigger tests.
        int distance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
        return EnemyMath.IsNegative16(distance - slot.SpawnSettings<BoyonSpawnSettings>().TriggerDistance);
    }

    /// <summary>Starts the idle or bounce animation and restarts its instruction and auxiliary timers.</summary>
    private static void SetBoyonProgram(RoomEnemySlot slot, BoyonEnemyState state, BoyonProgram program)
    {
        state.Program = AnimationProgramCursor<BoyonProgram>.Start(program);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>The bounce cue: permit the arc and request sound 14.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Boyon, BoyonBounceSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Boyon's idle and bouncing loops as typed programs (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BoyonAnimations.Programs, BoyonFrameDefinitions.ShownFrames.Of,
            static (BoyonEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyBoyonCue(slot, state, cue));
}
