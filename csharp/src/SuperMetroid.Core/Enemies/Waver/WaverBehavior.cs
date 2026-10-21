using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The Waver species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WaverBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Waver];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WaverEntity(this, slot, common);

    /// <summary>Room collision service used to reflect horizontal motion and the sine wave on impact.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Connects Waver AI to room terrain collision.</summary>
    internal WaverBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out Waver's one animation cue.</summary>
    private static WaverProgram? ApplyWaverCue(WaverEnemyState state, WaverCue cue)
    {
        switch (cue.Kind)
        {
            case WaverCueKind.FinishSpin:
                // The four-frame spin hands its completion back to main AI rather than
                // going on to the steady animation itself.
                state.SpinFinished = true;
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WaverCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Native horizontal speed of one and a half pixels per update in signed 16.16 form.</summary>
    private const int WaverHorizontalSpeedFixed = 98304;
    /// <summary>Four-pixel amplitude used by Waver's negative-sine vertical motion.</summary>
    private const int WaverVerticalRadius = 4;

    /// <summary>Ports <c>InitAI_Waver</c> at <c>$A3:86ED</c>.</summary>
    internal static void InitializeWaver(RoomEnemySlot slot)
    {
        bool facingRight = (slot.Spawn.Population.Parameter1 & 1) != 0;
        int signedSpeed = facingRight
            ? WaverHorizontalSpeedFixed
            : -WaverHorizontalSpeedFixed;
        var state = new WaverEnemyState
        {
            XVelocity = SnesSignedSixteenSixteen.FromRaw(signedSpeed),
            CurrentInstructionListIndex = WaverAnimationSelector.None,
            Angle = SnesAngle.Zero,
            SpinFinished = false,
            RequestedInstructionListIndex = facingRight ? WaverAnimationSelector.FacingRight : WaverAnimationSelector.None,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The native initializer first installs the left steady animation and then invokes the
        // change detector. Parameter bit zero therefore changes only right-facing spawns;
        // left-facing spawns intentionally observe current==requested and keep this one.
        state.Program = AnimationProgramCursor<WaverProgram>.Start(WaverProgram.SteadyFacingLeft);
        SetWaverProgram(slot, state);
    }

    /// <summary>Ports <c>MainAI_Waver</c> at <c>$A3:874C</c>.</summary>
    private void RunWaverMain(
        RoomEnemySlot slot,
        WaverEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Waver AI requires active room collision data.");

        int horizontalDisplacement = state.XVelocity.RawValue;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            // `$A3:875F-$8781` negates the overlapping velocity bytes and writes them back
            // as two words. For every initialized Waver this is exactly a signed 16.16
            // negation; expressing that value directly preserves the native ±1.5 px/frame.
            state.XVelocity = SnesSignedSixteenSixteen.FromRaw(unchecked(-horizontalDisplacement));
            state.RequestedInstructionListIndex =
                (state.RequestedInstructionListIndex ^ WaverAnimationSelector.FacingRight) &
                WaverAnimationSelector.FacingRight;
            SetWaverProgram(slot, state);
        }
        else
        {
            int verticalPixels = EnemyMath.ReadEightBitNegativeSineProduct(
                state.Angle.ToByteAngle(),
                WaverVerticalRadius);
            if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(verticalPixels)))
            {
                state.ReflectWave();
            }
            else
            {
                state.AdvanceWave();
            }
        }

        // Once per half-wave, the phase's position within its half-turn equals $38. Waver temporarily swaps to
        // the four-map spin list without changing the facing bit.
        if ((state.Angle.TableIndex & EnemyByteAngleDefinitions.HalfWaveIndexMask) == 56)
        {
            state.RequestedInstructionListIndex |= WaverAnimationSelector.Spinning;
            SetWaverProgram(slot, state);
        }

        // The list's $86:E3 command sets this flag after 32 animation frames. Main AI owns
        // the return to steady art, matching the command/AI seam rather than timing it here.
        if (state.SpinFinished)
        {
            state.SpinFinished = false;
            state.RequestedInstructionListIndex &= WaverAnimationSelector.FacingRight;
            SetWaverProgram(slot, state);
        }
    }

    /// <summary>Starts a changed facing or spin animation and restarts its instruction and loop timers.</summary>
    internal static void SetWaverProgram(RoomEnemySlot slot, WaverEnemyState state)
    {
        WaverAnimationSelector requested = state.RequestedInstructionListIndex;
        if (requested == state.CurrentInstructionListIndex)
            return;

        state.CurrentInstructionListIndex = requested;
        state.Program = AnimationProgramCursor<WaverProgram>.Start(WaverAnimationDefinitions.Program(requested));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports Waver's main AI.</summary>
    internal void RunWaverMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWaverMain(slot, slot.RequireSpeciesState<WaverEnemyState>(), frame.Level);
    }

    /// <summary>Advances Waver's typed animation.</summary>
    internal static void AdvanceWaverTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, WaverAnimations.Programs, WaverFrameDefinitions.ShownFrames.Of,
            static (WaverEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyWaverCue(state, cue));
}
