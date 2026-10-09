using SuperMetroid.Core.Assets;
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
    /// <summary>Starts at the left post and initially faces toward the right post.</summary>
    StartsAtLeftPostFacingRight = 0x0001,
}

/// <summary>
/// Literal bank-$B2 function pointers stored in native enemy variable A. These values are
/// intentionally addresses, so debugger state can be compared directly with WRAM and the
/// disassembly instead of being hidden behind a host-only state number.
/// </summary>
public enum NinjaSpacePirateFunction : ushort
{
    /// <summary>$B2:804B, shared return stub used while animation bytecode owns the actor.</summary>
    NoOperation = 0x804b,
    /// <summary>$B2:F6A9, waits for Samus to enter activation range.</summary>
    Initial = 0xf6a9,
    /// <summary>$B2:F6E4, chooses flinch, kick, jump, or claw attacks.</summary>
    Active = 0xf6e4,
    /// <summary>$B2:F817, rises during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftRising = 0xf817,
    /// <summary>$B2:F84C, falls during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftFalling = 0xf84c,
    /// <summary>$B2:F890, rises during a rightward post-to-post spin jump.</summary>
    SpinJumpRightRising = 0xf890,
    /// <summary>$B2:F8C5, falls during a rightward post-to-post spin jump.</summary>
    SpinJumpRightFalling = 0xf8c5,
    /// <summary>$B2:F909, waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick = 0xf909,
    /// <summary>$B2:F985, begins the leftward divekick jump.</summary>
    DivekickLeftJump = 0xf985,
    /// <summary>$B2:F9C1, performs the leftward collision-bearing dive.</summary>
    DivekickLeftDive = 0xf9c1,
    /// <summary>$B2:FA15, walks leftward back to the authored post.</summary>
    DivekickLeftWalkToPost = 0xfa15,
    /// <summary>$B2:FA59, begins the rightward divekick jump.</summary>
    DivekickRightJump = 0xfa59,
    /// <summary>$B2:FA95, performs the rightward collision-bearing dive.</summary>
    DivekickRightDive = 0xfa95,
    /// <summary>$B2:FAE9, walks rightward back to the authored post.</summary>
    DivekickRightWalkToPost = 0xfae9,
}

/// <summary>
/// Debugger-facing projection of one ninja Pirate. Variables A-F remain backed by the
/// ordinary enemy record. The final three properties represent the otherwise-unaliased
/// bank-$7E:7800 scratch words used for current speed, a calculated dive target, and spawn Y.
/// </summary>
public sealed class NinjaSpacePirateEnemyState
{
    /// <summary>Enemy slot that owns the native variables projected by this debugger-facing state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the state view for a ninja Pirate's existing enemy record.</summary>
    /// <param name="slot">Enemy slot whose variables A-F back the exposed state properties.</param>
    internal NinjaSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets the current bank-$B2 movement/decision function pointer.</summary>
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

    /// <summary>Gets the horizontal midpoint of the adjusted post pair.</summary>
    public ushort PostsMidpointX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Gets the adjusted left-post horizontal position in room pixels.</summary>
    public ushort LeftPostX
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Gets the adjusted right-post horizontal position in room pixels.</summary>
    public ushort RightPostX
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Gets the current native subpixel speed accumulator.</summary>
    public ushort Speed { get; internal set; }
    /// <summary>Gets the horizontal target calculated for the current divekick.</summary>
    public ushort DiveTargetX { get; internal set; }
    /// <summary>Gets the vertical spawn position used as the jump baseline.</summary>
    public ushort SpawnY { get; internal set; }
    /// <summary>Gets the host diagnostic count of claw projectiles requested by this actor.</summary>
    public int SpawnedClawCount { get; internal set; }
    /// <summary>Gets the host diagnostic count of landing-dust effects requested by this actor.</summary>
    public int LandingDustCount { get; internal set; }
}

/// <summary>
/// Complete translation of ninja Space Pirates $F4D3/$F513/$F553/$F593/$F5D3/$F613:
/// initialization, cartridge animation bytecode, proximity decisions, flinch/kick/claw
/// attacks, both post-to-post jump forms, room collision, landing effects, and claw actors.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy definition pointer for the grey ninja Space Pirate.</summary>
    internal const ushort GreyNinjaSpacePirateDefinition = 0xf4d3;
    /// <summary>Native enemy definition pointer for the green ninja Space Pirate.</summary>
    internal const ushort GreenNinjaSpacePirateDefinition = 0xf513;
    /// <summary>Native enemy definition pointer for the red ninja Space Pirate.</summary>
    internal const ushort RedNinjaSpacePirateDefinition = 0xf553;
    /// <summary>Native enemy definition pointer for the gold ninja Space Pirate.</summary>
    internal const ushort GoldNinjaSpacePirateDefinition = 0xf593;
    /// <summary>Native enemy definition pointer for the magenta ninja Space Pirate.</summary>
    internal const ushort MagentaNinjaSpacePirateDefinition = 0xf5d3;
    /// <summary>Native enemy definition pointer for the silver ninja Space Pirate.</summary>
    internal const ushort SilverNinjaSpacePirateDefinition = 0xf613;
    /// <summary>Initial 8.8 speed assigned to the vertical portion of a divekick.</summary>
    private const ushort NinjaPirateInitialDiveSpeed = 0x0600;
    /// <summary>Horizontal distance from Samus at which an idle pirate activates.</summary>
    private const int NinjaPirateActivationDistance = 128;
    /// <summary>Per-axis proximity threshold for projectile-triggered flinch behavior.</summary>
    private const int NinjaPirateFlinchDistance = 32;
    /// <summary>Distance from the posts' midpoint that enables spin-jump decisions.</summary>
    private const int NinjaPirateMidpointTriggerDistance = 32;
    /// <summary>Per-axis proximity threshold for a standing kick.</summary>
    private const int NinjaPirateKickDistance = 40;
    /// <summary>Horizontal displacement per update during the collision-bearing dive.</summary>
    private const int NinjaPirateDiveHorizontalPixels = 5;
    /// <summary>Horizontal displacement per update while walking back to a post.</summary>
    private const int NinjaPirateReturnWalkPixels = 2;
    /// <summary>Vertical displacement per update during either half of a post-to-post spin jump.</summary>
    private const int NinjaPirateSpinVerticalPixels = 2;

    /// <summary>Per-slot debugger state views for initialized ninja Space Pirates.</summary>
    private readonly NinjaSpacePirateEnemyState?[] _ninjaSpacePirateStates =
        new NinjaSpacePirateEnemyState?[MaximumEnemyCount];

    /// <summary>Tests whether an enemy definition belongs to one of the six compiled ninja Pirate variants.</summary>
    /// <param name="definition">Native enemy definition pointer.</param><returns>True for a supported ninja Space Pirate definition.</returns>
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
            ? NinjaSpacePirateInstructionProgramDefinitions.InitialFacingRight
            : NinjaSpacePirateInstructionProgramDefinitions.InitialFacingLeft;
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

        // Native copies the gold non-ninja Pirate's color image, not this actor's initial
        // room-graphics palette. Share that installed image so the same authored edit
        // affects both consumers without changing the ninja's animation or slot state.
        (TileArtwork ?? throw new InvalidOperationException(
            "Gold Space Pirate requires installed palette artwork."))
            .LoadPaletteTo(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateDefinition,
                _cgram!, NinjaSpacePiratePaletteDefinitions.TargetColor);
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

    /// <summary>Waits for Samus to enter activation range, then installs the facing-specific active instruction list.</summary>
    /// <param name="slot">Enemy slot whose function and instruction state are updated.</param><param name="state">Post positions and active-list state initialized for this pirate.</param><param name="samus">Current Samus position used by the activation-distance check.</param><param name="samusProjectiles">Optional projectile slots checked for a flinch while Samus remains out of range.</param>
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
            ? NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingRight
            : NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft;
        state.ActiveInstruction = active;
        InstallNinjaPirateInstruction(slot, active);
    }

    /// <summary>Runs active attack selection in native priority order: projectile flinch, standing kick, spin jump, then claw.</summary>
    /// <param name="slot">Active enemy slot.</param><param name="state">Post geometry and movement state for the pirate.</param><param name="samus">Current Samus position and pose.</param><param name="samusProjectiles">Optional projectile set used by the flinch check.</param>
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

    /// <summary>Checks the highest occupied projectile slot and installs a facing flinch list when it is close on both axes.</summary>
    /// <param name="slot">Pirate slot whose position determines projectile distance.</param><param name="samus">Samus position used to select the flinch facing.</param><param name="samusProjectiles">Projectile slots to inspect, or null when projectile checks are unavailable.</param>
    /// <returns>True when a nearby projectile caused the flinch list to be installed.</returns>
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
            // Native `$B2:F72E` selects by nonzero projectile type, not by bank-$93's
            // animation-list pointer. This includes the invisible zero-list Murder Beam.
            if (samusProjectiles.Slots[index].HasEnemyCollisionPayload)
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
                ? NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingRight
                : NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingLeft);
        return true;
    }

    /// <summary>Installs the appropriate standing-kick list when Samus is within the native horizontal and vertical thresholds.</summary>
    /// <param name="slot">Pirate slot whose position determines range and facing.</param><param name="samus">Actor position tested against the pirate.</param>
    /// <returns>True when a kick instruction list was installed.</returns>
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
                ? NinjaSpacePirateInstructionProgramDefinitions.KickFacingRight
                : NinjaSpacePirateInstructionProgramDefinitions.KickFacingLeft);
        return true;
    }

    /// <summary>Starts a post-to-post spin jump when Samus is close to the midpoint between the adjusted posts.</summary>
    /// <param name="slot">Pirate slot whose current post selects the jump direction.</param><param name="state">Adjusted post positions and midpoint.</param><param name="samus">Actor position checked against the midpoint trigger.</param>
    /// <returns>True when a spin-jump instruction list was installed.</returns>
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
                ? NinjaSpacePirateInstructionProgramDefinitions.SpinJumpRight
                : NinjaSpacePirateInstructionProgramDefinitions.SpinJumpLeft);
        return true;
    }

    /// <summary>Periodically throws a claw toward Samus when the pirate faces her from its current post.</summary>
    /// <param name="slot">Pirate slot supplying position, facing, and the native frame cadence.</param><param name="state">State receiving the successful claw-spawn diagnostic count.</param><param name="samus">Target position used to decide whether the actor is facing Samus.</param>
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
            InstallNinjaPirateInstruction(
                slot,
                NinjaSpacePirateInstructionProgramDefinitions.ClawAttackLeft);
            return;
        }

        if (unchecked((short)(slot.XPosition - samus.XPosition)) >= 0)
            return;
        InstallNinjaPirateInstruction(
            slot,
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackRight);
    }

    /// <summary>Checks interruption attacks, then uses the native random retry and midpoint gate to begin a divekick jump.</summary>
    /// <param name="slot">Pirate slot selecting the direction of return travel.</param><param name="state">Post geometry and dive state.</param><param name="samus">Current actor position.</param><param name="samusProjectiles">Optional projectile source for flinch priority.</param>
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
                ? NinjaSpacePirateInstructionProgramDefinitions.DivekickRightJump
                : NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftJump);
    }

    /// <summary>Advances one spin-jump update and installs the landing list after speed returns to zero.</summary>
    /// <param name="slot">Enemy slot whose room position advances.</param><param name="state">Speed, midpoint, and adjusted post coordinates.</param><param name="movingRight">Selects horizontal direction across the posts.</param><param name="rising">Selects ascent and midpoint crossing versus descent and landing.</param>
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
            movingRight
                ? NinjaSpacePirateInstructionProgramDefinitions.LandFacingRight
                : NinjaSpacePirateInstructionProgramDefinitions.LandFacingLeft);
        SpawnNinjaPirateLandingDust(slot, state);
    }

    /// <summary>Sets the initial vertical dive speed and computes a target halfway from the chosen post to the midpoint.</summary>
    /// <param name="state">State receiving speed and target X.</param><param name="movingRight">Chooses the left-to-midpoint or midpoint-to-right target interval.</param>
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

    /// <summary>Raises the pirate while reducing jump speed, then switches to the collision-bearing dive at the jump apex.</summary>
    /// <param name="slot">Enemy slot whose Y position and instruction list are changed.</param><param name="state">Dive speed and function state.</param><param name="movingRight">Selects which directional dive follows the jump.</param>
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
            movingRight
                ? NinjaSpacePirateInstructionProgramDefinitions.DivekickRightDive
                : NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftDive);
        state.Speed = NinjaPirateInitialDiveSpeed;
    }

    /// <summary>Advances the horizontal and collision-aware vertical dive until landing starts the walk back to its post.</summary>
    /// <param name="slot">Enemy slot being moved.</param><param name="state">Dive speed, baseline Y, and movement state.</param><param name="level">Room collision layer used to detect landing.</param><param name="movingRight">Selects horizontal dive and return direction.</param>
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
            movingRight
                ? NinjaSpacePirateInstructionProgramDefinitions.WalkToRightPost
                : NinjaSpacePirateInstructionProgramDefinitions.WalkToLeftPost);
        slot.YPosition = state.SpawnY;
        slot.YSubposition = 0;
        SpawnNinjaPirateLandingDust(slot, state);
    }

    /// <summary>Walks the landed pirate toward its authored post, clamps at the post, and restores the landing animation.</summary>
    /// <param name="slot">Enemy slot whose X position advances.</param><param name="state">Adjusted post coordinates.</param><param name="movingRight">Selects the destination post and walking direction.</param>
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
            movingRight
                ? NinjaSpacePirateInstructionProgramDefinitions.LandFacingRight
                : NinjaSpacePirateInstructionProgramDefinitions.LandFacingLeft);
        state.Function = NinjaSpacePirateFunction.NoOperation;
    }

    /// <summary>Requests the two dust sprites placed on either side of the pirate's feet after landing.</summary>
    /// <param name="slot">Landed pirate position anchoring the effects.</param><param name="state">State whose diagnostic count tracks successfully allocated dust objects.</param>
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

    /// <summary>Allocates and initializes one native claw projectile using the requested throw direction and spawn offsets.</summary>
    /// <param name="source">Pirate slot supplying position, palette, and VRAM tile selection.</param><param name="state">State whose spawn count increments when allocation succeeds.</param><param name="direction">Native direction parameter for the claw actor.</param><param name="xOffset">Signed horizontal spawn offset encoded as a word.</param><param name="yOffset">Signed vertical spawn offset encoded as a word.</param>
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
            ? SpacePirateProjectileInstructionProgramDefinitions.ClawLeft
            : SpacePirateProjectileInstructionProgramDefinitions.ClawRight;
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
        switch (opcode)
        {
            case SpacePirateInstructionCodes.Instruction_PirateWall_FunctionInY:
                state.Function = (NinjaSpacePirateFunction)ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_PaletteIndexInY:
                slot.PaletteIndex = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_QueueSoundInY_Lib2_Max6:
                LastSpacePirateSoundEffect = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset:
                SpawnNinjaPirateClaw(
                    slot,
                    state,
                    ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2))),
                    ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 4))),
                    ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 6))));
                cursor = unchecked((ushort)(cursor + 8));
                return true;
            case SpacePirateInstructionCodes.Instruction_PirateNinja_SetFunction0FAC_Active:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ninja Space Pirate facing selection requires the active Samus actor.");
                }
                state.ActiveInstruction = unchecked((short)(slot.XPosition - samus.XPosition)) < 0
                    ? NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingRight
                    : NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft;
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

    /// <summary>Installs an animation instruction list and arms its first instruction for the next update.</summary>
    /// <param name="slot">Enemy slot receiving the instruction pointer and timer.</param><param name="instructionPointer">Bank-relative instruction-list address.</param>
    private static void InstallNinjaPirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    /// <summary>Returns the initialized state view for this enemy slot or reports an invalid lifecycle call.</summary>
    /// <param name="slot">Enemy slot whose ninja Pirate state is required.</param><returns>The state stored at the slot's index.</returns>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as a ninja Space Pirate.</exception>
    private NinjaSpacePirateEnemyState RequireNinjaSpacePirateState(RoomEnemySlot slot) =>
        _ninjaSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized ninja Space Pirate state.");
}
