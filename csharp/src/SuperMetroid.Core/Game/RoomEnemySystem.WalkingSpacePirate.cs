using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Independently tested meanings in walking Pirate population parameter one. No other bits
/// are named: unknown/modded values remain losslessly available through the raw slot word.
/// </summary>
[Flags]
public enum WalkingSpacePirateParameterFlags : ushort
{
    None = 0,
    StartsFacingRight = 0x0001,
    SlowLaserAndProjectileFlinch = 0x8000,
}

/// <summary>
/// Literal 16-bit function identities stored in walking Space Pirate variable A. These are
/// addresses inside bank $B2, not host-selected behavior labels; exposing the raw identities
/// makes an instruction-list breakpoint directly comparable with the cartridge state.
/// </summary>
public enum WalkingSpacePirateFunction : ushort
{
    /// <summary>The shared bank-$B2 RTS copied by initialization and flinch lists.</summary>
    NoOperation = 0x804b,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft = 0xfd44,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight = 0xfdce,

    /// <summary>The second RTS used while a firing/look-around list owns presentation.</summary>
    AnimationOwnedNoOperation = 0xfe4a,
}

/// <summary>
/// Debugger-facing projection of one walking Space Pirate's native state. The patrol limits
/// deliberately remain wrapped 16-bit world coordinates because population parameter two
/// is added/subtracted without clamping by <c>$B2:FD02</c>.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal WalkingSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Bank-$B2 function address stored in native enemy variable A.</summary>
    public WalkingSpacePirateFunction Function
    {
        get => (WalkingSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Left patrol post stored in native variable E.</summary>
    public ushort LeftPostXPosition
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Right patrol post stored in native variable F.</summary>
    public ushort RightPostXPosition
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Successful allocations from the actor's three-shot firing animations.</summary>
    public int SpawnedLaserCount { get; internal set; }
}

/// <summary>
/// Translation of walking Space Pirates $F653/$F693/$F6D3/$F713/$F753/$F793, including
/// their bank-$B2 instruction opcodes and pirate/Mother-Brain laser projectile $86:A17B.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort GreyWalkingSpacePirateDefinition = 0xf653;
    internal const ushort GreenWalkingSpacePirateDefinition = 0xf693;
    internal const ushort RedWalkingSpacePirateDefinition = 0xf6d3;
    internal const ushort GoldWalkingSpacePirateDefinition = 0xf713;
    internal const ushort MagentaWalkingSpacePirateDefinition = 0xf753;
    internal const ushort SilverWalkingSpacePirateDefinition = 0xf793;

    private const ushort WalkingPirateFlinchFacingLeft = 0xfb4c;
    private const ushort WalkingPirateFlinchFacingRight = 0xfb58;
    private const ushort WalkingPirateWalkingLeft = 0xfb64;
    private const ushort WalkingPirateFireLasersLeft = 0xfb8c;
    private const ushort WalkingPirateLookingFacingLeft = 0xfbc6;
    private const ushort WalkingPirateWalkingRight = 0xfbe6;
    private const ushort WalkingPirateFireLasersRight = 0xfc0e;
    private const ushort WalkingPirateLookingFacingRight = 0xfc48;
    private const ushort PirateMotherBrainLaserSound = 0x0067;
    private const ushort PirateMotherBrainLaserListLeft = 0x9f41;
    private const ushort PirateMotherBrainLaserListRight = 0x9f7d;
    private const ushort PirateMotherBrainLaserNoOperation = 0xa05b;
    private const ushort PirateMotherBrainLaserMoveLeft = 0xa05c;
    private const ushort PirateMotherBrainLaserMoveRight = 0xa07a;
    private const int WalkingPirateOnePixelDown = 1 << 16;
    private const int WalkingPirateLeftLedgeProbePixels = 17;
    private const int WalkingPirateRightLedgeProbePixels = 16;
    private const int WalkingPirateLeftStepDisplacement = -0x3801;
    private const int WalkingPirateRightStepDisplacement = 0x3800;
    private const int WalkingPirateFiringBandPixels = 16;
    private const int WalkingPirateFlinchBoxPixels = 32;
    private const int WalkingPirateLaserMuzzleXOffset = 24;
    private const int WalkingPirateSlowLaserPixelsPerFrame = 2;
    private const int WalkingPirateFastLaserPixelsPerFrame = 4;

    // The six palette/health tiers share the same initialization, main AI, collision AI,
    // dimensions, animation lists, and projectile. Definition identity must still remain
    // explicit because damage, health, vulnerability, drops, tiles, and palette are ROM data.
    private readonly WalkingSpacePirateEnemyState?[] _walkingSpacePirateStates =
        new WalkingSpacePirateEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<WalkingSpacePirateEnemyState?> WalkingSpacePirateStates =>
        _walkingSpacePirateStates;

    /// <summary>
    /// Last library-two sound requested by the shared Pirate/Mother-Brain laser initializer
    /// or a Space Pirate instruction during this frame.
    /// </summary>
    public ushort? LastSpacePirateSoundEffect { get; private set; }

    internal static bool IsWalkingSpacePirateDefinition(ushort definition) => definition is
        GreyWalkingSpacePirateDefinition or
        GreenWalkingSpacePirateDefinition or
        RedWalkingSpacePirateDefinition or
        GoldWalkingSpacePirateDefinition or
        MagentaWalkingSpacePirateDefinition or
        SilverWalkingSpacePirateDefinition;

    /// <summary>Ports <c>InitAI_PirateWalking</c> at <c>$B2:FD02</c>.</summary>
    private void InitializeWalkingSpacePirate(RoomEnemySlot slot)
    {
        var state = new WalkingSpacePirateEnemyState(slot);
        _walkingSpacePirateStates[slot.SlotIndex] = state;

        // Parameter-one bit zero is the only initial-facing selector. Bit fifteen is an
        // independent projectile-flinch option and must not accidentally select right art.
        var parameterFlags = (WalkingSpacePirateParameterFlags)slot.Parameter1;
        slot.CurrentInstruction =
            (parameterFlags & WalkingSpacePirateParameterFlags.StartsFacingRight) != 0
            ? WalkingPirateWalkingRight
            : WalkingPirateWalkingLeft;
        state.Function = WalkingSpacePirateFunction.NoOperation;

        // Native ADC/SBC wrap at 16 bits. A modded patrol radius can therefore straddle the
        // world-coordinate seam; keeping ushort arithmetic preserves its signed CMP behavior.
        state.RightPostXPosition = unchecked((ushort)(slot.XPosition + slot.Parameter2));
        state.LeftPostXPosition = unchecked((ushort)(slot.XPosition - slot.Parameter2));
    }

    /// <summary>Ports <c>MainAI_PirateWalking</c> at <c>$B2:FD32</c>.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        switch (state.Function)
        {
            case WalkingSpacePirateFunction.NoOperation:
            case WalkingSpacePirateFunction.AnimationOwnedNoOperation:
                break;

            case WalkingSpacePirateFunction.WalkingLeft:
                RunWalkingSpacePirateLeft(slot, state, RequireSamus(), RequireLevel());
                break;

            case WalkingSpacePirateFunction.WalkingRight:
                RunWalkingSpacePirateRight(slot, state, RequireSamus(), RequireLevel());
                break;

            default:
                throw new NotSupportedException(
                    $"Walking Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
        }

        // Parameter-one bit fifteen enables the retail flinch detector after normal AI.
        // The detector is intentionally allowed to replace a list selected just above.
        if ((((WalkingSpacePirateParameterFlags)slot.Parameter1 &
                WalkingSpacePirateParameterFlags.SlowLaserAndProjectileFlinch) != 0) &&
            samus is not null)
            TryTriggerWalkingSpacePirateFlinch(slot, samus, samusProjectiles);

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires the active Samus position.");
        RoomLevelData RequireLevel() => level ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires room collision data.");
    }

    /// <summary>Ports the asymmetric left-walk function at <c>$B2:FD44</c>.</summary>
    private void RunWalkingSpacePirateLeft(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, samus);
            return;
        }

        // The first one-pixel downward move doubles as the grounded test. Carry clear in
        // the ROM means there was no floor, and the move itself is retained as falling.
        if (!MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        // Left and right are not mirrors in the cartridge. Left probes a point 17 pixels
        // away, then walks by -$3801 subpixels; right probes +16 and walks by +$3800.
        // Preserve that one-subpixel/one-pixel asymmetry instead of normalizing the speed.
        ushort originalX = slot.XPosition;
        slot.XPosition = unchecked((ushort)(
            slot.XPosition - WalkingPirateLeftLedgeProbePixels));
        bool floorAhead = MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.XPosition = originalX;

        if (floorAhead)
        {
            // The ignored `$A0:BBBF` probe checks raw level-word bit 15 at -9 pixels. Its
            // only mutable result is the DP displacement pair, which the following move
            // immediately overwrites. It therefore has no persistent actor-side effect.
            bool hitWall = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateLeftStepDisplacement);
            if (!hitWall &&
                unchecked((short)(slot.XPosition - state.LeftPostXPosition)) >= 0)
            {
                return;
            }
        }

        InstallWalkingSpacePirateInstruction(slot, WalkingPirateLookingFacingLeft);
    }

    /// <summary>Ports the right-walk function at <c>$B2:FDCE</c>.</summary>
    private void RunWalkingSpacePirateRight(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, samus);
            return;
        }

        if (!MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        ushort originalX = slot.XPosition;
        slot.XPosition = unchecked((ushort)(
            slot.XPosition + WalkingPirateRightLedgeProbePixels));
        bool floorAhead = MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.XPosition = originalX;

        if (floorAhead &&
            !MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateRightStepDisplacement) &&
            unchecked((short)(slot.XPosition - state.RightPostXPosition)) < 0)
        {
            return;
        }

        InstallWalkingSpacePirateInstruction(slot, WalkingPirateLookingFacingRight);
    }

    /// <summary>Ports <c>PirateWalking_FlinchTrigger</c> at <c>$B2:FE4B</c>.</summary>
    private static void TryTriggerWalkingSpacePirateFlinch(
        RoomEnemySlot slot,
        SamusState samus,
        SamusProjectileSystem? projectiles)
    {
        if (projectiles is null)
            return;

        SamusProjectileSlot? firstAllocated = null;
        for (int index = SamusProjectileSystem.SlotCount - 1; index >= 0; index--)
        {
            // Native code tests projectile_type, not the instruction pointer. More
            // importantly, it stops at the first nonzero high-index slot even when that
            // shot is far away; it never searches lower slots for a closer projectile.
            if (projectiles.Slots[index].Type != 0)
            {
                firstAllocated = projectiles.Slots[index];
                break;
            }
        }
        if (firstAllocated is null ||
            Math.Abs(unchecked((short)(firstAllocated.XPosition - slot.XPosition))) >=
                WalkingPirateFlinchBoxPixels ||
            Math.Abs(unchecked((short)(firstAllocated.YPosition - slot.YPosition))) >=
                WalkingPirateFlinchBoxPixels)
        {
            return;
        }

        ushort list = unchecked((short)(slot.XPosition - samus.XPosition)) < 0
            ? WalkingPirateFlinchFacingRight
            : WalkingPirateFlinchFacingLeft;
        InstallWalkingSpacePirateInstruction(slot, list);
    }

    /// <summary>Ports instruction <c>$B2:FCC8</c>'s direct-list selection.</summary>
    private static ushort SelectWalkingSpacePirateMovement(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Walking Space Pirate animation selection requires the active Samus position.");
        }

        bool samusIsRightOrAligned =
            unchecked((short)(samus.XPosition - slot.XPosition)) >= 0;
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            return samusIsRightOrAligned
                ? WalkingPirateFireLasersRight
                : WalkingPirateFireLasersLeft;
        }

        // When Samus is not in the firing band, the Pirate walks toward the opposite side:
        // Samus right selects leftward walking, Samus left selects rightward walking.
        return samusIsRightOrAligned
            ? WalkingPirateWalkingLeft
            : WalkingPirateWalkingRight;
    }

    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemySlot slot,
        SamusState samus) =>
        Math.Abs(unchecked((short)(samus.YPosition - slot.YPosition))) <
            WalkingPirateFiringBandPixels;

    private static void InstallWalkingSpacePirateAttack(
        RoomEnemySlot slot,
        SamusState samus) =>
        InstallWalkingSpacePirateInstruction(
            slot,
            unchecked((short)(samus.XPosition - slot.XPosition)) >= 0
                ? WalkingPirateFireLasersRight
                : WalkingPirateFireLasersLeft);

    private static void InstallWalkingSpacePirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    /// <summary>Ports firing opcodes <c>$B2:FC68/$FC90</c> and initializer $86:A009.</summary>
    private void SpawnWalkingSpacePirateLaser(
        RoomEnemySlot source,
        WalkingSpacePirateEnemyState state,
        bool movingRight,
        ushort yOffset)
    {
        if (!SpawnSpacePirateLaser(source, movingRight, yOffset))
            return;

        state.SpawnedLaserCount++;
    }

    /// <summary>
    /// Ports the common projectile initializer <c>$86:A009</c>. Wall and walking Pirates
    /// use different actor opcodes and muzzle offsets, but the allocated projectile is the
    /// same retail definition and therefore belongs in one shared implementation.
    /// </summary>
    private bool SpawnSpacePirateLaser(
        RoomEnemySlot source,
        bool movingRight,
        ushort yOffset)
    {
        RoomEnemyProjectileSlot? projectile = SpawnPirateMotherBrainLaser(
            source,
            unchecked((ushort)(
                source.XPosition +
                (movingRight ? WalkingPirateLaserMuzzleXOffset : -WalkingPirateLaserMuzzleXOffset))),
            unchecked((ushort)(source.YPosition - yOffset)),
            movingRight);
        if (projectile is null)
            return false;

        LastSpacePirateSoundEffect = PirateMotherBrainLaserSound;
        return true;
    }

    /// <summary>
    /// Ports the common projectile initializer <c>$86:A009</c> after the caller has chosen
    /// its muzzle coordinate. Walking Pirates, wall Pirates, and Mother Brain enter through
    /// different actor opcodes, but the cartridge deliberately funnels all of them through
    /// this definition so animation, speed flags, collision damage, and disposal agree.
    /// </summary>
    private RoomEnemyProjectileSlot? SpawnPirateMotherBrainLaser(
        RoomEnemySlot source,
        ushort xPosition,
        ushort yPosition,
        bool movingRight)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return null;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateMotherBrainLaser,
            graphicsIndex: 0);

        projectile.XPosition = xPosition;
        projectile.YPosition = yPosition;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.InstructionPointer = movingRight
            ? PirateMotherBrainLaserListRight
            : PirateMotherBrainLaserListLeft;
        projectile.InstructionTimer = 1;

        // Although the definition names A05C as its pre-instruction, initializer A009
        // deliberately replaces it with A05B. The three two-frame muzzle maps are thus
        // stationary until list opcode A050 installs and immediately executes movement.
        projectile.PreInstruction = PirateMotherBrainLaserNoOperation;
        projectile.Variable0 = source.Parameter1;

        // A009 replaces the projectile definition's property word with enemy damage OR
        // $1000. The source palette/VRAM are *not* copied: SpawnEprojWithRoomGfx passes a
        // graphics index of zero so the laser uses the room projectile tiles/palette.
        projectile.Damage = source.Definition.Damage;
        projectile.CanDamageSamus = true;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        projectile.DirectionParameter = movingRight ? (ushort)1 : (ushort)0;
        return projectile;
    }

    /// <summary>Runs pre-instructions $86:A05C/$A07A and exact 256x256 camera deletion.</summary>
    private static void RunPirateMotherBrainLaserPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        int pixels = (((WalkingSpacePirateParameterFlags)projectile.Variable0 &
                WalkingSpacePirateParameterFlags.SlowLaserAndProjectileFlinch) == 0)
            ? WalkingPirateFastLaserPixelsPerFrame
            : WalkingPirateSlowLaserPixelsPerFrame;
        projectile.XPosition = projectile.PreInstruction switch
        {
            PirateMotherBrainLaserMoveLeft => unchecked((ushort)(projectile.XPosition - pixels)),
            PirateMotherBrainLaserMoveRight => unchecked((ushort)(projectile.XPosition + pixels)),
            _ => throw new InvalidOperationException(
                $"Projectile {projectile.Kind} entered laser movement with " +
                $"pre-instruction $86:{projectile.PreInstruction:X4}."),
        };

        ushort right = unchecked((ushort)(cameraX + 256));
        ushort bottom = unchecked((ushort)(cameraY + 256));
        bool offScreen = unchecked((short)(projectile.XPosition - cameraX)) < 0 ||
            unchecked((short)(projectile.XPosition - right)) >= 0 ||
            unchecked((short)(projectile.YPosition - cameraY)) < 0 ||
            unchecked((short)(projectile.YPosition - bottom)) >= 0;
        if (offScreen)
            projectile.Clear();
    }

    private WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemySlot slot) =>
        _walkingSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized walking Space Pirate state.");
}
