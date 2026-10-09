using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The literal bank-$A8 function pointer stored in a Powamp body's variable F. Keeping the
/// cartridge addresses visible makes a debugger watch directly comparable with WRAM $0FB2.
/// The balloon half uses <see cref="BalloonNoOp"/> and never owns vertical movement.
/// </summary>
public enum PowampEnemyFunction : ushort
{
    /// <summary><c>Function_Powamp_Deflated_Resting</c> at <c>$A8:C283</c>; holds the body at its resting height until its 60-update countdown starts balloon inflation.</summary>
    DeflatedResting = 0xc283,
    /// <summary><c>Function_Powamp_Inflating</c> at <c>$A8:C2A6</c>; waits ten AI updates, then selects fast body animation and upward velocity of half a pixel per update.</summary>
    Inflating = 0xc2a6,
    /// <summary><c>Function_Powamp_Inflated_RiseToTargetHeight</c> at <c>$A8:C2CF</c>; rises with horizontal wiggle toward 64 pixels above the balloon's spawn Y, switching immediately to grappled rise if grapple AI activates.</summary>
    InflatedRiseToTargetHeight = 0xc2cf,
    /// <summary><c>Function_Powamp_Inflated_FinishWiggle</c> at <c>$A8:C36B</c>; completes the wiggle to a centered phase before deflating, continuing terrain-clipped upward movement while off center.</summary>
    InflatedFinishWiggle = 0xc36b,
    /// <summary><c>Function_Powamp_Grappled_RiseToTargetHeight</c> at <c>$A8:C3E1</c>; rises toward the population-defined grapple height while attached, switching to ordinary finish-wiggle without moving on the release update.</summary>
    GrappledRiseToTargetHeight = 0xc3e1,
    /// <summary><c>Function_Powamp_Grappled_FinishWiggle</c> at <c>$A8:C469</c>; centers the wiggle before resting at the grapple target, or hands off to ordinary finish-wiggle when released.</summary>
    GrappledFinishWiggle = 0xc469,
    /// <summary><c>Function_Powamp_Grappled_Resting</c> at <c>$A8:C4DC</c>; holds the raised body while grapple AI remains active and starts deflation on release.</summary>
    GrappledResting = 0xc4dc,
    /// <summary><c>Function_Powamp_Deflating</c> at <c>$A8:C500</c>; waits ten AI updates for balloon deflation, then selects downward velocity of one pixel per update.</summary>
    Deflating = 0xc500,
    /// <summary><c>Function_Powamp_Deflated_Sinking</c> at <c>$A8:C51D</c>; sinks with terrain collision handling until the body reaches the balloon's spawn Y, then restores slow animation and the rest countdown.</summary>
    DeflatedSinking = 0xc51d,
    /// <summary><c>RTL_A8C568</c> at <c>$A8:C568</c>; no-operation main AI for the balloon half, whose position is maintained by its following body.</summary>
    BalloonNoOp = 0xc568,
    /// <summary><c>Function_Powamp_FatalDamage</c> at <c>$A8:C569</c>; adjusts the balloon's inflation animation and initializes the 32-update death delay after a fatal shot.</summary>
    FatalDamage = 0xc569,
    /// <summary><c>Function_Powamp_DeathSequence</c> at <c>$A8:C59F</c>; maintains balloon alignment during the countdown, then deletes both halves and fires spikes in eight directions.</summary>
    DeathSequence = 0xc59f,
}

/// <summary>
/// Typed view of the six common enemy variables reused by both halves of a Powamp. The ROM
/// gives the words different meanings according to population parameter one: a nonzero
/// parameter is the balloon, while zero is the immediately following moving body.
/// </summary>
public sealed class PowampEnemyState
{
    /// <summary>The physical enemy slot whose shared variables are exposed through this typed view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a state view over the slot that stores this Powamp half's variables.</summary>
    /// <param name="slot">The physical slot containing the shared enemy-variable words.</param>
    internal PowampEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Whether population parameter one is nonzero, selecting the balloon interpretation of shared variables; zero selects the immediately following moving body.</summary>
    public bool IsBalloon => _slot.Parameter1 != 0;

    // Body meanings for variables A/B. Together these are a signed 16.16 displacement.
    /// <summary>Body variable A, the signed high word of vertical pixels-per-AI-update displacement; negative values move upward, and this word aliases <see cref="BalloonSpawnX"/> on balloon slots.</summary>
    public ushort YVelocity { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    /// <summary>Body variable B, the low word of vertical displacement in 1/65536-pixel units, combined with <see cref="YVelocity"/> as signed 16.16; aliases <see cref="BalloonSpawnY"/> on balloon slots.</summary>
    public ushort YSubvelocity { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    // Balloon meanings for the same physical words.
    /// <summary>Balloon variable A, the original room-pixel X used as both halves' horizontal wiggle center; aliases body <see cref="YVelocity"/>.</summary>
    public ushort BalloonSpawnX { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    /// <summary>Balloon variable B, the original room-pixel Y used for the body's resting and rise-target calculations; aliases body <see cref="YSubvelocity"/>.</summary>
    public ushort BalloonSpawnY { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    /// <summary>Body variable C, the zero-based index 0-11 into the signed three-pixel wiggle wave; phases zero and six are centered.</summary>
    public ushort WiggleIndex { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Body variable D, an AI-update countdown reset to five for each horizontal wiggle offset; aliases balloon <see cref="BalloonGrappleTravelDistance"/>.</summary>
    public ushort WiggleTimer { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Balloon variable D, copied from population parameter two; room-pixel distance subtracted from the balloon's spawn Y to obtain the grappled body target height.</summary>
    public ushort BalloonGrappleTravelDistance { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Body variable E, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or signed underflow.</summary>
    public ushort FunctionTimer { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F at first-slot WRAM $0FB2, interpreted as the bank-$A8 main-AI function pointer; balloon slots retain the no-op entry.</summary>
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
    /// <summary>Enemy definition pointer for the two-slot Powamp population entry at bank $A8:$C163.</summary>
    internal const ushort PowampDefinition = 0xe8bf;

    /// <summary>Rise distance in room pixels when the body is not grappled.</summary>
    private const ushort PowampUngrappledTravelDistance = 0x0040;
    /// <summary>Rest countdown in AI updates before the balloon begins inflating.</summary>
    private const ushort PowampRestFrames = 0x003c;
    /// <summary>Inflation and deflation transition duration in AI updates.</summary>
    private const ushort PowampTransitionFrames = 0x000a;
    /// <summary>Delay in AI updates between fatal damage and the balloon's deletion.</summary>
    private const ushort PowampDeathDelayFrames = 0x0020;
    /// <summary>AI updates spent at each horizontal offset in the body's wiggle cycle.</summary>
    private const ushort PowampWiggleFramesPerOffset = 0x0005;

    /// <summary>Typed state views indexed by physical enemy slot; balloon and body entries keep their aliased variables distinct.</summary>
    private readonly PowampEnemyState?[] _powampStates =
        new PowampEnemyState?[MaximumEnemyCount];

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
            SetPowampInstruction(slot, PowampInstructionProgramDefinitions.BodySlow);
            return;
        }

        state.BalloonSpawnX = slot.XPosition;
        state.BalloonSpawnY = slot.YPosition;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampInstruction(slot, PowampInstructionProgramDefinitions.BalloonDeflated);
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
                throw new InvalidDataException(
                    $"Powamp body function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Counts down the resting delay, starts inflation at expiry, and keeps the balloon aligned with the body.</summary>
    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            SetPowampInstruction(balloon, PowampInstructionProgramDefinitions.BalloonInflate0);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = PowampTransitionFrames;
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Waits for the balloon's inflation transition, then starts the body's upward movement.</summary>
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
            SetPowampInstruction(body, PowampInstructionProgramDefinitions.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Moves the body toward its ordinary or grapple-defined height while advancing the horizontal wiggle.</summary>
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

    /// <summary>Completes the current horizontal cycle before resting or deflating, preserving grapple release timing.</summary>
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

    /// <summary>Holds the body at grapple height until release, then begins the deflation transition.</summary>
    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if ((body.AiHandlerBits & 1) == 0)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Waits for the balloon to deflate before starting the body's downward return.</summary>
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

    /// <summary>Sinks the body to its recorded spawn height and restores its resting animation and delay.</summary>
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
            SetPowampInstruction(body, PowampInstructionProgramDefinitions.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Chooses the balloon animation compatible with its current instruction cursor and starts the fatal delay.</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        if (cursor >= PowampInstructionProgramDefinitions.BalloonStartSinking)
        {
            ushort byteOffset = unchecked((ushort)(
                cursor - 4 - PowampInstructionProgramDefinitions.BalloonStartSinking));
            byteOffset >>= 1;
            if (byteOffset != 0)
            {
                // The native pointer table is indexed by the byte offset left in Y. Offset
                // two selects inflate stage one and offset four restarts stage zero; entry
                // zero is skipped.
                ushort replacement = byteOffset switch
                {
                    2 => PowampInstructionProgramDefinitions.BalloonInflate1,
                    4 => PowampInstructionProgramDefinitions.BalloonInflate0,
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

    /// <summary>Maintains balloon alignment during the fatal delay, then deletes the balloon and starts the spike burst.</summary>
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

        // `$A8:C5A6-$C5B5`: the balloon is deleted, the spikes fire, and the body goes
        // through EnemyDeath with whatever the spike loop left in A as its animation.
        balloon.Health = 0;
        balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
        ushort accumulator = SpawnPowampSpikeBurst(body);
        StartGenericEnemyDeath(body, accumulator);
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
        body.PaletteIndex = EnemyPaletteBits.Palette5;
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

    /// <summary>Advances one timed horizontal offset and reports when a requested centered phase is reached.</summary>
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
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
        {
            throw new InvalidDataException(
                $"Powamp wiggle index {state.WiggleIndex} exceeds the 12-word ROM table.");
        }

        ushort x = unchecked((ushort)(
            RequirePowampState(balloon).BalloonSpawnX + PowampMotionDefinitions.WiggleOffset(state.WiggleIndex)));
        balloon.XPosition = x;
        body.XPosition = x;
        if (stopWhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex = unchecked((ushort)(state.WiggleIndex + 1));
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
            state.WiggleIndex = 0;
        return false;
    }

    /// <summary>Sets the balloon's Y from the body position and the active inflation or sinking animation frame.</summary>
    private static void AlignPowampBalloonY(RoomEnemySlot body, RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        bool sinking;
        ushort basePointer;
        if (cursor < PowampInstructionProgramDefinitions.BalloonStartSinking)
        {
            sinking = false;
            basePointer = PowampInstructionProgramDefinitions.BalloonInflate0;
        }
        else
        {
            sinking = true;
            basePointer = PowampInstructionProgramDefinitions.BalloonStartSinking;
        }

        // The 65C816 calculation produces a byte offset (0,2,4), not a logical element
        // index. Underflow and any value >=6 deliberately fall back to the first entry.
        ushort byteOffset = unchecked((ushort)(cursor - 4 - basePointer));
        byteOffset >>= 1;
        int index = byteOffset < 6 ? byteOffset / 2 : 0;
        balloon.YPosition = unchecked((ushort)(body.YPosition + PowampMotionDefinitions.BalloonOffset(index, sinking)));
    }

    /// <summary>Selects the deflation function, initializes its countdown, and starts the balloon's sinking animation.</summary>
    private static void StartPowampDeflating(PowampEnemyState state, RoomEnemySlot balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = PowampTransitionFrames;
        SetPowampInstruction(
            balloon,
            PowampInstructionProgramDefinitions.BalloonStartSinking);
    }

    /// <summary>Decrements the function countdown and reports expiry at zero or signed underflow.</summary>
    private static bool TickPowampTimer(PowampEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return state.FunctionTimer == 0 || IsNegative16(state.FunctionTimer);
    }

    /// <summary>Combines the body's signed high word and fractional low word into a 16.16 movement value.</summary>
    private static int ComposePowampVerticalDisplacement(PowampEnemyState state) =>
        unchecked((unchecked((short)state.YVelocity) << 16) | state.YSubvelocity);

    /// <summary>Reports whether the current phase is one of the two zero-offset positions in the wiggle cycle.</summary>
    private static bool IsPowampCentered(ushort wiggleIndex) => wiggleIndex is 0 or 6;

    /// <summary>Selects an instruction list and resets the interpreter counters for that animation.</summary>
    private static void SetPowampInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Requires room geometry for Powamp movement and collision handling.</summary>
    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    /// <summary>Resolves the immediately preceding balloon slot, rejecting a malformed two-slot population.</summary>
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

    /// <summary>Gets the state view created during Powamp initialization or fails for an uninitialized slot.</summary>
    private PowampEnemyState RequirePowampState(RoomEnemySlot slot) =>
        _powampStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Powamp state.");
}
