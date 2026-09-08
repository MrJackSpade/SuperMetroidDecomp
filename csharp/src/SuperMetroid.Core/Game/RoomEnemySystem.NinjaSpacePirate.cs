using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Confirmed meanings in ninja-Pirate population parameter one. The cartridge currently
/// tests only bit zero; preserving the remaining bits as raw data avoids assigning names to
/// behavior that the retail routine never actually proves.
/// </summary>
[Flags]
public enum NinjaSpacePirateParameterFlags : ushort
{
    None = 0,
    StartsAtLeftPostFacingRight = 0x0001,
}

/// <summary>
/// Literal bank-$B2 function pointers stored in native enemy variable A. These values are
/// intentionally addresses, so debugger state can be compared directly with WRAM and the
/// disassembly instead of being hidden behind a host-only state number.
/// </summary>
public enum NinjaSpacePirateFunction : ushort
{
    NoOperation = 0x804b,
    Initial = 0xf6a9,
    Active = 0xf6e4,
    SpinJumpLeftRising = 0xf817,
    SpinJumpLeftFalling = 0xf84c,
    SpinJumpRightRising = 0xf890,
    SpinJumpRightFalling = 0xf8c5,
    ReadyToDivekick = 0xf909,
    DivekickLeftJump = 0xf985,
    DivekickLeftDive = 0xf9c1,
    DivekickLeftWalkToPost = 0xfa15,
    DivekickRightJump = 0xfa59,
    DivekickRightDive = 0xfa95,
    DivekickRightWalkToPost = 0xfae9,
}

/// <summary>
/// Debugger-facing projection of one ninja Pirate. Variables A-F remain backed by the
/// ordinary enemy record. The final three properties represent the otherwise-unaliased
/// bank-$7E:7800 scratch words used for current speed, a calculated dive target, and spawn Y.
/// </summary>
public sealed class NinjaSpacePirateEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NinjaSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    public NinjaSpacePirateFunction Function
    {
        get => (NinjaSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>The derived peak spin-jump speed written to native variable B.</summary>
    public ushort CalculatedSpinJumpSpeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>The active left/right instruction-list pointer retained in variable C.</summary>
    public ushort ActiveInstruction
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort PostsMidpointX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort LeftPostX
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort RightPostX
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort Speed { get; internal set; }
    public ushort DiveTargetX { get; internal set; }
    public ushort SpawnY { get; internal set; }
    public int SpawnedClawCount { get; internal set; }
    public int LandingDustCount { get; internal set; }
}

/// <summary>
/// Complete translation of ninja Space Pirates $F4D3/$F513/$F553/$F593/$F5D3/$F613:
/// initialization, cartridge animation bytecode, proximity decisions, flinch/kick/claw
/// attacks, both post-to-post jump forms, room collision, landing effects, and claw actors.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort GreyNinjaSpacePirateDefinition = 0xf4d3;
    internal const ushort GreenNinjaSpacePirateDefinition = 0xf513;
    internal const ushort RedNinjaSpacePirateDefinition = 0xf553;
    internal const ushort GoldNinjaSpacePirateDefinition = 0xf593;
    internal const ushort MagentaNinjaSpacePirateDefinition = 0xf5d3;
    internal const ushort SilverNinjaSpacePirateDefinition = 0xf613;

    private const ushort NinjaPirateClawAttackLeft = 0xf15c;
    private const ushort NinjaPirateSpinJumpLeft = 0xf1c4;
    private const ushort NinjaPirateActiveFacingLeft = 0xf22e;
    private const ushort NinjaPirateFlinchFacingLeft = 0xf270;
    private const ushort NinjaPirateDivekickLeftJump = 0xf27c;
    private const ushort NinjaPirateDivekickLeftDive = 0xf2a0;
    private const ushort NinjaPirateWalkToLeftPost = 0xf2b2;
    private const ushort NinjaPirateInitialFacingLeft = 0xf2da;
    private const ushort NinjaPirateLandFacingLeft = 0xf2f8;
    private const ushort NinjaPirateKickFacingLeft = 0xf32e;
    private const ushort NinjaPirateClawAttackRight = 0xf34a;
    private const ushort NinjaPirateSpinJumpRight = 0xf3b2;
    private const ushort NinjaPirateActiveFacingRight = 0xf420;
    private const ushort NinjaPirateFlinchFacingRight = 0xf462;
    private const ushort NinjaPirateDivekickRightJump = 0xf46e;
    private const ushort NinjaPirateDivekickRightDive = 0xf492;
    private const ushort NinjaPirateWalkToRightPost = 0xf4a4;
    private const ushort NinjaPirateInitialFacingRight = 0xf4cc;
    private const ushort NinjaPirateLandFacingRight = 0xf4ea;
    private const ushort NinjaPirateKickFacingRight = 0xf51a;

    private const ushort NinjaPiratePaletteNormal = 0x0200;
    private const ushort NinjaPirateInitialDiveSpeed = 0x0600;
    private const ushort NinjaPirateSoundClawKickOrDive = 0x0066;
    private const int NinjaPirateActivationDistance = 128;
    private const int NinjaPirateFlinchDistance = 32;
    private const int NinjaPirateMidpointTriggerDistance = 32;
    private const int NinjaPirateKickDistance = 40;
    private const int NinjaPirateDiveHorizontalPixels = 5;
    private const int NinjaPirateReturnWalkPixels = 2;
    private const int NinjaPirateSpinVerticalPixels = 2;

    private readonly NinjaSpacePirateEnemyState?[] _ninjaSpacePirateStates =
        new NinjaSpacePirateEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<NinjaSpacePirateEnemyState?> NinjaSpacePirateStates =>
        _ninjaSpacePirateStates;

    internal static bool IsNinjaSpacePirateDefinition(ushort definition) => definition is
        GreyNinjaSpacePirateDefinition or
        GreenNinjaSpacePirateDefinition or
        RedNinjaSpacePirateDefinition or
        GoldNinjaSpacePirateDefinition or
        MagentaNinjaSpacePirateDefinition or
        SilverNinjaSpacePirateDefinition;

    /// <summary>Ports <c>InitAI_PirateNinja</c> at <c>$B2:F5DE</c>.</summary>
    private void InitializeNinjaSpacePirate(RoomEnemySlot slot)
    {
        var state = new NinjaSpacePirateEnemyState(slot);
        _ninjaSpacePirateStates[slot.SlotIndex] = state;

        bool startsAtLeft = ((NinjaSpacePirateParameterFlags)slot.Parameter1 &
            NinjaSpacePirateParameterFlags.StartsAtLeftPostFacingRight) != 0;
        slot.CurrentInstruction = startsAtLeft
            ? NinjaPirateInitialFacingRight
            : NinjaPirateInitialFacingLeft;
        state.ActiveInstruction = slot.CurrentInstruction;

        // Parameter two describes the requested distance between posts. The native setup
        // first establishes those requested posts around the population X coordinate, then
        // deliberately replaces them with a symmetric pair derived from its acceleration
        // sum. That adjustment is why the final span can differ slightly from parameter two.
        ushort requestedLeft = startsAtLeft
            ? slot.XPosition
            : unchecked((ushort)(slot.XPosition - slot.Parameter2));
        ushort requestedRight = startsAtLeft
            ? unchecked((ushort)(slot.XPosition + slot.Parameter2))
            : slot.XPosition;
        ushort requestedHalfSpan = unchecked((ushort)(requestedRight - requestedLeft));
        requestedHalfSpan >>= 1;
        state.PostsMidpointX = unchecked((ushort)(requestedLeft + requestedHalfSpan));

        // `$B2:F632` truncates the half-span to one byte before moving it to the high byte.
        // The do/while always executes once, including a zero-span debug population.
        ushort distanceInSubpixels = unchecked((ushort)((byte)requestedHalfSpan << 8));
        ushort speed = 0;
        ushort accumulatedDistance = 0;
        do
        {
            speed = unchecked((ushort)(speed + 0x0020));
            accumulatedDistance = unchecked((ushort)(accumulatedDistance + speed));
        }
        while (unchecked((short)(accumulatedDistance - distanceInSubpixels)) < 0);

        state.CalculatedSpinJumpSpeed = speed;
        ushort adjustedHalfSpan = unchecked((byte)(accumulatedDistance >> 8));
        state.RightPostX = unchecked((ushort)(state.PostsMidpointX + adjustedHalfSpan));
        state.LeftPostX = unchecked((ushort)(state.PostsMidpointX - adjustedHalfSpan));
        slot.XPosition = startsAtLeft ? state.LeftPostX : state.RightPostX;
        slot.XSubposition = 0;
        state.Function = NinjaSpacePirateFunction.NoOperation;
        state.SpawnY = slot.YPosition;

        // The final sixteen target-palette colors are common ninja-Pirate colors copied from
        // `$B2:8727`. In this runtime CGRAM is the visible palette target, so the transfer is
        // performed directly while retaining the cartridge source and destination indexes.
        _cgram!.LoadFromBus(_bus!, 0xb28727, colorCount: 16, destinationIndex: 240);
    }

    /// <summary>Dispatches the literal function installed in native variable A.</summary>
    private void RunNinjaSpacePirateMain(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        switch (state.Function)
        {
            case NinjaSpacePirateFunction.NoOperation:
                return;
            case NinjaSpacePirateFunction.Initial:
                RunNinjaPirateInitial(slot, state, RequireSamus(), samusProjectiles);
                return;
            case NinjaSpacePirateFunction.Active:
                RunNinjaPirateActive(slot, state, RequireSamus(), samusProjectiles);
                return;
            case NinjaSpacePirateFunction.SpinJumpLeftRising:
                StepNinjaPirateSpinJump(slot, state, movingRight: false, rising: true);
                return;
            case NinjaSpacePirateFunction.SpinJumpLeftFalling:
                StepNinjaPirateSpinJump(slot, state, movingRight: false, rising: false);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightRising:
                StepNinjaPirateSpinJump(slot, state, movingRight: true, rising: true);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightFalling:
                StepNinjaPirateSpinJump(slot, state, movingRight: true, rising: false);
                return;
            case NinjaSpacePirateFunction.ReadyToDivekick:
                RunNinjaPirateReadyToDivekick(
                    slot,
                    state,
                    RequireSamus(),
                    samusProjectiles);
                return;
            case NinjaSpacePirateFunction.DivekickLeftJump:
                StepNinjaPirateDiveJump(slot, state, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightJump:
                StepNinjaPirateDiveJump(slot, state, movingRight: true);
                return;
            case NinjaSpacePirateFunction.DivekickLeftDive:
                StepNinjaPirateDive(slot, state, level, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightDive:
                StepNinjaPirateDive(slot, state, level, movingRight: true);
                return;
            case NinjaSpacePirateFunction.DivekickLeftWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, movingRight: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Ninja Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Ninja Space Pirate decision AI requires the active Samus actor.");
    }

    private static void RunNinjaPirateInitial(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) >=
            NinjaPirateActivationDistance)
        {
            _ = TryNinjaPirateProjectileFlinch(slot, samus, samusProjectiles);
            return;
        }

        ushort active = unchecked((short)(slot.XPosition - samus.XPosition)) < 0
            ? NinjaPirateActiveFacingRight
            : NinjaPirateActiveFacingLeft;
        state.ActiveInstruction = active;
        InstallNinjaPirateInstruction(slot, active);
    }

    private static void RunNinjaPirateActive(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(slot, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(slot, samus) ||
            TryNinjaPirateSpinJump(slot, state, samus))
        {
            return;
        }

        TryNinjaPirateClawAttack(slot, state, samus);
    }

    private static bool TryNinjaPirateProjectileFlinch(
        RoomEnemySlot slot,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (samusProjectiles is null)
            return false;

        // `$B2:F72E` selects only the highest occupied Samus projectile slot. If that one is
        // far away, it returns immediately instead of searching a lower slot that is closer.
        SamusProjectileSlot? projectile = null;
        for (int index = Math.Min(4, samusProjectiles.Slots.Count - 1); index >= 0; index--)
        {
            if (samusProjectiles.Slots[index].IsActive)
            {
                projectile = samusProjectiles.Slots[index];
                break;
            }
        }
        if (projectile is null ||
            Math.Abs(unchecked((short)(projectile.XPosition - slot.XPosition))) >=
                NinjaPirateFlinchDistance ||
            Math.Abs(unchecked((short)(projectile.YPosition - slot.YPosition))) >=
                NinjaPirateFlinchDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            unchecked((short)(slot.XPosition - samus.XPosition)) < 0
                ? NinjaPirateFlinchFacingRight
                : NinjaPirateFlinchFacingLeft);
        return true;
    }

    private static bool TryNinjaPirateStandingKick(RoomEnemySlot slot, SamusState samus)
    {
        if (Math.Abs(unchecked((short)(samus.XPosition - slot.XPosition))) >=
                NinjaPirateKickDistance ||
            Math.Abs(unchecked((short)(samus.YPosition - slot.YPosition))) >=
                NinjaPirateKickDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            unchecked((short)(slot.XPosition - samus.XPosition)) < 0
                ? NinjaPirateKickFacingRight
                : NinjaPirateKickFacingLeft);
        return true;
    }

    private static bool TryNinjaPirateSpinJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(unchecked((short)(state.PostsMidpointX - samus.XPosition))) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            slot.XPosition == state.LeftPostX
                ? NinjaPirateSpinJumpRight
                : NinjaPirateSpinJumpLeft);
        return true;
    }

    private static void TryNinjaPirateClawAttack(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if ((slot.FrameCounter & 0x003f) != 0)
            return;

        if (slot.XPosition == state.LeftPostX)
        {
            if (unchecked((short)(slot.XPosition - samus.XPosition)) < 0)
                return;
            InstallNinjaPirateInstruction(slot, NinjaPirateClawAttackLeft);
            return;
        }

        if (unchecked((short)(slot.XPosition - samus.XPosition)) >= 0)
            return;
        InstallNinjaPirateInstruction(slot, NinjaPirateClawAttackRight);
    }

    private void RunNinjaPirateReadyToDivekick(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(slot, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(slot, samus))
        {
            return;
        }

        if (Math.Abs(unchecked((short)(state.PostsMidpointX - samus.XPosition))) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return;
        }

        // The table contains three identical entries per side. The random retry is retained
        // because advancing the native RNG until its low two bits are nonzero is observable
        // even though all three accepted values select the same list.
        ushort choice;
        do
            choice = unchecked((ushort)(_nextRandom!() & 3));
        while (choice == 0);

        InstallNinjaPirateInstruction(
            slot,
            slot.XPosition == state.LeftPostX
                ? NinjaPirateDivekickRightJump
                : NinjaPirateDivekickLeftJump);
    }

    private void StepNinjaPirateSpinJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight,
        bool rising)
    {
        int xPixels = state.Speed >> 8;
        slot.XPosition = unchecked((ushort)(slot.XPosition + (movingRight ? xPixels : -xPixels)));
        slot.YPosition = unchecked((ushort)(slot.YPosition +
            (rising ? -NinjaPirateSpinVerticalPixels : NinjaPirateSpinVerticalPixels)));

        if (rising)
        {
            state.Speed = unchecked((ushort)(state.Speed + 0x0020));
            bool crossedMidpoint = movingRight
                ? unchecked((short)(slot.XPosition - state.PostsMidpointX)) >= 0
                : unchecked((short)(slot.XPosition - state.PostsMidpointX)) < 0;
            if (crossedMidpoint)
            {
                state.Function = movingRight
                    ? NinjaSpacePirateFunction.SpinJumpRightFalling
                    : NinjaSpacePirateFunction.SpinJumpLeftFalling;
            }
            return;
        }

        state.Speed = unchecked((ushort)(state.Speed - 0x0020));
        if (state.Speed != 0)
            return;

        state.Function = NinjaSpacePirateFunction.NoOperation;
        slot.XPosition = movingRight ? state.RightPostX : state.LeftPostX;
        InstallNinjaPirateInstruction(
            slot,
            movingRight ? NinjaPirateLandFacingRight : NinjaPirateLandFacingLeft);
        SpawnNinjaPirateLandingDust(slot, state);
    }

    private static void InitializeNinjaPirateDive(
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        state.Speed = NinjaPirateInitialDiveSpeed;
        state.DiveTargetX = movingRight
            ? unchecked((ushort)(state.LeftPostX +
                ((ushort)(state.PostsMidpointX - state.LeftPostX) >> 1)))
            : unchecked((ushort)(state.PostsMidpointX +
                ((ushort)(state.RightPostX - state.PostsMidpointX) >> 1)));
    }

    private static void StepNinjaPirateDiveJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        slot.YPosition = unchecked((ushort)(slot.YPosition - (state.Speed >> 8)));
        state.Speed = unchecked((ushort)(state.Speed - 0x0040));
        if (unchecked((short)state.Speed) >= 0)
            return;

        state.Function = movingRight
            ? NinjaSpacePirateFunction.DivekickRightDive
            : NinjaSpacePirateFunction.DivekickLeftDive;
        InstallNinjaPirateInstruction(
            slot,
            movingRight ? NinjaPirateDivekickRightDive : NinjaPirateDivekickLeftDive);
        state.Speed = NinjaPirateInitialDiveSpeed;
    }

    private void StepNinjaPirateDive(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        RoomLevelData? level,
        bool movingRight)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Ninja Space Pirate divekick movement requires room collision data.");
        }

        slot.XPosition = unchecked((ushort)(slot.XPosition +
            (movingRight ? NinjaPirateDiveHorizontalPixels : -NinjaPirateDiveHorizontalPixels)));

        // MoveEnemyDownBy_14_12 receives the high speed byte as the signed integer word and
        // the low byte as only the low eight bits of the fractional word. This odd packing is
        // preserved literally; treating Speed as a conventional 8.8 fixed value is subtly
        // different from the 65816 register setup at $B2:F9CB/$FA9F.
        int displacement = ((state.Speed >> 8) << 16) | (state.Speed & 0x00ff);
        bool landed = MoveEnemyVertically(level, slot, displacement);
        if (!landed)
        {
            state.Speed = unchecked((ushort)(state.Speed - 0x0040));
            landed = unchecked((short)state.Speed) < 0 || (state.Speed & 0xff00) == 0;
        }
        if (!landed)
            return;

        state.Function = movingRight
            ? NinjaSpacePirateFunction.DivekickRightWalkToPost
            : NinjaSpacePirateFunction.DivekickLeftWalkToPost;
        InstallNinjaPirateInstruction(
            slot,
            movingRight ? NinjaPirateWalkToRightPost : NinjaPirateWalkToLeftPost);
        slot.YPosition = state.SpawnY;
        slot.YSubposition = 0;
        SpawnNinjaPirateLandingDust(slot, state);
    }

    private static void StepNinjaPirateWalkToPost(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        slot.XPosition = unchecked((ushort)(slot.XPosition +
            (movingRight ? NinjaPirateReturnWalkPixels : -NinjaPirateReturnWalkPixels)));
        bool reachedPost = movingRight
            ? unchecked((short)(slot.XPosition - state.RightPostX)) >= 0
            : unchecked((short)(slot.XPosition - state.LeftPostX)) < 0;
        if (!reachedPost)
            return;

        slot.XPosition = movingRight ? state.RightPostX : state.LeftPostX;
        InstallNinjaPirateInstruction(
            slot,
            movingRight ? NinjaPirateLandFacingRight : NinjaPirateLandFacingLeft);
        state.Function = NinjaSpacePirateFunction.NoOperation;
    }

    private void SpawnNinjaPirateLandingDust(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state)
    {
        ushort graphicsIndex = 0;
        if (SpawnRoomSpriteObject(
                unchecked((ushort)(slot.XPosition - 8)),
                unchecked((ushort)(slot.YPosition + 28)),
                RoomSpriteObjectKind.NinjaPirateLandingDust,
                graphicsIndex) is not null)
        {
            state.LandingDustCount++;
        }
        if (SpawnRoomSpriteObject(
                unchecked((ushort)(slot.XPosition + 8)),
                unchecked((ushort)(slot.YPosition + 28)),
                RoomSpriteObjectKind.NinjaPirateLandingDust,
                graphicsIndex) is not null)
        {
            state.LandingDustCount++;
        }
    }

    private void SpawnNinjaPirateClaw(
        RoomEnemySlot source,
        NinjaSpacePirateEnemyState state,
        ushort direction,
        ushort xOffset,
        ushort yOffset)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateClaw,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        projectile.XPosition = unchecked((ushort)(source.XPosition + unchecked((short)xOffset)));
        projectile.YPosition = unchecked((ushort)(source.YPosition + unchecked((short)yOffset)));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.DirectionParameter = direction;
        projectile.InstructionPointer = direction == 0
            ? EnemyProjectileInstructionLists.PirateLaserLeft
            : EnemyProjectileInstructionLists.PirateLaserRight;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = EnemyProjectileCodePointers.RTS_86A05B;
        projectile.Variable0 = 0x0800;
        projectile.Variable1 = 1;
        state.SpawnedClawCount++;
    }

    /// <summary>Runs claw pre-instructions $86:A0D1/$A124 and exact camera culling.</summary>
    private static void RunNinjaPirateClawPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        int pixels = projectile.Variable0 >> 8;
        bool outbound = projectile.Variable1 != 0;
        bool thrownRight = projectile.PreInstruction ==
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Right;
        int direction = thrownRight ? 1 : -1;

        // Outbound motion follows the throw direction while decelerating to zero; inbound
        // motion reverses direction and accelerates indefinitely until the 256x256 camera
        // check deletes the claw. Y advances one pixel on both halves of the flight.
        projectile.XPosition = unchecked((ushort)(projectile.XPosition +
            (outbound ? direction * pixels : -direction * pixels)));
        projectile.Variable0 = unchecked((ushort)(projectile.Variable0 +
            (outbound ? -0x0020 : 0x0020)));
        if (outbound && projectile.Variable0 == 0)
            projectile.Variable1 = 0;
        projectile.YPosition = unchecked((ushort)(projectile.YPosition + 1));

        ushort right = unchecked((ushort)(cameraX + 256));
        ushort bottom = unchecked((ushort)(cameraY + 256));
        bool offScreen = unchecked((short)(projectile.XPosition - cameraX)) < 0 ||
            unchecked((short)(projectile.XPosition - right)) >= 0 ||
            unchecked((short)(projectile.YPosition - cameraY)) < 0 ||
            unchecked((short)(projectile.YPosition - bottom)) >= 0;
        if (offScreen)
            projectile.Clear();
    }

    /// <summary>Executes all cartridge opcodes reachable from retail ninja lists.</summary>
    private bool TryProcessNinjaSpacePirateInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort opcode,
        ref ushort cursor)
    {
        if (!IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer))
            return false;

        NinjaSpacePirateEnemyState state = RequireNinjaSpacePirateState(slot);
        int operandAddress = (slot.Definition.Bank << 16) |
            unchecked((ushort)(cursor + 2));
        switch (opcode)
        {
            case SpacePirateInstructionCodes.Instruction_PirateWall_FunctionInY:
                state.Function = (NinjaSpacePirateFunction)ReadWord(_bus!, operandAddress);
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_PaletteIndexInY:
                slot.PaletteIndex = ReadWord(_bus!, operandAddress);
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_QueueSoundInY_Lib2_Max6:
                LastSpacePirateSoundEffect = ReadWord(_bus!, operandAddress);
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset:
                SpawnNinjaPirateClaw(
                    slot,
                    state,
                    ReadWord(_bus!, operandAddress),
                    ReadWord(_bus!, operandAddress + 2),
                    ReadWord(_bus!, operandAddress + 4));
                cursor = unchecked((ushort)(cursor + 8));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SetFunction0FAC_Active:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ninja Space Pirate facing selection requires the active Samus actor.");
                }
                state.ActiveInstruction = unchecked((short)(slot.XPosition - samus.XPosition)) < 0
                    ? NinjaPirateActiveFacingRight
                    : NinjaPirateActiveFacingLeft;
                slot.InstructionTimer = 1;
                // The native instruction returns the selected list, not its next word.
                // Re-entering that list executes FunctionInY and restores decision AI
                // after the idle animation explicitly installed the no-op function.
                cursor = state.ActiveInstruction;
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_ResetSpeed:
                state.Speed = 0;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SetLeftDivekickJumpInitialYSpeed:
                InitializeNinjaPirateDive(state, movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SetRightDivekickJumpInitialYSpeed:
                InitializeNinjaPirateDive(state, movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                return false;
        }
    }

    private static void InstallNinjaPirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    private NinjaSpacePirateEnemyState RequireNinjaSpacePirateState(RoomEnemySlot slot) =>
        _ninjaSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized ninja Space Pirate state.");
}
