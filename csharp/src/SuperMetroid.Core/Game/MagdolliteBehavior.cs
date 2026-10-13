using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Magdollite species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MagdolliteBehavior : IEnemySpecies
{
    /// <summary>Supplies the room artwork used by the shared palette hook.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the lava shots emitted by the overlay's instruction list.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Resolves the three adjacent records that form one Magdollite.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Provides the live CGRAM target for the palette cycle.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Runs common contact damage before the species reaction.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates the item drop when a thrown lava projectile is destroyed.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects the Magdollite AI, projectile, combat and palette services.</summary>
    internal MagdolliteBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyPresentation presentation, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _presentation = presentation;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Typed state indexed by the three physical enemy records of each composite.</summary>
    internal readonly MagdolliteEnemyState?[] _magdolliteStates =
        new MagdolliteEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Eight-frame countdown for the singleton graphics-drawn palette hook.</summary>
    internal NativeCountdown _magdollitePaletteAnimationTimer;
    /// <summary>Frame cursor masked into the four-frame palette cycle.</summary>
    internal ushort _magdollitePaletteAnimationIndex;
    /// <summary>OBJ palette byte offset selected by the final initialized Magdollite record.</summary>
    internal ushort _magdollitePaletteBaseByteOffset;
    /// <summary>Whether any Magdollite initializer installed the room's palette hook.</summary>
    internal bool _magdollitePaletteAnimationInstalled;

    /// <summary>Last library-two sound emitted by throw opcode $A8:AE12 this frame.</summary>
    public ushort? LastMagdolliteSoundEffect { get; private set; }

    /// <summary>Clears room-lifetime state before a new enemy population is initialized.</summary>
    internal void ResetMagdolliteRoomState()
    {
        Array.Clear(_magdolliteStates);
        LastMagdolliteSoundEffect = null;
        LastMagdolliteLavaDropRequest = null;
        _magdollitePaletteAnimationInstalled = false;
        _magdollitePaletteAnimationTimer = NativeCountdown.Start(0);
        _magdollitePaletteAnimationIndex = 0;
        _magdollitePaletteBaseByteOffset = 0;
    }

    /// <summary>Clears events whose public meaning is limited to one enemy frame.</summary>
    internal void ResetMagdolliteFrameEvents()
    {
        LastMagdolliteSoundEffect = null;
        LastMagdolliteLavaDropRequest = null;
    }

    /// <summary>Ports <c>NorfairLavaMan_Init</c> at $A8:AF8B.</summary>
    internal void InitializeMagdollite(RoomEnemySlot slot, SamusState? samus)
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
        ushort speedOffset = unchecked((ushort)((SnesLittleEndian.HighByte(slot.Parameter2)) * 8));
        (short positiveWhole, ushort positiveFraction) = EnemyMath.ReadLinearEnemySpeed(speedOffset);
        (short negativeWhole, ushort negativeFraction) = EnemyMath.ReadLinearEnemySpeed(
            unchecked((ushort)(speedOffset + 4)));
        state.PositiveSpeedWhole = unchecked((ushort)positiveWhole);
        state.PositiveSpeedFraction = positiveFraction;
        state.NegativeSpeedWhole = unchecked((ushort)negativeWhole);
        state.NegativeSpeedFraction = negativeFraction;

        // Every one of the three initializers installs the same global graphics-drawn hook.
        // Retail's last Magdollite record therefore resets this clock to eight and selects
        // its OBJ palette. Model the resulting singleton hook, not nine independent cycles.
        _magdollitePaletteAnimationInstalled = true;
        int paletteNumber = new SuperMetroid.Core.Hardware.SnesObjAttributeWord(
            slot.PaletteIndex).PaletteIndex;
        _magdollitePaletteBaseByteOffset = unchecked((ushort)(
            paletteNumber * 0x20 + 0x0100));
        _magdollitePaletteAnimationTimer = NativeCountdown.Start(8);
        _magdollitePaletteAnimationIndex = 0;
    }

    /// <summary>Starts the head in its Samus-facing idle list, retaining the initializer's reversed marker convention until the first tick.</summary>
    private static void InitializeMagdolliteHead(
        RoomEnemySlot slot,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        state.InstalledInstructionList = 0;
        state.FacingMarker = 0;
        state.HeadWaitsForBodyLanding = false;
        state.OriginY = slot.Position.Y.Whole;

        // The initializer's marker convention is briefly opposite main function $B11A's
        // convention. Preserve that one-frame oddity; the first main tick normalizes it.
        bool samusIsLeft = samus is not null &&
            EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole)));
        if (samusIsLeft)
            state.FacingMarker = 1;
        state.DesiredInstructionList = state.FacingMarker != 0
            ? MagdolliteInstructionProgramDefinitions.LeftIdle
            : MagdolliteInstructionProgramDefinitions.RightIdle;
        InstallMagdolliteInstructionList(slot, state);
        state.Function = MagdolliteEnemyFunction.HeadWaiting;
    }

    /// <summary>Hides the rising body 32 pixels below its origin until the head triggers an attack.</summary>
    private static void InitializeMagdolliteBody(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        state.OriginY = slot.Position.Y.Whole;
        state.InstalledInstructionList = 0;
        state.BodyReachedApex = false;
        state.BodyWakeRequested = true;
        state.DesiredInstructionList = MagdollitePhaseDefinitions.Phase(0).BodyInstructionList;
        InstallMagdolliteInstructionList(slot, state);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 32)));
        state.Function = MagdolliteEnemyFunction.BodyDormant;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Hides the projectile-throwing overlay below its origin and starts its pillar-cap list.</summary>
    private static void InitializeMagdolliteOverlay(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        state.OriginY = slot.Position.Y.Whole;
        state.InitializationOriginX = slot.Position.X.Whole;
        state.InstalledInstructionList = 0;
        state.AttackTimer = NativeCountdown.Start(0);
        state.DesiredInstructionList = MagdolliteInstructionProgramDefinitions.PillarCap;
        InstallMagdolliteInstructionList(slot, state);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 32)));
        state.Function = MagdolliteEnemyFunction.OverlayWaitingForAttack;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>NorfairLavaMan_Main</c> at $A8:B10A and its indirect functions.</summary>
    private void RunMagdolliteMain(
        RoomEnemySlot slot,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        state.AttackTimer = state.AttackTimer.Decrement().Remaining;
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

    /// <summary>Tracks Samus while idle and begins submerging when the overlay cooldown expires within the head's horizontal range.</summary>
    private void RunMagdolliteHeadWaiting(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            return;

        bool samusIsRight = !EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - head.Position.X.Whole)));
        state.FacingMarker = samusIsRight ? (ushort)1 : (ushort)0;
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteInstructionProgramDefinitions.RightIdle
            : MagdolliteInstructionProgramDefinitions.LeftIdle;
        InstallMagdolliteInstructionList(head, state);

        (_, _, RoomEnemySlot overlay) = RequireMagdolliteComposite(head);
        MagdolliteEnemyState overlayState = RequireMagdolliteState(overlay);
        if (!EnemyMath.IsNegative16(overlayState.AttackTimer.RawValue))
            return;

        overlayState.AttackTimer = NativeCountdown.Start(0);
        ushort range = unchecked((byte)head.Parameter2);
        if (!EnemyMath.IsWithinStrictModularDistance(head.Position.X.Whole, samus.Position.X.Whole, range))
            return;

        state.DesiredInstructionList = samusIsRight
            ? MagdolliteInstructionProgramDefinitions.RightSubmerge
            : MagdolliteInstructionProgramDefinitions.LeftSubmerge;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaitingForAttackAnimation;
    }

    /// <summary>Wakes the adjacent body only after the head's submerge animation has cleared its busy flag.</summary>
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

    /// <summary>Starts the Samus-facing emerge list once the falling body releases the head's landing latch.</summary>
    private static void RunMagdolliteHeadWaitingForBodyLanding(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (state.HeadWaitsForBodyLanding)
            return;

        bool samusIsRight = samus is not null &&
            !EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - head.Position.X.Whole)));
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteInstructionProgramDefinitions.RightEmerge
            : MagdolliteInstructionProgramDefinitions.LeftEmerge;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaitingForReturnAnimation;
    }

    /// <summary>Restores the appropriate idle list after the emerge animation finishes.</summary>
    private static void RunMagdolliteHeadWaitingForReturnAnimation(
        RoomEnemySlot head,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        if (state.AnimationBusy)
            return;

        bool samusIsRight = samus is not null &&
            !EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - head.Position.X.Whole)));
        state.DesiredInstructionList = samusIsRight
            ? MagdolliteInstructionProgramDefinitions.RightIdle
            : MagdolliteInstructionProgramDefinitions.LeftIdle;
        InstallMagdolliteInstructionList(head, state);
        state.Function = MagdolliteEnemyFunction.HeadWaiting;
    }

    /// <summary>Returns the hidden body to its origin and enters rising once the head clears its wake latch.</summary>
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
        body.Position = body.Position.WithWholeY(state.OriginY);
    }

    /// <summary>Rises using the preceding head's velocity, advances growth phases at their native thresholds, and stops at the apex or above Samus.</summary>
    internal void RunMagdolliteBodyRising(
        RoomEnemySlot body,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        // The native arm reads velocity at X-$40: the preceding head owns the
        // population speed parameter. The arm's own parameter is zero in retail.
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
        MagdolliteEnemyState speedOwner = RequireMagdolliteState(head);
        int wholeDelta = AddMagdolliteY(
            body,
            speedOwner.NegativeSpeedWhole,
            speedOwner.NegativeSpeedFraction);
        state.VerticalWholeDisplacement = unchecked((ushort)(
            state.VerticalWholeDisplacement + wholeDelta));

        ushort upwardDistance = unchecked((ushort)-state.VerticalWholeDisplacement);
        int phaseIndex = state.BodyPhaseOffset >> 1;
        MagdollitePhaseDefinition phase = MagdollitePhaseDefinitions.Phase(phaseIndex);
        bool roseMaximum = !EnemyMath.IsNegative16(
            upwardDistance - MagdollitePhaseDefinitions.MaximumRise);
        bool rosePastSamus = samus is not null && EnemyMath.IsNegative16(unchecked((ushort)(
            body.Position.Y.Whole - phase.OverlayYOffset - samus.Position.Y.Whole)));
        // The maximum-rise branch skips the growth-index load, so its threshold is the
        // overread at the main loop's enemy index; the Samus test loads the phase's own.
        ushort threshold = roseMaximum
            ? MagdollitePhaseDefinitions.ApexThreshold(body.SlotIndex)
            : phase.DistanceThreshold;

        if (roseMaximum || rosePastSamus)
        {
            state.Function = MagdolliteEnemyFunction.NoOpAtApex;
            state.BodyReachedApex = true;
            if (EnemyMath.IsNegative16(upwardDistance - threshold))
                return;
        }
        else if (EnemyMath.IsNegative16(upwardDistance - threshold))
        {
            return;
        }

        state.BodyPhaseOffset = unchecked((ushort)(state.BodyPhaseOffset + 2));
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 8)));
        state.DesiredInstructionList = MagdollitePhaseDefinitions
            .Phase(state.BodyPhaseOffset >> 1).BodyInstructionList;
        InstallMagdolliteInstructionList(body, state);
    }

    /// <summary>Retraces the body's growth phases and releases the head's landing latch at its origin.</summary>
    internal void RunMagdolliteBodyFalling(
        RoomEnemySlot body,
        MagdolliteEnemyState state)
    {
        (RoomEnemySlot speedHead, _, _) = RequireMagdolliteComposite(body);
        MagdolliteEnemyState speedOwner = RequireMagdolliteState(speedHead);
        int wholeDelta = AddMagdolliteY(
            body,
            speedOwner.PositiveSpeedWhole,
            speedOwner.PositiveSpeedFraction);
        state.VerticalWholeDisplacement = unchecked((ushort)(
            state.VerticalWholeDisplacement + wholeDelta));

        if (!EnemyMath.IsNegative16(state.VerticalWholeDisplacement))
        {
            state.BodyWakeRequested = true;
            (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
            RequireMagdolliteState(head).HeadWaitsForBodyLanding = false;
            state.Function = MagdolliteEnemyFunction.BodyDormant;
            return;
        }

        ushort downwardDistanceRemaining = unchecked((ushort)-state.VerticalWholeDisplacement);
        int priorPhaseIndex = unchecked((ushort)(state.BodyPhaseOffset - 2)) >> 1;
        ushort threshold = MagdollitePhaseDefinitions.Phase(priorPhaseIndex).DistanceThreshold;
        if (!EnemyMath.IsNegative16(downwardDistanceRemaining - threshold))
            return;

        state.BodyPhaseOffset = unchecked((ushort)(state.BodyPhaseOffset - 2));
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 8)));
        state.DesiredInstructionList = MagdollitePhaseDefinitions
            .Phase(state.BodyPhaseOffset >> 1).BodyInstructionList;
        InstallMagdolliteInstructionList(body, state);
    }

    /// <summary>Begins following the rising body when the head enters its attack animation.</summary>
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

    /// <summary>Holds the head radius while throwing, then restores the overlay and starts the body's descent.</summary>
    private void RunMagdolliteOverlayAnimation(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state)
    {
        if (!state.AnimationBusy)
        {
            (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
            MagdolliteEnemyState bodyState = RequireMagdolliteState(body);
            state.DesiredInstructionList = MagdolliteInstructionProgramDefinitions.PillarCap;
            InstallMagdolliteInstructionList(overlay, state);
            bodyState.Function = MagdolliteEnemyFunction.BodyFalling;
            state.Function = MagdolliteEnemyFunction.OverlayTrackingFallingBody;
            bodyState.BodyReachedApex = false;
            overlay.Position = overlay.Position.WithWhole(state.ThrowOrigin);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Positions the overlay over each rising body phase, then starts its Samus-facing throw at the apex.</summary>
    internal void RunMagdolliteOverlayTrackingRise(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteEnemyState bodyState = RequireMagdolliteState(body);
        if (bodyState.BodyReachedApex)
        {
            bool samusIsRight = samus is not null &&
                !EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - overlay.Position.X.Whole)));
            state.DesiredInstructionList = samusIsRight
                ? MagdolliteInstructionProgramDefinitions.RightThrow
                : MagdolliteInstructionProgramDefinitions.LeftThrow;
            state.BodyPhaseOffset = samusIsRight ? (ushort)1 : (ushort)0;
            InstallMagdolliteInstructionList(overlay, state);
            state.Function = MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation;
            state.ThrowOrigin = overlay.Position.Whole;
        }
        else
        {
            ushort offset = MagdollitePhaseDefinitions
                .Phase(bodyState.BodyPhaseOffset >> 1).OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - offset)));
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Follows the descending body until the head returns to idle, while maintaining its collision radius.</summary>
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
            ushort offset = MagdollitePhaseDefinitions
                .Phase(bodyState.BodyPhaseOffset >> 1).OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - offset)));
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
        ushort radius = unchecked((ushort)(head.Position.Y.Whole - overlay.Position.Y.Whole + 2));
        head.Radius = head.Radius.WithY(EnemyMath.IsNegative16(radius - 8) ? (ushort)8 : radius);
    }

    /// <summary>Adds signed whole and fractional Y velocity with the native carry and returns the resulting whole-pixel delta.</summary>
    private static int AddMagdolliteY(
        RoomEnemySlot slot,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        uint fractionalSum = (uint)slot.Position.Y.Fraction + fractionalVelocity;
        slot.Position = slot.Position.WithFractionY(unchecked((ushort)fractionalSum));
        int wholeDelta = unchecked((short)wholeVelocity) +
            (fractionalSum > ushort.MaxValue ? 1 : 0);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + wholeDelta)));
        return wholeDelta;
    }

    /// <summary>Restarts instruction timing only when the desired animation list differs from the installed list.</summary>
    private static void InstallMagdolliteInstructionList(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        if (state.DesiredInstructionList == state.InstalledInstructionList)
            return;
        slot.CurrentInstruction = state.DesiredInstructionList;
        state.InstalledInstructionList = state.DesiredInstructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects a physical enemy slot that has no Magdollite state for the current room.</summary>
    private MagdolliteEnemyState RequireMagdolliteState(RoomEnemySlot slot) =>
        _magdolliteStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Magdollite slot {slot.SlotIndex} has no typed state.");

    /// <summary>Finds and validates the retail-ordered head, body and overlay around any one component.</summary>
    private (RoomEnemySlot Head, RoomEnemySlot Body, RoomEnemySlot Overlay)
        RequireMagdolliteComposite(RoomEnemySlot member)
    {
        int headIndex = member.SlotIndex - checked((int)member.Parameter1);
        if (headIndex < 0 || headIndex + 2 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {member.SlotIndex} has no complete composite.");

        RoomEnemySlot head = _slotTable._slots[headIndex];
        RoomEnemySlot body = _slotTable._slots[headIndex + 1];
        RoomEnemySlot overlay = _slotTable._slots[headIndex + 2];
        if (head.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || head.Parameter1 != 0 ||
            body.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || body.Parameter1 != 1 ||
            overlay.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || overlay.Parameter1 != 2)
        {
            throw new InvalidDataException(
                $"Magdollite slot {member.SlotIndex} is not inside a retail-ordered 0/1/2 composite.");
        }
        return (head, body, overlay);
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:B0B2.</summary>
    internal void StepMagdollitePaletteAnimation()
    {
        if (!_magdollitePaletteAnimationInstalled)
            return;

        _magdollitePaletteAnimationTimer = _magdollitePaletteAnimationTimer.Decrement().Remaining;
        if (!_magdollitePaletteAnimationTimer.IsZero)
            return;

        _magdollitePaletteAnimationTimer = NativeCountdown.Start(8);
        _magdollitePaletteAnimationIndex = unchecked((ushort)(
            _magdollitePaletteAnimationIndex + 1));
        int paletteFrame = _magdollitePaletteAnimationIndex &
            (MagdollitePaletteRomData.FrameCount - 1);
        int destination = (_magdollitePaletteBaseByteOffset >> 1) +
            MagdollitePaletteRomData.FirstAnimatedColor;
        (_room.Artwork?.MagdollitePaletteCycle ?? throw new InvalidDataException(
            "Magdollite palette animation requires installed artwork."))
            .ApplyFrame(_presentation._cgram!, paletteFrame, destination);
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
        Position16<RoomPixels> camera)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite)
            return false;

        MagdolliteEnemyState state = RequireMagdolliteState(slot);
        if (!Enum.IsDefined((MagdolliteInstruction)opcode))
            return false;
        switch ((MagdolliteInstruction)opcode)
        {
            case MagdolliteInstruction.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen:
            {
                bool offScreen = EnemyMath.IsNegative16(slot.Position.X.Whole - camera.X) ||
                    EnemyMath.IsNegative16(camera.X + 256 - slot.Position.X.Whole) ||
                    EnemyMath.IsNegative16(slot.Position.Y.Whole - camera.Y) ||
                    EnemyMath.IsNegative16(camera.Y + 256 - slot.Position.Y.Whole);
                if (!offScreen)
                {
                    LastMagdolliteSoundEffect = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                }
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MagdolliteInstruction.Instruction_Magdollite_MoveDown2Pixels:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 2)));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_MoveUp2Pixels:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 2)));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_SetWaitingFlag:
                state.AnimationBusy = true;
                break;
            case MagdolliteInstruction.Instruction_Magdollite_ResetWaitingFlag:
                state.AnimationBusy = false;
                break;
            case MagdolliteInstruction.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 1)));
                next.Position = next.Position.WithWholeY(unchecked((ushort)(next.Position.Y.Whole - 1)));
                break;
            }
            case MagdolliteInstruction.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
                next.Position = next.Position.WithWholeY(unchecked((ushort)(next.Position.Y.Whole + 1)));
                break;
            }
            case MagdolliteInstruction.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = unchecked((ushort)(state.OriginY + 24));
                slot.Position = slot.Position.WithWholeY(y);
                body.Position = body.Position.WithWholeY(y);
                body.Properties = body.Properties.Without(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.Without(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteInstruction.Instruction_Magdollite_RestoreInitialYPositions:
            {
                (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(slot);
                slot.Position = slot.Position.WithWholeY(state.OriginY);
                body.Position = body.Position.WithWholeY(state.OriginY);
                break;
            }
            case MagdolliteInstruction.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = unchecked((ushort)(state.OriginY + 4));
                slot.Position = slot.Position.WithWholeY(y);
                body.Position = body.Position.WithWholeY(y);
                body.Properties = body.Properties.With(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.With(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteInstruction.Instruction_Magdollite_SpawnLavaProjectile:
                SpawnMagdolliteLava(slot, state.BodyPhaseOffset);
                break;
            case MagdolliteInstruction.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -4));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -8));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteInstruction.Instruction_Magdollite_SetCooldownTimerTo100:
                state.AttackTimer = NativeCountdown.Start(256);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteInstruction)} {opcode:X4}.");
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Resolves the next physical record used by paired base-and-pillar animation opcodes.</summary>
    private RoomEnemySlot NextMagdolliteSlot(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} has no following component.");
        return _slotTable._slots[slot.SlotIndex + 1];
    }

    /// <summary>Ports the shared post-combat tail at $A8:B410.</summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemySlot current)
    {
        // The native callback literally addresses current+$40 and current+$80. Ordinary
        // touch/shot reaches only the collision-enabled head, but power bombs may expose
        // this retail cross-record quirk, so do not silently normalize current to a group.
        RoomEnemySlot? following = current.SlotIndex + 1 < RoomEnemySystem.MaximumEnemyCount
            ? _slotTable._slots[current.SlotIndex + 1]
            : null;
        RoomEnemySlot? secondFollowing = current.SlotIndex + 2 < RoomEnemySystem.MaximumEnemyCount
            ? _slotTable._slots[current.SlotIndex + 2]
            : null;

        if (current.Health == 0)
        {
            if (following is not null)
                following.Properties = following.Properties.With(EnemyProperties.Deleted);
            if (secondFollowing is not null)
                secondFollowing.Properties = secondFollowing.Properties.With(EnemyProperties.Deleted);
        }

        if (current.FrozenTimer.IsZero)
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

    /// <summary>Attaches the species tail after common bomb damage when the shot AI selects Magdollite's reaction.</summary>
    internal void RegisterMagdolliteBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.MagdolliteShot)
        {
            AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects thrown-lava movement and its shot-list drop instruction to the projectile interpreter.</summary>
    internal void RegisterMagdolliteProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunMagdolliteLavaPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MagdolliteLava);

        table.RegisterInstruction(EnemyProjectileInstruction.MagdolliteFlame_SpawnDrops, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                RequestMagdolliteLavaDrop(projectile);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }

    /// <summary>Binds the bank-$A8 initialization address to the three-part species initializer.</summary>
    internal void RegisterMagdolliteInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Magdollite, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMagdollite(slot, samus);
        });
    }

    /// <summary>Binds the bank-$A8 main address to the per-component function dispatch.</summary>
    internal void RegisterMagdolliteMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Magdollite, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite, (slot, frame, nmiFrameCounter8) =>
        {
            RunMagdolliteMain(slot, RequireMagdolliteState(slot), frame.Samus);
        });
    }

    /// <summary>Dispatches Magdollite's private animation opcodes before the generic enemy interpreter.</summary>
    internal void RegisterMagdolliteOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessMagdolliteInstruction(slot, word, ref cursor, context.Camera);
        });
    }

    /// <summary>Propagates common power-bomb death and freezing effects to following composite records.</summary>
    internal void RegisterMagdollitePowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.MagdollitePowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveMagdolliteCombatAfterCommon(enemy),
        });
    }

    /// <summary>Propagates common shot death and freezing effects to following composite records.</summary>
    internal void RegisterMagdolliteShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MagdolliteShot)
        {
            AfterCommon = (c, _) => ResolveMagdolliteCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Applies normal Samus contact damage before mirroring the enemy's death or freeze state.</summary>
    internal void RegisterMagdolliteTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MagdolliteTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveMagdolliteCombatAfterCommon(context.Slot);
            });
    }
    /// <summary>Positive 8.8 speed used for rightward thrown lava.</summary>
    private const ushort MagdolliteLavaHorizontalSpeed = 0x0300;
    /// <summary>Negative 8.8 value that the leftward routine reads from the projectile's Y velocity field.</summary>
    private const ushort MagdolliteLavaUpwardSpeed = 0xfd00;

    /// <summary>Last drop request produced by a destroyed lava projectile this frame.</summary>
    public MagdolliteLavaDropRequest? LastMagdolliteLavaDropRequest { get; private set; }

    /// <summary>Ports <c>EprojInit_NorfairLavaMan</c> at $86:E000.</summary>
    internal void SpawnMagdolliteLava(RoomEnemySlot source, ushort directionParameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.LavaThrownByMagdollite,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));

        // SpawnEprojInner has already cleared all generic scratch words. The initializer
        // copies both subpixels from the throwing overlay, starts two pixels below it, and
        // stores the one-bit facing parameter exactly as supplied by the enemy bytecode.
        projectile.DirectionParameter = directionParameter;
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(unchecked((ushort)(source.Position.Y.Whole + 2)), source.Position.Y.Fraction));
        projectile.Velocity = new(new SnesSignedEightEight(MagdolliteLavaHorizontalSpeed), new SnesSignedEightEight(MagdolliteLavaUpwardSpeed));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.MagdolliteLava;
        projectile.InstructionPointer = directionParameter == 0
            ? MagdolliteLavaInstructionProgramDefinitions.Left
            : MagdolliteLavaInstructionProgramDefinitions.Right;
    }

    /// <summary>Ports $86:E049-$E09B, including the cartridge's asymmetric velocity use.</summary>
    internal static void RunMagdolliteLavaPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        // The left routine at $E050 deliberately adds the Y-velocity word to X. The right
        // routine at $E07A uses X velocity. They begin as -$0300 and +$0300 respectively,
        // so expressing this oddity literally keeps debugger state aligned with the ROM.
        ushort horizontalVelocity = projectile.DirectionParameter == 0
            ? projectile.Velocity.Y.RawValue
            : projectile.Velocity.X.RawValue;
        projectile.Position = projectile.Position.AdvanceX(new SnesSignedEightEight(horizontalVelocity).ToSixteenSixteenDelta());

        // $86:E049 deletes actors outside the inclusive 256x256 camera square after their
        // horizontal move. No vertical movement occurs despite the field's directional use.
        if (unchecked((ushort)(projectile.Position.X.Whole - camera.X)) >= 256 ||
            unchecked((ushort)(projectile.Position.Y.Whole - camera.Y)) >= 256)
        {
            projectile.Clear();
        }
    }

    /// <summary>Ports shot-list instruction $86:DFEA.</summary>
    internal void RequestMagdolliteLavaDrop(RoomEnemyProjectileSlot projectile)
    {
        LastMagdolliteLavaDropRequest = new MagdolliteLavaDropRequest();
        _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
            projectile.Position.Whole,
            EnemyDefinitionId.Magdollite);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetMagdolliteRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        ResetMagdolliteFrameEvents();
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastMagdolliteSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
