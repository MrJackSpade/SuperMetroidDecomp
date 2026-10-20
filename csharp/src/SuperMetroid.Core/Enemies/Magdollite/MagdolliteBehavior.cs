using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The Magdollite species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MagdolliteBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Magdollite];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MagdolliteEntity(this, slot, common);

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

    /// <summary>Eight-frame countdown for the singleton graphics-drawn palette hook.</summary>
    internal NativeCountdown MagdollitePaletteAnimationTimer;
    /// <summary>The palette cycle's current row, 0 through <see cref="MagdollitePaletteRomData.FrameCount"/> minus one; each step advances it around the cycle.</summary>
    internal int MagdollitePaletteFrame;
    /// <summary>OBJ palette byte offset selected by the final initialized Magdollite record.</summary>
    internal int MagdollitePaletteBaseColor;
    /// <summary>Whether any Magdollite initializer installed the room's palette hook.</summary>
    internal bool MagdollitePaletteAnimationInstalled;

    /// <summary>Last library-two sound emitted by throw opcode $A8:AE12 this frame.</summary>
    public SoundEffectId? LastMagdolliteSoundEffect { get; private set; }

    /// <summary>Clears events whose public meaning is limited to one enemy frame.</summary>
    internal void ResetMagdolliteFrameEvents()
    {
        LastMagdolliteSoundEffect = null;
    }

    /// <summary>Ports <c>NorfairLavaMan_Init</c> at $A8:AF8B.</summary>
    internal void InitializeMagdollite(RoomEnemySlot slot, SamusState? samus)
    {
        if (slot.Spawn.Population.Parameter1 > (ushort)MagdollitePart.TrackingOverlay)
        {
            throw new InvalidDataException(
                $"Magdollite slot {slot.SlotIndex} has invalid part parameter ${slot.Spawn.Population.Parameter1:X4}.");
        }

        var state = new MagdolliteEnemyState((MagdollitePart)slot.Spawn.Population.Parameter1);
        slot.SetSpeciesState(state, state.ClearWithRecord);

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
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdollitePart)} {state.Part}.");
        }

        // Parameter two's high byte is a native byte offset selector multiplied by eight.
        // The table stores +whole,+fraction,-whole,-fraction. Reading both signed records
        // retains retail's independently encoded negative fraction rather than negating a
        // host 32-bit value and accidentally repairing a possible ROM rounding quirk.
        int speedOffset = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2) * EnemyLinearSpeedDefinitions.RecordSize;
        SnesSignedSixteenSixteen positive = EnemyMath.ReadLinearEnemySpeed(checked((ushort)speedOffset));
        SnesSignedSixteenSixteen negative = EnemyMath.ReadLinearEnemySpeed(checked((ushort)(speedOffset + 4)));
        state.DownwardVelocity = positive;
        state.UpwardVelocity = negative;

        // Every one of the three initializers installs the same global graphics-drawn hook.
        // Retail's last Magdollite record therefore resets this clock to eight and selects
        // its OBJ palette. Model the resulting singleton hook, not nine independent cycles.
        MagdollitePaletteAnimationInstalled = true;
        int paletteNumber = new SuperMetroid.Core.Hardware.SnesObjAttributeWord(
            checked((ushort)slot.PaletteIndex)).PaletteIndex;
        MagdollitePaletteBaseColor =
            (paletteNumber * MagdollitePaletteRomData.PaletteBytes + MagdollitePaletteRomData.SpritePaletteByteBase) / sizeof(ushort);
        MagdollitePaletteAnimationTimer = NativeCountdown.Start(8);
        MagdollitePaletteFrame = 0;
    }

    /// <summary>Starts the head in its Samus-facing idle list, retaining the initializer's reversed marker convention until the first tick.</summary>
    private static void InitializeMagdolliteHead(
        RoomEnemySlot slot,
        MagdolliteEnemyState state,
        SamusState? samus)
    {
        state.InstalledProgram = null;
        state.HeadWaitsForBodyLanding = false;
        state.Origin = slot.Position.Whole;

        bool samusIsLeft = samus is not null && (samus.Position.Whole - slot.Position.Whole).X < 0;
        state.DesiredProgram = samusIsLeft
            ? MagdolliteProgram.LeftIdle
            : MagdolliteProgram.RightIdle;
        InstallMagdolliteInstructionList(slot, state);
        state.Function = MagdolliteEnemyFunction.HeadWaiting;
    }

    /// <summary>Hides the rising body 32 pixels below its origin until the head triggers an attack.</summary>
    private static void InitializeMagdolliteBody(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        state.Origin = slot.Position.Whole;
        state.InstalledProgram = null;
        state.BodyReachedApex = false;
        state.BodyWakeRequested = true;
        state.DesiredProgram = MagdollitePhaseDefinitions.Phase(0).BodyProgram;
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
        state.Origin = slot.Position.Whole;
        state.InstalledProgram = null;
        state.AttackTimer = NativeCountdown.Start(0);
        state.DesiredProgram = MagdolliteProgram.PillarCap;
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
                    $"Magdollite function {state.Function?.ToString() ?? "none"} is not translated.");
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
        state.DesiredProgram = samusIsRight
            ? MagdolliteProgram.RightIdle
            : MagdolliteProgram.LeftIdle;
        InstallMagdolliteInstructionList(head, state);

        (_, _, RoomEnemySlot overlay) = RequireMagdolliteComposite(head);
        MagdolliteEnemyState overlayState = overlay.RequireSpeciesState<MagdolliteEnemyState>();
        if (!EnemyMath.IsNegative16(overlayState.AttackTimer.RawValue))
            return;

        overlayState.AttackTimer = NativeCountdown.Start(0);
        ushort range = unchecked((byte)head.Spawn.Population.Parameter2);
        if (!EnemyMath.IsWithinStrictModularDistance(head.Position.X.Whole, samus.Position.X.Whole, range))
            return;

        state.DesiredProgram = samusIsRight
            ? MagdolliteProgram.RightSubmerge
            : MagdolliteProgram.LeftSubmerge;
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
        body.RequireSpeciesState<MagdolliteEnemyState>().BodyWakeRequested = false;
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
        state.DesiredProgram = samusIsRight
            ? MagdolliteProgram.RightEmerge
            : MagdolliteProgram.LeftEmerge;
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
        state.DesiredProgram = samusIsRight
            ? MagdolliteProgram.RightIdle
            : MagdolliteProgram.LeftIdle;
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
        state.RiseDisplacement = Offset16<RoomPixels>.Zero;
        state.BodyWakeRequested = false;
        state.BodyPhase = 1;
        body.Position = body.Position.WithWholeY(state.Origin.Y);
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
        MagdolliteEnemyState speedOwner = head.RequireSpeciesState<MagdolliteEnemyState>();
        state.RiseDisplacement += AddMagdolliteY(body, speedOwner.UpwardVelocity);

        ushort upwardDistance = unchecked((ushort)-state.RiseDisplacement.Y);
        MagdollitePhaseDefinition phase = MagdollitePhaseDefinitions.Phase(state.BodyPhase);
        bool roseMaximum = !EnemyMath.IsNegative16(
            upwardDistance - MagdollitePhaseDefinitions.MaximumRise);
        bool rosePastSamus = samus is not null && EnemyMath.IsNegative16(unchecked((ushort)(
            body.Position.Y.Whole - phase.OverlayYOffset - samus.Position.Y.Whole)));
        // The maximum-rise branch skips the growth-index load, so its comparison depends on the
        // main loop's enemy index; the Samus test compares with the phase's own threshold.
        bool growsAPhase = roseMaximum
            ? MagdollitePhaseDefinitions.ApexGrowsFinalPhase(body.SlotIndex)
            : !EnemyMath.IsNegative16(upwardDistance - phase.DistanceThreshold);

        if (roseMaximum || rosePastSamus)
        {
            state.Function = MagdolliteEnemyFunction.NoOpAtApex;
            state.BodyReachedApex = true;
            if (!growsAPhase)
                return;
        }
        else if (!growsAPhase)
        {
            return;
        }

        state.BodyPhase++;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 8)));
        state.DesiredProgram = MagdollitePhaseDefinitions
            .Phase(state.BodyPhase).BodyProgram;
        InstallMagdolliteInstructionList(body, state);
    }

    /// <summary>Retraces the body's growth phases and releases the head's landing latch at its origin.</summary>
    internal void RunMagdolliteBodyFalling(
        RoomEnemySlot body,
        MagdolliteEnemyState state)
    {
        (RoomEnemySlot speedHead, _, _) = RequireMagdolliteComposite(body);
        MagdolliteEnemyState speedOwner = speedHead.RequireSpeciesState<MagdolliteEnemyState>();
        state.RiseDisplacement += AddMagdolliteY(body, speedOwner.DownwardVelocity);

        if (state.RiseDisplacement.Y >= 0)
        {
            state.BodyWakeRequested = true;
            (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
            head.RequireSpeciesState<MagdolliteEnemyState>().HeadWaitsForBodyLanding = false;
            state.Function = MagdolliteEnemyFunction.BodyDormant;
            return;
        }

        ushort downwardDistanceRemaining = unchecked((ushort)-state.RiseDisplacement.Y);
        ushort threshold = MagdollitePhaseDefinitions.Phase(state.BodyPhase - 1).DistanceThreshold;
        if (!EnemyMath.IsNegative16(downwardDistanceRemaining - threshold))
            return;

        state.BodyPhase--;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 8)));
        state.DesiredProgram = MagdollitePhaseDefinitions
            .Phase(state.BodyPhase).BodyProgram;
        InstallMagdolliteInstructionList(body, state);
    }

    /// <summary>Begins following the rising body when the head enters its attack animation.</summary>
    private void RunMagdolliteOverlayWaiting(
        RoomEnemySlot overlay,
        MagdolliteEnemyState state)
    {
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(overlay);
        if (head.RequireSpeciesState<MagdolliteEnemyState>().Function ==
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
            MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
            state.DesiredProgram = MagdolliteProgram.PillarCap;
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
        MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
        if (bodyState.BodyReachedApex)
        {
            bool samusIsRight = samus is not null &&
                !EnemyMath.IsNegative16(unchecked((ushort)(samus.Position.X.Whole - overlay.Position.X.Whole)));
            state.DesiredProgram = samusIsRight
                ? MagdolliteProgram.RightThrow
                : MagdolliteProgram.LeftThrow;
            state.ThrowsRight = samusIsRight;
            InstallMagdolliteInstructionList(overlay, state);
            state.Function = MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation;
            state.ThrowOrigin = overlay.Position.Whole;
        }
        else
        {
            ushort offset = MagdollitePhaseDefinitions
                .Phase(bodyState.BodyPhase).OverlayYOffset;
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
        MagdolliteEnemyState headState = head.RequireSpeciesState<MagdolliteEnemyState>();
        MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
        if (headState.Function == MagdolliteEnemyFunction.HeadWaiting)
        {
            state.Function = MagdolliteEnemyFunction.OverlayWaitingForAttack;
        }
        else
        {
            ushort offset = MagdollitePhaseDefinitions
                .Phase(bodyState.BodyPhase).OverlayYOffset;
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

    /// <summary>
    /// Adds a signed 16.16 Y velocity, carrying the fraction into the whole, and returns the whole-pixel move it made
    /// (only Y is meaningful).
    /// </summary>
    private static Offset16<RoomPixels> AddMagdolliteY(
        RoomEnemySlot slot,
        SnesSignedSixteenSixteen velocity)
    {
        Position16<RoomPixels> before = slot.Position.Whole;
        slot.Position = slot.Position.AdvanceY(velocity);
        return slot.Position.Whole - before;
    }

    /// <summary>Restarts the animation only when the desired program differs from the installed one.</summary>
    private static void InstallMagdolliteInstructionList(
        RoomEnemySlot slot,
        MagdolliteEnemyState state)
    {
        if (state.DesiredProgram == state.InstalledProgram)
            return;
        state.Program = AnimationProgramCursor<MagdolliteProgram>.Start(state.DesiredProgram);
        state.InstalledProgram = state.DesiredProgram;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Finds and validates the retail-ordered head, body and overlay around any one component.</summary>
    private (RoomEnemySlot Head, RoomEnemySlot Body, RoomEnemySlot Overlay)
        RequireMagdolliteComposite(RoomEnemySlot member)
    {
        int headIndex = member.SlotIndex - checked(member.Spawn.Population.Parameter1);
        if (headIndex < 0 || headIndex + 2 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {member.SlotIndex} has no complete composite.");

        RoomEnemySlot head = _slotTable.Slots[headIndex];
        RoomEnemySlot body = _slotTable.Slots[headIndex + 1];
        RoomEnemySlot overlay = _slotTable.Slots[headIndex + 2];
        if (head.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || head.Spawn.Population.Parameter1 != 0 ||
            body.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || body.Spawn.Population.Parameter1 != 1 ||
            overlay.EnemyDefinitionPointer != EnemyDefinitionId.Magdollite || overlay.Spawn.Population.Parameter1 != 2)
        {
            throw new InvalidDataException(
                $"Magdollite slot {member.SlotIndex} is not inside a retail-ordered 0/1/2 composite.");
        }
        return (head, body, overlay);
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:B0B2.</summary>
    internal void StepMagdollitePaletteAnimation()
    {
        if (!MagdollitePaletteAnimationInstalled)
            return;

        MagdollitePaletteAnimationTimer = MagdollitePaletteAnimationTimer.Decrement().Remaining;
        if (!MagdollitePaletteAnimationTimer.IsZero)
            return;

        MagdollitePaletteAnimationTimer = NativeCountdown.Start(8);
        MagdollitePaletteFrame = (MagdollitePaletteFrame + 1) % MagdollitePaletteRomData.FrameCount;
        int paletteFrame = MagdollitePaletteFrame;
        int destination = MagdollitePaletteBaseColor +
            MagdollitePaletteRomData.FirstAnimatedColor;
        (_room.Artwork?.MagdollitePaletteCycle ?? throw new InvalidDataException(
            "Magdollite palette animation requires installed artwork."))
            .ApplyFrame(_presentation.Cgram, paletteFrame, destination);
    }

    /// <summary>
    /// Carries out one cue of a Magdollite part's animations: the sixteen private bank-$A8 callbacks the ten
    /// retail lists reference. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private MagdolliteProgram? ApplyMagdolliteCue(
        RoomEnemySlot slot,
        MagdolliteCue cue,
        Position16<RoomPixels> camera)
    {
        MagdolliteEnemyState state = slot.RequireSpeciesState<MagdolliteEnemyState>();
        switch (cue.Kind)
        {
            case MagdolliteCueKind.PlaySoundIfOnScreen:
            {
                bool offScreen = EnemyMath.IsNegative16(slot.Position.X.Whole - camera.X) ||
                    EnemyMath.IsNegative16(camera.X + 256 - slot.Position.X.Whole) ||
                    EnemyMath.IsNegative16(slot.Position.Y.Whole - camera.Y) ||
                    EnemyMath.IsNegative16(camera.Y + 256 - slot.Position.Y.Whole);
                if (!offScreen)
                    LastMagdolliteSoundEffect = cue.Sound ?? throw new InvalidDataException("A Magdollite sound cue carries no sound.");
                break;
            }
            case MagdolliteCueKind.MoveDown2:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 2)));
                break;
            case MagdolliteCueKind.MoveUp2:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 2)));
                break;
            case MagdolliteCueKind.SetAnimationBusy:
                state.AnimationBusy = true;
                break;
            case MagdolliteCueKind.ClearAnimationBusy:
                state.AnimationBusy = false;
                break;
            case MagdolliteCueKind.MoveWithPillarUp1:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 1)));
                next.Position = next.Position.WithWholeY(unchecked((ushort)(next.Position.Y.Whole - 1)));
                break;
            }
            case MagdolliteCueKind.MoveWithPillarDown1:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
                next.Position = next.Position.WithWholeY(unchecked((ushort)(next.Position.Y.Whole + 1)));
                break;
            }
            case MagdolliteCueKind.SinkAndShowParts:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = (state.Origin + new Offset16<RoomPixels>(0, 24)).Y;
                slot.Position = slot.Position.WithWholeY(y);
                body.Position = body.Position.WithWholeY(y);
                body.Properties = body.Properties.Without(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.Without(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteCueKind.RestoreOrigins:
            {
                (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(slot);
                slot.Position = slot.Position.WithWholeY(state.Origin.Y);
                body.Position = body.Position.WithWholeY(state.Origin.Y);
                break;
            }
            case MagdolliteCueKind.SurfaceAndHideParts:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                ushort y = (state.Origin + new Offset16<RoomPixels>(0, 4)).Y;
                slot.Position = slot.Position.WithWholeY(y);
                body.Position = body.Position.WithWholeY(y);
                body.Properties = body.Properties.With(EnemyProperties.Invisible);
                overlay.Properties = overlay.Properties.With(EnemyProperties.Invisible);
                break;
            }
            case MagdolliteCueKind.SpawnLava:
                SpawnMagdolliteLava(slot, state.ThrowsRight);
                break;
            case MagdolliteCueKind.ShiftRightUp4:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -4));
                break;
            case MagdolliteCueKind.ShiftLeftUp4:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteCueKind.ShiftRightUp8:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -8));
                break;
            case MagdolliteCueKind.ShiftLeftUp4Again:
                slot.Position = slot.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteCueKind.StartCooldown:
                state.AttackTimer = NativeCountdown.Start(256);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Resolves the next physical record used by paired base-and-pillar animation opcodes.</summary>
    private RoomEnemySlot NextMagdolliteSlot(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} has no following component.");
        return _slotTable.Slots[slot.SlotIndex + 1];
    }

    /// <summary>Ports the shared post-combat tail at $A8:B410.</summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemySlot current)
    {
        // The native callback literally addresses current+$40 and current+$80. Ordinary
        // touch/shot reaches only the collision-enabled head, but power bombs may expose
        // this retail cross-record quirk, so do not silently normalize current to a group.
        RoomEnemySlot? following = current.SlotIndex + 1 < RoomEnemySystem.MaximumEnemyCount
            ? _slotTable.Slots[current.SlotIndex + 1]
            : null;
        RoomEnemySlot? secondFollowing = current.SlotIndex + 2 < RoomEnemySystem.MaximumEnemyCount
            ? _slotTable.Slots[current.SlotIndex + 2]
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
            following.AiStates = following.AiStates | EnemyAiStates.Frozen;
        }
        if (secondFollowing is not null)
        {
            secondFollowing.FrozenTimer = current.FrozenTimer;
            secondFollowing.AiStates = secondFollowing.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Attaches the species tail after common bomb damage when the shot AI selects Magdollite's reaction.</summary>
    internal void RegisterMagdolliteBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.MagdolliteShot)
        {
            AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects thrown-lava movement, its typed programs' cues and its shot program to the projectile engine.</summary>
    internal void RegisterMagdolliteProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunMagdolliteLavaPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MagdolliteLava);

        table.RegisterCue<MagdolliteLavaCue>((context, cue) => ApplyMagdolliteLavaCue(context.Projectile, cue));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.LavaThrownByMagdollite, () => new MagdolliteLavaAnimation(MagdolliteLavaProgram.Shot));
    }

    /// <summary>Carries out one cue of the thrown lava's programs.</summary>
    private void ApplyMagdolliteLavaCue(RoomEnemyProjectileSlot projectile, MagdolliteLavaCue cue)
    {
        switch (cue.Kind)
        {
            case MagdolliteLavaCueKind.Delete:
                projectile.Clear();
                break;
            case MagdolliteLavaCueKind.SpawnDrops:
                RequestMagdolliteLavaDrop(projectile);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteLavaCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Ports Magdollite's initialization AI.</summary>
    internal void RunMagdolliteInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? samus, SnesButton _1, ushort _2)
    {
        InitializeMagdollite(slot, samus);
    }

    /// <summary>Ports Magdollite's main AI.</summary>
    internal void RunMagdolliteMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMagdolliteMain(slot, slot.RequireSpeciesState<MagdolliteEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Magdollite's typed animation.</summary>
    internal void AdvanceMagdolliteTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, MagdolliteAnimations.Programs, MagdolliteFrameDefinitions.ShownFrames.Of,
            static (MagdolliteEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyMagdolliteCue(slot, cue, context.Camera));

    /// <summary>Propagates common power-bomb death and freezing effects to following composite records.</summary>
    internal void RegisterMagdollitePowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.MagdollitePowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveMagdolliteCombatAfterCommon(enemy),
        });
    }

    /// <summary>Propagates common shot death and freezing effects to following composite records.</summary>
    internal void RegisterMagdolliteShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MagdolliteShot)
        {
            AfterCommon = (c, _) => ResolveMagdolliteCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Applies normal Samus contact damage before mirroring the enemy's death or freeze state.</summary>
    internal void RegisterMagdolliteTouch(EnemySharedReactions table)
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
    private const ushort MagdolliteLavaHorizontalSpeed = 768;
    /// <summary>Negative 8.8 value that the leftward routine reads from the projectile's Y velocity field.</summary>
    private const ushort MagdolliteLavaUpwardSpeed = unchecked((ushort)-768);

    /// <summary>Ports <c>EprojInit_NorfairLavaMan</c> at $86:E000.</summary>
    internal void SpawnMagdolliteLava(RoomEnemySlot source, bool throwsRight)
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
        projectile.DirectionParameter = throwsRight ? (ushort)1 : (ushort)0;
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(unchecked((ushort)(source.Position.Y.Whole + 2)), source.Position.Y.Fraction));
        projectile.Velocity = new(new SnesSignedEightEight(MagdolliteLavaHorizontalSpeed), new SnesSignedEightEight(MagdolliteLavaUpwardSpeed));
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.MagdolliteLava;
        projectile.InstallAnimation(new MagdolliteLavaAnimation(throwsRight ? MagdolliteLavaProgram.Right : MagdolliteLavaProgram.Left));
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
        _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
            projectile.Position.Whole,
            EnemyDefinitionId.Magdollite);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        ResetMagdolliteFrameEvents();
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastMagdolliteSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }
}
