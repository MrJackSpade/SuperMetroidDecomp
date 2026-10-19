using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The Hoppers species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class HoppersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dessgeega, EnemyDefinitionId.DessgeegaLarge, EnemyDefinitionId.Sidehopper, EnemyDefinitionId.SidehopperLarge, EnemyDefinitionId.SidehopperTourian];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new HoppersEntity(this, slot, common);

    /// <summary>Supplies the shared RNG seed consumed during Hopper initialization.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves vertical and horizontal collisions in both floor and ceiling hops.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds the five Hopper definitions to shared room RNG and terrain movement.</summary>
    internal HoppersBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the hoppers' animations.</summary>
    private HopperProgram? ApplyHopperCue(RoomEnemySlot slot, HopperEnemyState state, HopperCue cue)
    {
        switch (cue.Kind)
        {
            case HopperCueKind.ProcessOffScreen:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case HopperCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            case HopperCueKind.PlaySound:
                LastHopperSoundEffect = cue.Sound ?? throw new InvalidOperationException("A hopper sound cue has no sound.");
                break;
            case HopperCueKind.ReadyToHop:
                state.ReadyToHop = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(HopperCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Native global RNG seed written when a Hopper initializes.</summary>
    private const ushort HopperRandomSeed = 37;
    /// <summary>Terminal quadratic-speed table index used to cap the falling half of a hop.</summary>
    private const int MaximumHopperYSpeedTableIndex = 64;

    /// <summary>Ports <c>InitAI_Hopper</c> at $A3:AB09.</summary>
    internal void InitializeHopper(RoomEnemySlot slot)
    {
        if (slot.Definition.VariantIndex > 3)
        {
            throw new InvalidDataException(
                $"Hopper definition ${(int)slot.EnemyDefinitionPointer:X4} variant " +
                $"{slot.Definition.VariantIndex} exceeds its four-entry animation tables.");
        }
        // The initializer overwrites the global RNG seed and consumes one generated value
        // even though it never reads that value. This changes later enemy randomness and is
        // therefore observable whenever a hopper appears earlier in population order.
        _room.Random.Seed(HopperRandomSeed);
        _room.Random.Generate();

        var state = new HopperEnemyState
        {
            Falling = false,
            ReadyToHop = false,
            Variant = slot.Definition.VariantIndex,
            UpsideDown = slot.Spawn.Population.Parameter1 != 0,
            Function = HopperEnemyFunction.ChooseHopSize,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        SetHopperProgram(slot, state, ReadHopperProgram(
            state, state.UpsideDown, jumping: false));

        // Both physics rows currently contain the same constants, but the header-dependent
        // byte offset and two independent calculations are part of the ROM contract.
        state.SmallHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 4096,
            tableIndexDelta: 3);
        state.BigHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 12288,
            tableIndexDelta: 4);
    }

    /// <summary>Ports <c>MainAI_Hopper</c> and every indirect target at $A3:ABCF-$AD56.</summary>
    private void RunHopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        HopperEnemyState state = RequireHopperState(slot);
        switch (state.Function)
        {
            case HopperEnemyFunction.ChooseHopSize:
                // The ROM consumes an entire frame choosing one of two setup functions.
                state.Function = (_room.Random.Generate() & 1) == 0
                    ? HopperEnemyFunction.PrepareSmallHop
                    : HopperEnemyFunction.PrepareBigHop;
                return;
            case HopperEnemyFunction.PrepareSmallHop:
                PrepareHopperHop(state, bigHop: false);
                return;
            case HopperEnemyFunction.PrepareBigHop:
                PrepareHopperHop(state, bigHop: true);
                return;
            case HopperEnemyFunction.ChooseDirectionUpsideUp:
                ChooseHopperDirection(slot, state, samus, upsideDown: false);
                return;
            case HopperEnemyFunction.ChooseDirectionUpsideDown:
                ChooseHopperDirection(slot, state, samus, upsideDown: true);
                return;
            case HopperEnemyFunction.StartBackwardHopUpsideUp:
                StartHopperJump(slot, state, upsideDown: false, backward: true);
                return;
            case HopperEnemyFunction.StartForwardHopUpsideUp:
                StartHopperJump(slot, state, upsideDown: false, backward: false);
                return;
            case HopperEnemyFunction.StartBackwardHopUpsideDown:
                StartHopperJump(slot, state, upsideDown: true, backward: true);
                return;
            case HopperEnemyFunction.StartForwardHopUpsideDown:
                StartHopperJump(slot, state, upsideDown: true, backward: false);
                return;
            case HopperEnemyFunction.Landed:
                LandHopper(slot, state);
                return;
            case HopperEnemyFunction.JumpingUpsideUpBackward:
            case HopperEnemyFunction.JumpingUpsideUpForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level, upsideDown: false);
                return;
            case HopperEnemyFunction.JumpingUpsideDownBackward:
            case HopperEnemyFunction.JumpingUpsideDownForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level, upsideDown: true);
                return;
            case HopperEnemyFunction.WaitToHop:
                if (state.ReadyToHop)
                {
                    state.ReadyToHop = false;
                    state.Function = HopperEnemyFunction.ChooseHopSize;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Hopper function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Rejects movement dispatch before a room's collision plane is available.</summary>
    private static void RequireHopperLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Hopper movement requires room level data.");
    }

    /// <summary>Selects big or small quadratic-speed parameters and routes to the facing choice for the next hop.</summary>
    private static void PrepareHopperHop(HopperEnemyState state, bool bigHop)
    {
        state.YSpeedTableIndexDelta = bigHop ? 4 : 3;
        state.XVelocity = 3;
        state.YSpeedTableIndex = bigHop
            ? state.BigHopInitialYSpeedTableIndex
            : state.SmallHopInitialYSpeedTableIndex;
        state.Function = state.UpsideDown
            ? HopperEnemyFunction.ChooseDirectionUpsideDown
            : HopperEnemyFunction.ChooseDirectionUpsideUp;
    }

    /// <summary>Chooses the floor or ceiling hop branch from Samus's signed horizontal position relative to the actor.</summary>
    private static void ChooseHopperDirection(
        RoomEnemySlot slot,
        HopperEnemyState state,
        SamusState? samus,
        bool upsideDown)
    {
        if (samus is null)
            throw new InvalidOperationException("Hopper direction selection requires Samus.");

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        state.Function = (upsideDown, samusIsLeft) switch
        {
            (false, true) => HopperEnemyFunction.StartBackwardHopUpsideUp,
            (false, false) => HopperEnemyFunction.StartForwardHopUpsideUp,
            (true, true) => HopperEnemyFunction.StartBackwardHopUpsideDown,
            (true, false) => HopperEnemyFunction.StartForwardHopUpsideDown,
        };
    }

    /// <summary>Installs the jumping animation and negates X velocity only for the leftward branch.</summary>
    internal static void StartHopperJump(
        RoomEnemySlot slot,
        HopperEnemyState state,
        bool upsideDown,
        bool backward)
    {
        // “Backward” is the leftward branch selected when Samus is left of the actor. The
        // initial setup always writes +3, so only this branch negates the whole-pixel word.
        if (backward)
            state.XVelocity = unchecked((short)-state.XVelocity);

        SetHopperProgram(slot, state, ReadHopperProgram(
            state, upsideDown, jumping: true));
        state.Function = (upsideDown, backward) switch
        {
            (false, true) => HopperEnemyFunction.JumpingUpsideUpBackward,
            (false, false) => HopperEnemyFunction.JumpingUpsideUpForward,
            (true, true) => HopperEnemyFunction.JumpingUpsideDownBackward,
            (true, false) => HopperEnemyFunction.JumpingUpsideDownForward,
        };
    }

    /// <summary>Installs the landed list and returns the actor to its wait-for-hop function.</summary>
    internal static void LandHopper(RoomEnemySlot slot, HopperEnemyState state)
    {
        SetHopperProgram(slot, state, ReadHopperProgram(
            state, state.UpsideDown, jumping: false));
        state.Function = HopperEnemyFunction.WaitToHop;
    }

    /// <summary>Advances a floor or ceiling hop with native vertical-first collision and phase-dependent horizontal reversal.</summary>
    private void RunHopperMovement(
        RoomEnemySlot slot,
        HopperEnemyState state,
        RoomLevelData level,
        bool upsideDown)
    {
        bool movingAwayFromSurface = !state.Falling;
        // Native has four movement routines: floor actors use the negative half while
        // jumping and the positive half while falling; ceiling actors do the inverse.
        // Inequality expresses that table exactly. Equality reverses both phases, making
        // each actor collide with its starting surface and then migrate to the opposite one.
        bool useNegativeSpeed = upsideDown != movingAwayFromSurface;
        int verticalDisplacement = EnemyMath.ReadQuadraticEnemySpeed(
            checked((ushort)state.YSpeedTableIndex),
            negative: useNegativeSpeed);

        // Every native movement entry resolves Y first. A collision while rising reverses
        // X and changes to the falling half without performing horizontal motion that frame.
        // A collision while falling installs Landed and likewise ends the frame immediately.
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            if (movingAwayFromSurface)
            {
                state.XVelocity = unchecked((short)-state.XVelocity);
                state.Falling = true;
            }
            else
            {
                state.Falling = false;
                state.Function = HopperEnemyFunction.Landed;
            }
            return;
        }

        int horizontalDisplacement = SnesSixteenSixteen.FromWhole(state.XVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            state.XVelocity = unchecked((short)-state.XVelocity);
            if (movingAwayFromSurface)
            {
                // Rising code treats a wall exactly like a ceiling/floor interruption and
                // immediately enters the falling half. Falling code only reverses X.
                state.Falling = true;
                return;
            }
        }

        if (movingAwayFromSurface)
        {
            // The cartridge stores the decremented record and branches on its sign: a record below zero is the apex,
            // replaced by zero as the hop turns back toward the surface.
            int next = state.YSpeedTableIndex - state.YSpeedTableIndexDelta;
            if (next < 0)
            {
                state.YSpeedTableIndex = 0;
                state.Falling = true;
            }
            else
            {
                state.YSpeedTableIndex = next;
            }
            return;
        }

        // The cartridge caps the returning record by the sign of its word difference from the cap; both stay far
        // below the word's sign bit, so that is the ordinary minimum.
        state.YSpeedTableIndex = Math.Min(
            state.YSpeedTableIndex + state.YSpeedTableIndexDelta,
            MaximumHopperYSpeedTableIndex);
    }

    /// <summary>Accumulates the native unaligned quadratic-speed words until the authored jump height is reached.</summary>
    internal static ushort CalculateInitialHopperYSpeedTableIndex(
        ushort jumpHeight,
        ushort tableIndexDelta)
    {
        ushort tableIndex = 0;
        ushort accumulatedHeight = 0;
        for (int iteration = 0; iteration < ushort.MaxValue; iteration++)
        {
            tableIndex = unchecked((ushort)(tableIndex + tableIndexDelta));

            // $A3:ABAF deliberately reads at table+1: the word consists of the subspeed's
            // high byte and speed's low byte. A naturally aligned 16-bit read changes every
            // initial jump index, so retain this odd unaligned cartridge access literally.
            ushort heightIncrement = EnemyQuadraticSpeedDefinitions.ReadWord(tableIndex * 8 + 1);
            accumulatedHeight = unchecked((ushort)(accumulatedHeight + heightIncrement));
            if (unchecked((short)(accumulatedHeight - jumpHeight)) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            "Hopper initial-speed calculation did not reach its ROM jump height.");
    }

    /// <summary>Selects the program for this variant's facing and jumping or landing half.</summary>
    private static HopperProgram ReadHopperProgram(
        HopperEnemyState state,
        bool upsideDown,
        bool jumping) =>
        HopperAnimationDefinitions.Program(state.Variant, upsideDown, jumping);

    /// <summary>Starts a new animation with a one-tick countdown.</summary>
    private static void SetHopperProgram(
        RoomEnemySlot slot,
        HopperEnemyState state,
        HopperProgram program)
    {
        state.Program = AnimationProgramCursor<HopperProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires initialized per-slot hop state before dispatching AI or private animation commands.</summary>
    private static HopperEnemyState RequireHopperState(RoomEnemySlot slot) =>
        slot.SpeciesState<HopperEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Hopper state.");

    /// <summary>Ports Hoppers's initialization AI.</summary>
    internal void RunHoppersInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeHopper(slot);
    }

    /// <summary>Ports Hoppers's main AI.</summary>
    internal void RunHoppersMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunHopperMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Hoppers's typed animation.</summary>
    internal void AdvanceHoppersTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        HopperEnemyState state = RequireHopperState(slot);
        AnimationProgramCursor<HopperProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, HopperAnimations.Programs, ref cursor, HopperFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyHopperCue(slot, state, cue));
        state.Program = cursor;
    }

    /// <summary>Last library-two sound emitted by a Hopper instruction this enemy pass, admitted through the native Max6 queue entry; null means no request.</summary>
    public SoundEffectId? LastHopperSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastHopperSoundEffect = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastHopperSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastHopperSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }
}
