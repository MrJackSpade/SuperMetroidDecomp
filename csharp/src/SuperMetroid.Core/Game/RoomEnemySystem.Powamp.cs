using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The literal bank-$A8 function pointer stored in a Powamp body's variable F. Keeping the
/// cartridge addresses visible makes a debugger watch directly comparable with WRAM $0FB4.
/// The balloon half uses <see cref="BalloonNoOp"/> and never owns vertical movement.
/// </summary>
public enum PowampEnemyFunction : ushort
{
    DeflatedResting = 0xc283,
    Inflating = 0xc2a6,
    InflatedRiseToTargetHeight = 0xc2cf,
    InflatedFinishWiggle = 0xc36b,
    GrappledRiseToTargetHeight = 0xc3e1,
    GrappledFinishWiggle = 0xc469,
    GrappledResting = 0xc4dc,
    Deflating = 0xc500,
    DeflatedSinking = 0xc51d,
    BalloonNoOp = 0xc568,
    FatalDamage = 0xc569,
    DeathSequence = 0xc59f,
}

/// <summary>
/// Typed view of the six common enemy variables reused by both halves of a Powamp. The ROM
/// gives the words different meanings according to population parameter one: a nonzero
/// parameter is the balloon, while zero is the immediately following moving body.
/// </summary>
public sealed class PowampEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal PowampEnemyState(RoomEnemySlot slot) => _slot = slot;

    public bool IsBalloon => _slot.Parameter1 != 0;

    // Body meanings for variables A/B. Together these are a signed 16.16 displacement.
    public ushort YVelocity { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    public ushort YSubvelocity { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    // Balloon meanings for the same physical words.
    public ushort BalloonSpawnX { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    public ushort BalloonSpawnY { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    public ushort WiggleIndex { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    public ushort WiggleTimer { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    public ushort BalloonGrappleTravelDistance { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    public ushort FunctionTimer { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    public PowampEnemyFunction Function
    {
        get => (PowampEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Literal translation of the two-slot Powamp enemy $E8BF at $A8:C163-$C6B2. Population
/// order is part of its ABI: every balloon is followed immediately by the body that moves
/// both halves, aligns their animation-dependent Y offset, and owns combat reactions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort PowampDefinition = 0xe8bf;

    private const ushort PowampBodyFastInstruction = 0xc163;
    private const ushort PowampBodySlowInstruction = 0xc173;
    private const ushort PowampBalloonInflateInstruction = 0xc183;
    private const ushort PowampBalloonStartSinkingInstruction = 0xc191;
    private const ushort PowampBalloonDeflatedInstruction = 0xc199;
    private const ushort PowampUngrappledTravelDistance = 0x0040;
    private const ushort PowampRestFrames = 0x003c;
    private const ushort PowampTransitionFrames = 0x000a;
    private const ushort PowampDeathDelayFrames = 0x0020;
    private const ushort PowampWiggleFramesPerOffset = 0x0005;

    // These are the signed ROM words at $A8:C1A1. Index six is centered just like index
    // zero, which is why both values are legal state-transition boundaries.
    private static readonly short[] PowampWiggleOffsets =
        [0, 1, 2, 3, 2, 1, 0, -1, -2, -3, -2, -1];
    private static readonly short[] PowampRisingBalloonYOffsets = [-12, -16, -20];
    private static readonly short[] PowampSinkingBalloonYOffsets = [-20, -16, -12];

    private readonly PowampEnemyState?[] _powampStates =
        new PowampEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Powamp state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<PowampEnemyState?> PowampStates => _powampStates;

    /// <summary>Ports <c>InitAI_Powamp</c> at $A8:C1C9.</summary>
    private void InitializePowamp(RoomEnemySlot slot)
    {
        var state = new PowampEnemyState(slot);
        _powampStates[slot.SlotIndex] = state;

        // The initializer forces animation processing for both halves and clears the
        // instruction interpreter's loop counter before choosing the half-specific list.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        if (!state.IsBalloon)
        {
            // Enemy[-1] is not a descriptive relationship in the original—it is a literal
            // $40-byte negative index. Reject malformed populations instead of accidentally
            // binding a body to an unrelated actor.
            RequirePowampBalloon(slot);
            state.FunctionTimer = PowampRestFrames;
            state.Function = PowampEnemyFunction.DeflatedResting;
            SetPowampInstruction(slot, PowampBodySlowInstruction);
            return;
        }

        state.BalloonSpawnX = slot.XPosition;
        state.BalloonSpawnY = slot.YPosition;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampInstruction(slot, PowampBalloonDeflatedInstruction);
        state.BalloonGrappleTravelDistance = slot.Parameter2;
    }

    /// <summary>Ports <c>MainAI_Powamp</c> and every body function it dispatches.</summary>
    private void RunPowampMain(RoomEnemySlot slot, PowampEnemyState state, RoomLevelData? level)
    {
        if (state.IsBalloon)
            return;

        RoomEnemySlot balloon = RequirePowampBalloon(slot);
        switch (state.Function)
        {
            case PowampEnemyFunction.DeflatedResting:
                RunPowampDeflatedResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Inflating:
                RunPowampInflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.InflatedRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), grappled: false);
                return;
            case PowampEnemyFunction.InflatedFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), grappled: false);
                return;
            case PowampEnemyFunction.GrappledRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), grappled: true);
                return;
            case PowampEnemyFunction.GrappledFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), grappled: true);
                return;
            case PowampEnemyFunction.GrappledResting:
                RunPowampGrappledResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Deflating:
                RunPowampDeflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeflatedSinking:
                RunPowampSinking(slot, state, balloon, RequirePowampLevel(level));
                return;
            case PowampEnemyFunction.FatalDamage:
                BeginPowampDeathSequence(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeathSequence:
                RunPowampDeathSequence(slot, state, balloon);
                return;
            default:
                throw new NotSupportedException(
                    $"Powamp body function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            SetPowampInstruction(balloon, PowampBalloonInflateInstruction);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = PowampTransitionFrames;
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampInflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.InflatedRiseToTargetHeight;
            // $FFFF:$8000 is exactly -0.5 pixels/frame on the NTSC cartridge.
            state.YVelocity = 0xffff;
            state.YSubvelocity = 0x8000;
            SetPowampInstruction(body, PowampBodyFastInstruction);
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampRise(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level,
        bool grappled)
    {
        // The ordinary rise changes dispatch and executes the grappled routine immediately
        // on the frame common grapple AI publishes bit one.
        bool grappleAiActive = (body.AiHandlerBits & 1) != 0;
        if (!grappled && grappleAiActive)
        {
            state.Function = PowampEnemyFunction.GrappledRiseToTargetHeight;
            RunPowampRise(body, state, balloon, level, grappled: true);
            return;
        }

        // Releasing a grapple during either grappled rising state consumes the frame after
        // changing the function. In particular it does not realign the balloon on this path.
        if (grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        AdvancePowampWiggle(body, state, balloon, stopWhenCentered: false);
        bool collided = MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        int travelDistance = grappled
            ? RequirePowampState(balloon).BalloonGrappleTravelDistance
            : PowampUngrappledTravelDistance;
        ushort targetY = unchecked((ushort)(RequirePowampState(balloon).BalloonSpawnY - travelDistance));

        // CMP target,body followed by BMI means "continue while body is still below the
        // target". A room collision completes the rise regardless of its Y coordinate.
        bool reachedTarget = !IsNegative16(targetY - body.YPosition);
        if (collided || reachedTarget)
        {
            if (IsPowampCentered(state.WiggleIndex))
            {
                if (grappled)
                    state.Function = PowampEnemyFunction.GrappledResting;
                else
                    StartPowampDeflating(state, balloon);
            }
            else
            {
                state.Function = grappled
                    ? PowampEnemyFunction.GrappledFinishWiggle
                    : PowampEnemyFunction.InflatedFinishWiggle;
            }
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampFinishWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level,
        bool grappled)
    {
        bool grappleAiActive = (body.AiHandlerBits & 1) != 0;
        if (grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        bool centered = AdvancePowampWiggle(body, state, balloon, stopWhenCentered: true);
        if (centered)
        {
            if (grappled)
                state.Function = PowampEnemyFunction.GrappledResting;
            else
                StartPowampDeflating(state, balloon);
        }
        else
        {
            // Native finish-wiggle ignores collision carry; terrain clips the displacement
            // but does not select another function until the horizontal wiggle is centered.
            MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if ((body.AiHandlerBits & 1) == 0)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampDeflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.DeflatedSinking;
            // $0001:$0000 is exactly +1 pixel/frame.
            state.YVelocity = 1;
            state.YSubvelocity = 0;
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampSinking(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level)
    {
        MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        ushort spawnY = RequirePowampState(balloon).BalloonSpawnY;
        if (!IsNegative16(body.YPosition - spawnY))
        {
            body.YPosition = spawnY;
            state.Function = PowampEnemyFunction.DeflatedResting;
            state.FunctionTimer = PowampRestFrames;
            SetPowampInstruction(body, PowampBodySlowInstruction);
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        if (cursor >= PowampBalloonStartSinkingInstruction)
        {
            ushort byteOffset = unchecked((ushort)(cursor - 4 - PowampBalloonStartSinkingInstruction));
            byteOffset >>= 1;
            if (byteOffset != 0)
            {
                // $C599 is a pointer table indexed by the byte offset left in Y. Offset
                // two selects $C187 and offset four selects $C183; entry zero is skipped.
                ushort replacement = byteOffset switch
                {
                    2 => 0xc187,
                    4 => 0xc183,
                    _ => throw new InvalidDataException(
                        $"Powamp death saw unsupported balloon cursor $A8:{cursor:X4}."),
                };
                SetPowampInstruction(balloon, replacement);
            }
        }

        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = PowampDeathDelayFrames;
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (!TickPowampTimer(state))
        {
            AlignPowampBalloonY(body, balloon);
            return;
        }

        balloon.Health = 0;
        balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
        SpawnPowampSpikeBurst(body);
        if (!body.Properties.HasAny(EnemyProperties.Deleted))
        {
            body.Health = 0;
            body.Properties = body.Properties.With(EnemyProperties.Deleted);
            EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
        }
    }

    /// <summary>Ports the body-only private tail of <c>EnemyTouch_Powamp</c>.</summary>
    private void ResolvePowampTouch(RoomEnemySlot body, SamusState samus, ushort controllerInput)
    {
        ushort originalPalette = body.PaletteIndex;
        ResolveNormalEnemyTouch(body, samus, controllerInput);
        if (body.Health != 0)
            return;

        RoomEnemySlot balloon = RequirePowampBalloon(body);
        balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
        body.PaletteIndex = originalPalette;
        SpawnPowampSpikeBurst(body);
        body.PaletteIndex = 0x0a00;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private void ResolvePowampShotAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot balloon = RequirePowampBalloon(body);
        if ((body.AiHandlerBits & 4) != 0)
        {
            balloon.FrozenTimer = body.FrozenTimer;
            balloon.AiHandlerBits = unchecked((ushort)(balloon.AiHandlerBits | 4));
        }
        if ((body.AiHandlerBits & 2) != 0)
        {
            balloon.FlashTimer = body.FlashTimer;
            balloon.AiHandlerBits = unchecked((ushort)(balloon.AiHandlerBits | 2));
        }
        if (body.Health == 0)
        {
            RequirePowampState(body).Function = PowampEnemyFunction.FatalDamage;
            // init1 becomes the native one-word guard that rejects further touch/shot AI.
            body.Parameter2 = 1;
        }
    }

    /// <summary>Ports the paired state propagation after <c>PowerBombReaction_Powamp</c>.</summary>
    private void ResolvePowampPowerBombAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot balloon = RequirePowampBalloon(body);
        if (body.Health == 0)
        {
            balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
            return;
        }

        balloon.ShakeTimer = body.ShakeTimer;
        balloon.InvincibilityTimer = body.InvincibilityTimer;
        balloon.FlashTimer = body.FlashTimer;
        balloon.FrozenTimer = body.FrozenTimer;
        balloon.AiHandlerBits = body.AiHandlerBits;
    }

    private bool AdvancePowampWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        bool stopWhenCentered)
    {
        state.WiggleTimer = unchecked((ushort)(state.WiggleTimer - 1));
        if (state.WiggleTimer != 0 && !IsNegative16(state.WiggleTimer))
            return false;

        state.WiggleTimer = PowampWiggleFramesPerOffset;
        if (state.WiggleIndex >= PowampWiggleOffsets.Length)
        {
            throw new InvalidDataException(
                $"Powamp wiggle index {state.WiggleIndex} exceeds the 12-word ROM table.");
        }

        ushort x = unchecked((ushort)(
            RequirePowampState(balloon).BalloonSpawnX + PowampWiggleOffsets[state.WiggleIndex]));
        balloon.XPosition = x;
        body.XPosition = x;
        if (stopWhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex = unchecked((ushort)(state.WiggleIndex + 1));
        if (state.WiggleIndex >= PowampWiggleOffsets.Length)
            state.WiggleIndex = 0;
        return false;
    }

    private static void AlignPowampBalloonY(RoomEnemySlot body, RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        short[] offsets;
        ushort basePointer;
        if (cursor < PowampBalloonStartSinkingInstruction)
        {
            offsets = PowampRisingBalloonYOffsets;
            basePointer = PowampBalloonInflateInstruction;
        }
        else
        {
            offsets = PowampSinkingBalloonYOffsets;
            basePointer = PowampBalloonStartSinkingInstruction;
        }

        // The 65C816 calculation produces a byte offset (0,2,4), not a logical element
        // index. Underflow and any value >=6 deliberately fall back to the first entry.
        ushort byteOffset = unchecked((ushort)(cursor - 4 - basePointer));
        byteOffset >>= 1;
        int index = byteOffset < 6 ? byteOffset / 2 : 0;
        balloon.YPosition = unchecked((ushort)(body.YPosition + offsets[index]));
    }

    private static void StartPowampDeflating(PowampEnemyState state, RoomEnemySlot balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = PowampTransitionFrames;
        SetPowampInstruction(balloon, PowampBalloonStartSinkingInstruction);
    }

    private static bool TickPowampTimer(PowampEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return state.FunctionTimer == 0 || IsNegative16(state.FunctionTimer);
    }

    private static int ComposePowampVerticalDisplacement(PowampEnemyState state) =>
        unchecked((unchecked((short)state.YVelocity) << 16) | state.YSubvelocity);

    private static bool IsPowampCentered(ushort wiggleIndex) => wiggleIndex is 0 or 6;

    private static void SetPowampInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    private RoomEnemySlot RequirePowampBalloon(RoomEnemySlot body)
    {
        if (body.SlotIndex == 0)
            throw new InvalidDataException("A Powamp body has no preceding balloon slot.");
        RoomEnemySlot balloon = _slots[body.SlotIndex - 1];
        if (balloon.EnemyDefinitionPointer != PowampDefinition || balloon.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Powamp body slot {body.SlotIndex} is not preceded by its balloon half.");
        }
        return balloon;
    }

    private PowampEnemyState RequirePowampState(RoomEnemySlot slot) =>
        _powampStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Powamp state.");
}
