namespace SuperMetroid.Core.Game;

/// <summary>
/// The role encoded by Magdollite population parameter one. Retail always allocates these
/// records consecutively as head, rising body, then tracking overlay. The names describe
/// ownership; they do not collapse the three physical enemy slots into one host actor.
/// </summary>
public enum MagdollitePart : ushort
{
    Head = 0,
    RisingBody = 1,
    TrackingOverlay = 2,
}

/// <summary>Bank-$A8 function pointers stored in native Magdollite variable F.</summary>
public enum MagdolliteEnemyFunction : ushort
{
    HeadWaiting = 0xb11a,
    HeadWaitingForAttackAnimation = 0xb175,
    HeadWaitingForBodyLanding = 0xb193,
    HeadWaitingForReturnAnimation = 0xb1b8,
    BodyDormant = 0xb1dd,
    BodyRising = 0xb204,
    NoOpAtApex = 0xb291,
    BodyFalling = 0xb295,
    OverlayWaitingForAttack = 0xb30d,
    OverlayWaitingForThrowAnimation = 0xb31f,
    OverlayTrackingRisingBody = 0xb356,
    OverlayTrackingFallingBody = 0xb3a7,
}

/// <summary>
/// Typed projection of Magdollite's common slot words and bank-$7E:7800 extension. The
/// ordinary A-F words remain backed by <see cref="RoomEnemySlot"/> so debugger values match
/// the cartridge. Extension words are named here instead of reproducing native's deliberately
/// obscure drawing-queue/spawn-snapshot aliases between three neighboring records.
/// </summary>
public sealed class MagdolliteEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MagdolliteEnemyState(RoomEnemySlot slot, MagdollitePart part)
    {
        _slot = slot;
        Part = part;
    }

    public MagdollitePart Part { get; }
    public MagdolliteEnemyFunction Function
    {
        get => (MagdolliteEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Native variable B: byte offset into the nine-entry rise tables.</summary>
    public ushort BodyPhaseOffset
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: last instruction list actually installed.</summary>
    public ushort InstalledInstructionList
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: desired instruction list.</summary>
    public ushort DesiredInstructionList
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: signed whole-pixel displacement accumulated by the body.</summary>
    public ushort VerticalWholeDisplacement
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort FacingMarker { get; internal set; }
    public bool AnimationBusy { get; internal set; }
    public bool HeadWaitsForBodyLanding { get; internal set; }
    public ushort OriginY { get; internal set; }

    /// <summary>
    /// Native extension variable four. Only the overlay's instance is polled by the head;
    /// decrementing it in every part remains observable and matches main AI exactly.
    /// </summary>
    public ushort AttackTimer { get; internal set; }

    public ushort PositiveSpeedFraction { get; internal set; }
    public ushort PositiveSpeedWhole { get; internal set; }
    public ushort NegativeSpeedFraction { get; internal set; }
    public ushort NegativeSpeedWhole { get; internal set; }
    public bool BodyReachedApex { get; internal set; }
    public bool BodyWakeRequested { get; internal set; }
    public ushort InitializationOriginX { get; internal set; }
    public ushort ThrowOriginX { get; internal set; }
    public ushort ThrowOriginY { get; internal set; }
}

public sealed partial class RoomEnemySystem
{
    internal const ushort MagdolliteDefinition = 0xe83f;
    internal const ushort MagdollitePowerBombAi = 0xb400;
    internal const ushort MagdolliteTouchAi = 0xb406;
    internal const ushort MagdolliteShotAi = 0xb40c;

    private const ushort MagdolliteLeftIdleList = 0xac9c;
    private const ushort MagdolliteLeftThrowList = 0xacb0;
    private const ushort MagdolliteLeftAttackList = 0xacde;
    private const ushort MagdolliteLeftReturnList = 0xad0c;
    private const ushort MagdolliteRightIdleList = 0xad3c;
    private const ushort MagdolliteRightThrowList = 0xad50;
    private const ushort MagdolliteRightAttackList = 0xad7e;
    private const ushort MagdolliteRightReturnList = 0xadac;
    private const ushort MagdolliteBodyBaseList = 0xaddc;
    private const ushort MagdolliteOverlayIdleList = 0xae0c;

    private const int MagdollitePaletteTable = 0xa8ac1c;
    private const int MagdolliteRiseThresholdTable = 0xa8af55;
    private const int MagdolliteBodyListTable = 0xa8af67;
    private const int MagdolliteVerticalOffsetTable = 0xa8af79;
    private const ushort MagdolliteMaximumRise = 108;

    private readonly MagdolliteEnemyState?[] _magdolliteStates =
        new MagdolliteEnemyState?[MaximumEnemyCount];
    private ushort _magdollitePaletteAnimationTimer;
    private ushort _magdollitePaletteAnimationIndex;
    private ushort _magdollitePaletteBaseByteOffset;
    private bool _magdollitePaletteAnimationInstalled;

    public IReadOnlyList<MagdolliteEnemyState?> MagdolliteStates => _magdolliteStates;

    /// <summary>Last library-two sound emitted by throw opcode $A8:AE12 this frame.</summary>
    public ushort? LastMagdolliteSoundEffect { get; private set; }

    /// <summary>Clears room-lifetime state before a new enemy population is initialized.</summary>
    private void ResetMagdolliteRoomState()
    {
        Array.Clear(_magdolliteStates);
        LastMagdolliteSoundEffect = null;
        LastMagdolliteLavaDropRequest = null;
        _magdollitePaletteAnimationInstalled = false;
        _magdollitePaletteAnimationTimer = 0;
        _magdollitePaletteAnimationIndex = 0;
        _magdollitePaletteBaseByteOffset = 0;
    }

    /// <summary>Clears events whose public meaning is limited to one enemy frame.</summary>
    private void ResetMagdolliteFrameEvents()
    {
        LastMagdolliteSoundEffect = null;
        LastMagdolliteLavaDropRequest = null;
    }

    /// <summary>Ports <c>NorfairLavaMan_Init</c> at $A8:AF8B.</summary>
    private void InitializeMagdollite(RoomEnemySlot slot, SamusState? samus)
    {
        if (slot.Parameter1 > (ushort)MagdollitePart.TrackingOverlay)
        {
            throw new InvalidDataException(
                $"Magdollite slot {slot.SlotIndex} has invalid part parameter ${slot.Parameter1:X4}.");
        }

        var state = new MagdolliteEnemyState(slot, (MagdollitePart)slot.Parameter1);
        _magdolliteStates[slot.SlotIndex] = state;

        switch (state.Part)
        {
            case MagdollitePart.Head:
                InitializeMagdolliteHead(slot, state, samus);
                break;
            case MagdollitePart.RisingBody:
                InitializeMagdolliteBody(slot, state);
                break;
            case MagdollitePart.TrackingOverlay:
                InitializeMagdolliteOverlay(slot, state);
                break;
        }

        // Parameter two's high byte is a native byte offset selector multiplied by eight.
        // The table stores +whole,+fraction,-whole,-fraction. Reading both signed records
        // retains retail's independently encoded negative fraction rather than negating a
        // host 32-bit value and accidentally repairing a possible ROM rounding quirk.
        ushort speedOffset = unchecked((ushort)((slot.Parameter2 >> 8) * 8));
        (short positiveWhole, ushort positiveFraction) = ReadLinearEnemySpeed(speedOffset);
        (short negativeWhole, ushort negativeFraction) = ReadLinearEnemySpeed(
            unchecked((ushort)(speedOffset + 4)));
        state.PositiveSpeedWhole = unchecked((ushort)positiveWhole);
        state.PositiveSpeedFraction = positiveFraction;
        state.NegativeSpeedWhole = unchecked((ushort)negativeWhole);
        state.NegativeSpeedFraction = negativeFraction;

        // Every one of the three initializers installs the same global graphics-drawn hook.
        // Retail's last Magdollite record therefore resets this clock to eight and selects
        // its OBJ palette. Model the resulting singleton hook, not nine independent cycles.
        _magdollitePaletteAnimationInstalled = true;
        _magdollitePaletteBaseByteOffset = unchecked((ushort)(
            ((slot.PaletteIndex & 0x0e00) >> 4) + 0x0100));
        _magdollitePaletteAnimationTimer = 8;
        _magdollitePaletteAnimationIndex = 0;
    }

    private static void InitializeMagdolliteHead(
        RoomEnemySlot slot,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        state.InstalledInstructionList = 0;
        state.FacingMarker = 0;
        state.HeadWaitsForBodyLanding = false;
        state.OriginY = slot.YPosition;

        // The initializer's marker convention is briefly opposite main function $B11A's
        // convention. Preserve that one-frame oddity; the first main tick normalizes it.
        bool samusIsLeft = samus is not null &&
            IsNegative16(unchecked((ushort)(samus.XPosition - slot.XPosition)));
        if (samusIsLeft)
            state.FacingMarker = 1;
        state.DesiredInstructionList = state.FacingMarker != 0
            ? MagdolliteLeftIdleList
            : MagdolliteRightIdleList;
        InstallMagdolliteInstructionList(slot, state);
        state.Function = MagdolliteEnemyFunction.HeadWaiting;
    }

    private static void InitializeMagdolliteBody(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        state.OriginY = slot.YPosition;
        state.InstalledInstructionList = 0;
        state.BodyReachedApex = false;
        state.BodyWakeRequested = true;
        state.DesiredInstructionList = MagdolliteBodyBaseList;
        InstallMagdolliteInstructionList(slot, state);
        slot.YPosition = unchecked((ushort)(slot.YPosition + 32));
        state.Function = MagdolliteEnemyFunction.BodyDormant;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    private static void InitializeMagdolliteOverlay(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        state.OriginY = slot.YPosition;
        state.InitializationOriginX = slot.XPosition;
        state.InstalledInstructionList = 0;
        state.AttackTimer = 0;
        state.DesiredInstructionList = MagdolliteOverlayIdleList;
        InstallMagdolliteInstructionList(slot, state);
        slot.YPosition = unchecked((ushort)(slot.YPosition + 32));
        state.Function = MagdolliteEnemyFunction.OverlayWaitingForAttack;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>NorfairLavaMan_Main</c> at $A8:B10A and its indirect functions.</summary>
    private void RunMagdolliteMain(
        RoomEnemySlot slot,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        state.AttackTimer = unchecked((ushort)(state.AttackTimer - 1));
        switch (state.Function)
        {
            case MagdolliteEnemyFunction.HeadWaiting:
                RunMagdolliteHeadWaiting(slot, state, samus);
                return;
            case MagdolliteEnemyFunction.HeadWaitingForAttackAnimation:
                RunMagdolliteHeadWaitingForAttackAnimation(slot, state);
                return;
            case MagdolliteEnemyFunction.HeadWaitingForBodyLanding:
                RunMagdolliteHeadWaitingForBodyLanding(slot, state, samus);
                return;
            case MagdolliteEnemyFunction.HeadWaitingForReturnAnimation:
                RunMagdolliteHeadWaitingForReturnAnimation(slot, state, samus);
                return;
            case MagdolliteEnemyFunction.BodyDormant:
                RunMagdolliteBodyDormant(slot, state);
                return;
            case MagdolliteEnemyFunction.BodyRising:
                RunMagdolliteBodyRising(slot, state, samus);
                return;
            case MagdolliteEnemyFunction.NoOpAtApex:
                return;
            case MagdolliteEnemyFunction.BodyFalling:
                RunMagdolliteBodyFalling(slot, state);
                return;
            case MagdolliteEnemyFunction.OverlayWaitingForAttack:
                RunMagdolliteOverlayWaiting(slot, state);
                return;
            case MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation:
                RunMagdolliteOverlayAnimation(slot, state);
                return;
            case MagdolliteEnemyFunction.OverlayTrackingRisingBody:
                RunMagdolliteOverlayTrackingRise(slot, state, samus);
                return;
            case MagdolliteEnemyFunction.OverlayTrackingFallingBody:
                RunMagdolliteOverlayTrackingFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunMagdolliteHeadWaiting(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            return;

        bool samusIsRight = !IsNegative16(unchecked((ushort)(samus.XPosition - head.XPosition)));
        state.FacingMarker = samusIsRight ? (ushort)1 : (ushort)0;
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteRightIdleList
            : MagdolliteLeftIdleList;
        InstallMagdolliteInstructionList(head, state);

        (_, _, RoomEnemySlot overlay) = RequireMagdolliteComposite(head);
        MagdolliteEnemyState overlayState = RequireMagdolliteState(overlay);
        if (!IsNegative16(overlayState.AttackTimer))
            return;

        overlayState.AttackTimer = 0;
        ushort range = unchecked((byte)head.Parameter2);
        if (!IsWithinStrictModularDistance(head.XPosition, samus.XPosition, range))
            return;

        state.DesiredInstructionList = samusIsRight
            ? MagdolliteRightAttackList
            : MagdolliteLeftAttackList;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaitingForAttackAnimation;
    }

    private void RunMagdolliteHeadWaitingForAttackAnimation(
        RoomEnemySlot head,
        MagdolliteEnemyState state)
    {
        if (state.AnimationBusy)
            return;

        (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(head);
        state.HeadWaitsForBodyLanding = true;
        RequireMagdolliteState(body).BodyWakeRequested = false;
        state.Function = MagdolliteEnemyFunction.HeadWaitingForBodyLanding;
    }

    private static void RunMagdolliteHeadWaitingForBodyLanding(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (state.HeadWaitsForBodyLanding)
            return;

        bool samusIsRight = samus is not null &&
            !IsNegative16(unchecked((ushort)(samus.XPosition - head.XPosition)));
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteRightReturnList
            : MagdolliteLeftReturnList;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaitingForReturnAnimation;
    }

    private static void RunMagdolliteHeadWaitingForReturnAnimation(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (state.AnimationBusy)
            return;

        bool samusIsRight = samus is not null &&
            !IsNegative16(unchecked((ushort)(samus.XPosition - head.XPosition)));
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteRightIdleList
            : MagdolliteLeftIdleList;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaiting;
    }

    private static void RunMagdolliteBodyDormant(
        RoomEnemySlot body,
        MagdolliteEnemyState state)
    {
        if (state.BodyWakeRequested)
            return;

        state.Function = MagdolliteEnemyFunction.BodyRising;
        state.VerticalWholeDisplacement = 0;
        state.BodyWakeRequested = false;
        state.BodyPhaseOffset = 2;
        body.YPosition = state.OriginY;
    }

    private void RunMagdolliteBodyRising(
        RoomEnemySlot body,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        int wholeDelta = AddMagdolliteY(
            body,
            state.NegativeSpeedWhole,
            state.NegativeSpeedFraction);
        state.VerticalWholeDisplacement = unchecked((ushort)(
            state.VerticalWholeDisplacement + wholeDelta));

        ushort upwardDistance = unchecked((ushort)-state.VerticalWholeDisplacement);
        int phaseIndex = state.BodyPhaseOffset >> 1;
        ushort bodyToOverlayOffset = ReadWord(
            _bus!,
            MagdolliteVerticalOffsetTable + phaseIndex * 2);
        bool roseMaximum = !IsNegative16(upwardDistance - MagdolliteMaximumRise);
        bool rosePastSamus = samus is not null && IsNegative16(unchecked((ushort)(
            body.YPosition - bodyToOverlayOffset - samus.YPosition)));
        ushort threshold = ReadWord(_bus!, MagdolliteRiseThresholdTable + phaseIndex * 2);

        if (roseMaximum || rosePastSamus)
        {
            state.Function = MagdolliteEnemyFunction.NoOpAtApex;
            state.BodyReachedApex = true;
            if (IsNegative16(upwardDistance - threshold))
                return;
        }
        else if (IsNegative16(upwardDistance - threshold))
        {
            return;
        }

        state.BodyPhaseOffset = unchecked((ushort)(state.BodyPhaseOffset + 2));
        body.YPosition = unchecked((ushort)(body.YPosition + 8));
        state.DesiredInstructionList = ReadWord(
            _bus!,
            MagdolliteBodyListTable + (state.BodyPhaseOffset >> 1) * 2);
        InstallMagdolliteInstructionList(body, state);
    }

    private void RunMagdolliteBodyFalling(
        RoomEnemySlot body,
        MagdolliteEnemyState state)
    {
        int wholeDelta = AddMagdolliteY(
            body,
            state.PositiveSpeedWhole,
            state.PositiveSpeedFraction);
        state.VerticalWholeDisplacement = unchecked((ushort)(
            state.VerticalWholeDisplacement + wholeDelta));

        if (!IsNegative16(state.VerticalWholeDisplacement))
        {
            state.BodyWakeRequested = true;
            (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
            RequireMagdolliteState(head).HeadWaitsForBodyLanding = false;
            state.Function = MagdolliteEnemyFunction.BodyDormant;
            return;
        }

        ushort downwardDistanceRemaining = unchecked((ushort)-state.VerticalWholeDisplacement);
        int priorPhaseIndex = unchecked((ushort)(state.BodyPhaseOffset - 2)) >> 1;
        ushort threshold = ReadWord(_bus!, MagdolliteRiseThresholdTable + priorPhaseIndex * 2);
        if (!IsNegative16(downwardDistanceRemaining - threshold))
            return;

        state.BodyPhaseOffset = unchecked((ushort)(state.BodyPhaseOffset - 2));
        body.YPosition = unchecked((ushort)(body.YPosition - 8));
        state.DesiredInstructionList = ReadWord(
            _bus!,
            MagdolliteBodyListTable + (state.BodyPhaseOffset >> 1) * 2);
        InstallMagdolliteInstructionList(body, state);
    }

    private void RunMagdolliteOverlayWaiting(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state)
    {
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(overlay);
        if (RequireMagdolliteState(head).Function ==
            MagdolliteEnemyFunction.HeadWaitingForAttackAnimation)
        {
            state.Function = MagdolliteEnemyFunction.OverlayTrackingRisingBody;
        }
    }

    private void RunMagdolliteOverlayAnimation(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state)
    {
        if (!state.AnimationBusy)
        {
            (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
            MagdolliteEnemyState bodyState = RequireMagdolliteState(body);
            state.DesiredInstructionList = MagdolliteOverlayIdleList;
            InstallMagdolliteInstructionList(overlay, state);
            bodyState.Function = MagdolliteEnemyFunction.BodyFalling;
            state.Function = MagdolliteEnemyFunction.OverlayTrackingFallingBody;
            bodyState.BodyReachedApex = false;
            overlay.XPosition = state.ThrowOriginX;
            overlay.YPosition = state.ThrowOriginY;
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    private void RunMagdolliteOverlayTrackingRise(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteEnemyState bodyState = RequireMagdolliteState(body);
        if (bodyState.BodyReachedApex)
        {
            bool samusIsRight = samus is not null &&
                !IsNegative16(unchecked((ushort)(samus.XPosition - overlay.XPosition)));
            state.DesiredInstructionList = samusIsRight
                ? MagdolliteRightThrowList
                : MagdolliteLeftThrowList;
            state.BodyPhaseOffset = samusIsRight ? (ushort)1 : (ushort)0;
            InstallMagdolliteInstructionList(overlay, state);
            state.Function = MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation;
            state.ThrowOriginX = overlay.XPosition;
            state.ThrowOriginY = overlay.YPosition;
        }
        else
        {
            overlay.YPosition = unchecked((ushort)(body.YPosition - ReadWord(
                _bus!,
                MagdolliteVerticalOffsetTable + (bodyState.BodyPhaseOffset >> 1) * 2)));
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    private void RunMagdolliteOverlayTrackingFall(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state)
    {
        (RoomEnemySlot head, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteEnemyState headState = RequireMagdolliteState(head);
        MagdolliteEnemyState bodyState = RequireMagdolliteState(body);
        if (headState.Function == MagdolliteEnemyFunction.HeadWaiting)
        {
            state.Function = MagdolliteEnemyFunction.OverlayWaitingForAttack;
        }
        else
        {
            overlay.YPosition = unchecked((ushort)(body.YPosition - ReadWord(
                _bus!,
                MagdolliteVerticalOffsetTable + (bodyState.BodyPhaseOffset >> 1) * 2)));
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>
    /// Ports $A8:B3CB's drawing-queue alias. For the third record, source +61 is the head's
    /// Y word and destination +64 is its Y radius. Expressing that relationship directly
    /// preserves collision geometry without exposing unrelated queue storage to C#.
    /// </summary>
    private void UpdateMagdolliteHeadRadius(RoomEnemySlot overlay)
    {
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(overlay);
        ushort radius = unchecked((ushort)(head.YPosition - overlay.YPosition + 2));
        head.YRadius = IsNegative16(radius - 8) ? (ushort)8 : radius;
    }

    private static int AddMagdolliteY(
        RoomEnemySlot slot,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        uint fractionalSum = (uint)slot.YSubposition + fractionalVelocity;
        slot.YSubposition = unchecked((ushort)fractionalSum);
        int wholeDelta = unchecked((short)wholeVelocity) +
            (fractionalSum > ushort.MaxValue ? 1 : 0);
        slot.YPosition = unchecked((ushort)(slot.YPosition + wholeDelta));
        return wholeDelta;
    }

    private static void InstallMagdolliteInstructionList(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        if (state.DesiredInstructionList == state.InstalledInstructionList)
            return;
        slot.CurrentInstruction = state.DesiredInstructionList;
        state.InstalledInstructionList = state.DesiredInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private MagdolliteEnemyState RequireMagdolliteState(RoomEnemySlot slot) =>
        _magdolliteStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Magdollite slot {slot.SlotIndex} has no typed state.");

    private (RoomEnemySlot Head, RoomEnemySlot Body, RoomEnemySlot Overlay)
        RequireMagdolliteComposite(RoomEnemySlot member)
    {
        int headIndex = member.SlotIndex - checked((int)member.Parameter1);
        if (headIndex < 0 || headIndex + 2 >= MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {member.SlotIndex} has no complete composite.");

        RoomEnemySlot head = _slots[headIndex];
        RoomEnemySlot body = _slots[headIndex + 1];
        RoomEnemySlot overlay = _slots[headIndex + 2];
        if (head.EnemyDefinitionPointer != MagdolliteDefinition || head.Parameter1 != 0 ||
            body.EnemyDefinitionPointer != MagdolliteDefinition || body.Parameter1 != 1 ||
            overlay.EnemyDefinitionPointer != MagdolliteDefinition || overlay.Parameter1 != 2)
        {
            throw new InvalidDataException(
                $"Magdollite slot {member.SlotIndex} is not inside a retail-ordered 0/1/2 composite.");
        }
        return (head, body, overlay);
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:B0B2.</summary>
    private void StepMagdollitePaletteAnimation()
    {
        if (!_magdollitePaletteAnimationInstalled)
            return;

        _magdollitePaletteAnimationTimer = unchecked((ushort)(
            _magdollitePaletteAnimationTimer - 1));
        if (_magdollitePaletteAnimationTimer != 0)
            return;

        _magdollitePaletteAnimationTimer = 8;
        _magdollitePaletteAnimationIndex = unchecked((ushort)(
            _magdollitePaletteAnimationIndex + 1));
        int paletteFrame = _magdollitePaletteAnimationIndex & 3;
        int source = MagdollitePaletteTable + paletteFrame * 32 + 9 * 2;
        int destination = (_magdollitePaletteBaseByteOffset >> 1) + 9;
        for (int color = 0; color < 4; color++)
            _cgram!.SetColor(destination + color, ReadWord(_bus!, source + color * 2));
    }

    /// <summary>
    /// Ports all sixteen private bank-$A8 instruction callbacks referenced by the ten
    /// retail lists. The cursor arithmetic follows the callback return value: AE12 consumes
    /// one operand; every other command consumes only its function word.
    /// </summary>
    private bool TryProcessMagdolliteInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor,
        ushort cameraX,
        ushort cameraY)
    {
        if (slot.EnemyDefinitionPointer != MagdolliteDefinition)
            return false;

        MagdolliteEnemyState state = RequireMagdolliteState(slot);
        switch (opcode)
        {
            case MagdolliteInstructionCodes.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen:
            {
                bool offScreen = IsNegative16(slot.XPosition - cameraX) ||
                    IsNegative16(cameraX + 256 - slot.XPosition) ||
                    IsNegative16(slot.YPosition - cameraY) ||
                    IsNegative16(cameraY + 256 - slot.YPosition);
                if (!offScreen)
                {
                    LastMagdolliteSoundEffect = ReadWord(
                        _bus!,
                        0xa80000 | unchecked((ushort)(cursor + 2)));
                }
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels:
                slot.YPosition = unchecked((ushort)(slot.YPosition + 2));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels:
                slot.YPosition = unchecked((ushort)(slot.YPosition - 2));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag:
                state.AnimationBusy = true;
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag:
                state.AnimationBusy = false;
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                next.YPosition = unchecked((ushort)(next.YPosition - 1));
                break;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.YPosition = unchecked((ushort)(slot.YPosition + 1));
                next.YPosition = unchecked((ushort)(next.YPosition + 1));
                break;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = unchecked((ushort)(state.OriginY + 24));
                slot.YPosition = y;
                body.YPosition = y;
                body.Properties = body.Properties.Without(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.Without(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_RestoreInitialYPositions:
            {
                (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(slot);
                slot.YPosition = state.OriginY;
                body.YPosition = state.OriginY;
                break;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = unchecked((ushort)(state.OriginY + 4));
                slot.YPosition = y;
                body.YPosition = y;
                body.Properties = body.Properties.With(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.With(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile:
                SpawnMagdolliteLava(slot, state.BodyPhaseOffset);
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight:
                slot.XPosition = unchecked((ushort)(state.ThrowOriginX + 8));
                slot.YPosition = unchecked((ushort)(state.ThrowOriginY - 4));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft:
                slot.XPosition = unchecked((ushort)(state.ThrowOriginX - 8));
                slot.YPosition = unchecked((ushort)(state.ThrowOriginY - 4));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup:
                slot.XPosition = unchecked((ushort)(state.ThrowOriginX + 8));
                slot.YPosition = unchecked((ushort)(state.ThrowOriginY - 8));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup:
                slot.XPosition = unchecked((ushort)(state.ThrowOriginX - 8));
                slot.YPosition = unchecked((ushort)(state.ThrowOriginY - 4));
                break;
            case MagdolliteInstructionCodes.Instruction_Magdollite_SetCooldownTimerTo100:
                state.AttackTimer = 256;
                break;
            default:
                return false;
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private RoomEnemySlot NextMagdolliteSlot(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} has no following component.");
        return _slots[slot.SlotIndex + 1];
    }

    /// <summary>Ports the shared post-combat tail at $A8:B410.</summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemySlot current)
    {
        // The native callback literally addresses current+$40 and current+$80. Ordinary
        // touch/shot reaches only the collision-enabled head, but power bombs may expose
        // this retail cross-record quirk, so do not silently normalize current to a group.
        RoomEnemySlot? following = current.SlotIndex + 1 < MaximumEnemyCount
            ? _slots[current.SlotIndex + 1]
            : null;
        RoomEnemySlot? secondFollowing = current.SlotIndex + 2 < MaximumEnemyCount
            ? _slots[current.SlotIndex + 2]
            : null;

        if (current.Health == 0)
        {
            if (following is not null)
                following.Properties = following.Properties.With(EnemyProperties.Deleted);
            if (secondFollowing is not null)
                secondFollowing.Properties = secondFollowing.Properties.With(EnemyProperties.Deleted);
        }

        if (current.FrozenTimer == 0)
            return;
        if (following is not null)
        {
            following.FrozenTimer = current.FrozenTimer;
            following.AiHandlerBits = unchecked((ushort)(following.AiHandlerBits | 4));
        }
        if (secondFollowing is not null)
        {
            secondFollowing.FrozenTimer = current.FrozenTimer;
            secondFollowing.AiHandlerBits = unchecked((ushort)(secondFollowing.AiHandlerBits | 4));
        }
    }
}
