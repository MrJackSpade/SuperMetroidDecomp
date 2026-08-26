using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The room-FX words and derived liquid state consulted by Samus's bank-$90/$91 physics.
/// </summary>
/// <remarks>
/// Super Metroid does not have one universal "underwater" flag. Different native routines
/// compare either Samus's top, bottom, or bottom-minus-one boundary against the live FX
/// surface, and <c>$0AD2</c> remembers the medium selected by the animation/room-load pass.
/// Keeping the literal source words here lets each translated caller reproduce its own
/// comparison instead of sharing a convenient but incorrect host-side boolean.
/// </remarks>
public sealed class SamusLiquidPhysicsState
{
    /// <summary>No liquid physics are active at the sampled Samus boundary.</summary>
    public const ushort Air = 0;

    /// <summary>Water physics, the native value stored at WRAM <c>$0AD2</c>.</summary>
    public const ushort Water = 1;

    /// <summary>Lava/acid physics, the native value stored at WRAM <c>$0AD2</c>.</summary>
    public const ushort LavaAcid = 2;

    /// <summary>Gravity Suit equipment bit in WRAM <c>$09A2</c>.</summary>
    public const ushort GravitySuitItem = 0x0020;

    /// <summary>
    /// FX type word at WRAM <c>$196E</c>. Only its low nibble is dispatched: values two and
    /// four are lava/acid, while six is water. A default value of zero is the no-FX handler.
    /// </summary>
    public ushort FxType { get; set; }

    /// <summary>
    /// General FX surface Y at WRAM <c>$195E</c>. A negative 16-bit value tells movement
    /// routines to consult <see cref="LavaAcidYPosition"/> instead of the water surface.
    /// </summary>
    public ushort FxYPosition { get; set; } = ushort.MaxValue;

    /// <summary>Lava/acid surface Y at WRAM <c>$1962</c>; negative means absent.</summary>
    public ushort LavaAcidYPosition { get; set; } = ushort.MaxValue;

    /// <summary>
    /// Liquid-options word at WRAM <c>$197E</c>. Bit two disables water interaction even
    /// when the geometric surface comparison says Samus is below it.
    /// </summary>
    public ushort LiquidOptions { get; set; }

    /// <summary>
    /// Remembered native medium at WRAM <c>$0AD2</c>. This is intentionally stateful:
    /// Space Jump reads it, and the animation handlers use changes to detect entry/exit.
    /// </summary>
    public ushort LiquidPhysicsType { get; private set; }

    /// <summary>Configures the exact room-FX words for an ordinary water surface.</summary>
    public void ConfigureWater(ushort surfaceY, ushort liquidOptions = 0)
    {
        FxType = 6;
        FxYPosition = surfaceY;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = liquidOptions;
    }

    /// <summary>Configures the exact room-FX words for a lava (type two) or acid (type four) surface.</summary>
    public void ConfigureLavaAcid(ushort surfaceY, bool acid = false)
    {
        FxType = acid ? (ushort)4 : (ushort)2;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = surfaceY;
        LiquidOptions = 0;
    }

    /// <summary>Restores the no-FX sentinel state used by dry rooms.</summary>
    public void Clear()
    {
        FxType = 0;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = 0;
        LiquidPhysicsType = Air;
    }

    /// <summary>
    /// Reproduces <c>SetLiquidPhysicsType</c> at <c>$90:8E0F</c>. Unlike movement-table
    /// selection, this room-load helper dispatches on FX type and does not exempt Gravity
    /// Suit; animation later suppresses the suit's delay while retaining the medium word.
    /// </summary>
    public void InitializeRememberedMedium(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ushort bottom = samus.Kinematics.BottomBoundary;
        LiquidPhysicsType = ((FxType & 0x000f) >> 1) switch
        {
            1 or 2 when IsBelowSurface(LavaAcidYPosition, bottom) => LavaAcid,
            3 when WaterAffectsBoundary(bottom) => Water,
            _ => Air,
        };
    }

    /// <summary>
    /// Selects air/water/lava physics for routines that test Samus's bottom boundary and
    /// bypass all liquid behavior when Gravity Suit bit <c>$0020</c> is equipped.
    /// </summary>
    public ushort DetermineMovementMedium(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if ((samus.EquippedItems & GravitySuitItem) != 0)
            return Air;
        return DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary);
    }

    /// <summary>
    /// True when the spin routine's top-boundary test at <c>$90:A449-$A469</c> finds Samus
    /// fully under water/lava. The caller performs the Gravity-Suit palette-bit exemption.
    /// </summary>
    public bool IsTopBoundarySubmerged(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.TopBoundary) != Air;
    }

    /// <summary>
    /// Reproduces the liquid-flag update at <c>$9B:C4BE-$C4EA</c>, which runs after the
    /// current grapple function and therefore affects the following grapple frame.
    /// </summary>
    /// <remarks>
    /// This test is intentionally not the ordinary movement-medium selector. Native grapple
    /// checks the suit-palette Gravity bit, requires a nonzero FX type, and compares only the
    /// general FX Y word against Samus's bottom. It does not consult liquid-options bit two
    /// and never falls back to the lava/acid Y word. Those quirks are retained rather than
    /// replacing the cartridge's single flag with a friendlier host notion of “submerged.”
    /// </remarks>
    public bool DetermineGrappleSubmersion(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        return (samus.EquippedItems & GravitySuitItem) == 0 &&
            FxType != 0 &&
            IsBelowSurface(FxYPosition, samus.Kinematics.BottomBoundary);
    }

    /// <summary>
    /// Returns the extra animation delay used by <c>$91:FB08</c> while installing a changed
    /// pose. That routine samples <c>Y + radius - 1</c>, not the normal bottom boundary.
    /// </summary>
    public ushort DeterminePoseChangeAnimationBuffer(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if ((samus.EquippedItems & GravitySuitItem) != 0)
            return samus.XSpeedDivisor;

        ushort bottomMinusOne = unchecked((ushort)(
            samus.Kinematics.YPosition + samus.Kinematics.YRadius - 1));
        return DetermineRawMediumAtBoundary(bottomMinusOne) switch
        {
            Water => 3,
            LavaAcid => 2,
            _ => samus.XSpeedDivisor,
        };
    }

    /// <summary>
    /// Runs the movement-visible half of <c>Samus_Animate</c>'s FX dispatch. It publishes
    /// the exact water/lava animation buffer and maintains `$0AD2`; particles, sounds, and
    /// periodic damage remain separate presentation/combat work and are not fabricated.
    /// </summary>
    public void PrepareAnimationFrame(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ushort bottom = samus.Kinematics.BottomBoundary;
        int handler = (FxType & 0x000f) >> 1;
        bool gravitySuit = (samus.EquippedItems & GravitySuitItem) != 0;

        if (handler == 3 && WaterAffectsBoundary(bottom))
        {
            // `$90:80B8` first writes three and establishes medium one. Its bubble helper
            // then clears the delay buffer for Gravity Suit without clearing `$0AD2`.
            LiquidPhysicsType = Water;
            samus.AnimationFrameBuffer = gravitySuit ? (ushort)0 : (ushort)3;
            return;
        }

        if ((handler == 1 || handler == 2) && IsBelowSurface(LavaAcidYPosition, bottom))
        {
            if (handler == 1 && samus.HorizontalSpeed.SpeedBoostCounter != 0)
            {
                // Lava alone executes `$90:81C9-$81D5` before the Gravity-Suit branch.
                // Cancel_SpeedBoosting owns the momentum/counter/palette/echo transition;
                // the two extra-run words are then cleared explicitly by the FX handler.
                // Acid deliberately skips all three writes.
                samus.HorizontalSpeed.CancelRunningMomentum(samus.ReadPoseXDirection(bus));
                samus.HorizontalSpeed.ExtraRunSpeed = 0;
                samus.HorizontalSpeed.ExtraRunSubspeed = 0;
            }

            // Both lava and acid share `$90:824C`'s delay-two submerged animation path.
            LiquidPhysicsType = LavaAcid;
            samus.AnimationFrameBuffer = gravitySuit ? (ushort)0 : (ushort)2;
            return;
        }

        // `$90:8078` publishes the X-speed divisor and clears a remembered medium after
        // leaving its surface. Splash/audio side effects are deliberately not hidden here.
        samus.AnimationFrameBuffer = samus.XSpeedDivisor;
        LiquidPhysicsType = Air;
    }

    /// <summary>
    /// Native movement routines distinguish water from lava by the sign of `$195E`, then
    /// perform signed 16-bit subtraction against the selected surface. Retain that exact
    /// ordering so sentinel and wraparound behavior stay debugger-visible.
    /// </summary>
    private ushort DetermineRawMediumAtBoundary(ushort boundary)
    {
        if (unchecked((short)FxYPosition) >= 0)
            return WaterAffectsBoundary(boundary) ? Water : Air;
        return IsBelowSurface(LavaAcidYPosition, boundary) ? LavaAcid : Air;
    }

    private bool WaterAffectsBoundary(ushort boundary) =>
        (LiquidOptions & 0x0004) == 0 && IsBelowSurface(FxYPosition, boundary);

    private static bool IsBelowSurface(ushort surface, ushort boundary) =>
        unchecked((short)surface) >= 0 &&
        unchecked((short)(surface - boundary)) < 0;
}
