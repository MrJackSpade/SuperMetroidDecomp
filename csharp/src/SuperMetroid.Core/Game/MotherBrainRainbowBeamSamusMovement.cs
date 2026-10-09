using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal translation of the four Samus-position helpers at <c>$A9:BBB5-$BC75</c>
/// used by Mother Brain's phase-two rainbow beam.
/// </summary>
/// <remarks>
/// This state belongs to Mother Brain, not to Samus's movement-type dispatcher. Pose type
/// <c>$1B</c> deliberately returns without moving at <c>$90:A7DA</c>; the enemy AI calls
/// these helpers separately after it has locked Samus. Keeping that ownership explicit
/// prevents a future runtime from accidentally applying both ordinary and forced movement.
///
/// The native helpers use a peculiar 8.8 velocity against only byte <c>+1</c> of Samus's
/// 16-bit subposition. The low subposition byte is untouched, the 8-bit addition's carry
/// enters the whole-pixel addition, and both current and previous positions are overwritten
/// with the result. This implementation spells out those byte operations instead of using
/// floating point or the ordinary 16.16 movement helpers.
/// </remarks>
public sealed class MotherBrainRainbowBeamSamusMovement
{
    // `$A0:B443` is the signed 8-bit sine table consumed by the shared bank-$86 velocity
    // component routine. Entries are 16-bit sign-extended values in the range -256..256.

    /// <summary>Mother Brain body extra word <c>$0FB2</c>, interpreted as signed 8.8.</summary>
    public ushort CustomXVelocity { get; private set; }

    /// <summary>Mother Brain body extra word <c>$0FB4</c>, interpreted as signed 8.8.</summary>
    public ushort CustomYVelocity { get; private set; }

    /// <summary>
    /// The low-byte beam angle stored by <c>$A9:BBA8-$BBAB</c>; zero points down and the
    /// positive direction is anti-clockwise, matching the shared bank-$86 trig routines.
    /// </summary>
    public SnesAngle RainbowBeamAngle { get; set; }

    /// <summary>
    /// Ports <c>$A9:BBB5</c>: move right at <c>$10.00</c> pixels per call and, until the
    /// hardcoded wall is reached, derive the vertical component from the live beam angle.
    /// </summary>
    public MotherBrainForcedSamusMovementResult MoveTowardWall(
        ISnesAddressSpace bus,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);
        bool reachedWall = MoveHorizontallyTowardWall(samus, velocity: 0x1000);
        ushort yVelocity = 0;
        bool reachedVerticalBoundary = false;

        if (!reachedWall)
        {
            // `$86:C272` adds $40 before indexing the sine table. Its multiplication returns
            // product bits 8..23, i.e. a signed 8.8 component for the supplied 8.8 speed.
            yVelocity = CalculateYVelocity(speed: 0x1000, RainbowBeamAngle);
            reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

            // `$A9:BBCD` unconditionally clears carry after the vertical helper. The caller
            // can therefore observe only the wall result; keep the clamp witness separately
            // for debugging without pretending it controls the native state transition.
        }

        return CreateResult(
            before,
            samus,
            xVelocity: 0x1000,
            yVelocity,
            reachedWall,
            reachedVerticalBoundary,
            nativeCarry: reachedWall);
    }

    /// <summary>
    /// Ports <c>$A9:BBCF</c>: move down by <c>$00.40</c> below/equal to Y <c>$7C</c>, or
    /// up by <c>-$00.40</c> above it. Native does not snap to the target and can oscillate.
    /// </summary>
    public static MotherBrainForcedSamusMovementResult MoveTowardMiddleOfWall(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);

        // `CPY SamusYPosition / BPL` tests signed(YTarget-currentY). For the fight's normal
        // coordinate range this is exactly currentY <= $7C, but preserve the 16-bit branch
        // semantics instead of introducing a host integer comparison with different wraps.
        ushort difference = unchecked((ushort)(0x007c - samus.YPosition));
        ushort yVelocity = unchecked((short)difference >= 0 ? (ushort)0x0040 : (ushort)0xffc0);
        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

        return CreateResult(
            before,
            samus,
            xVelocity: 0,
            yVelocity,
            reachedWall: false,
            reachedVerticalBoundary,
            nativeCarry: reachedVerticalBoundary);
    }

    /// <summary>
    /// Ports <c>$A9:BA82-$BA88</c>, executed when the rainbow beam has narrowed below its
    /// finishing threshold: initial left speed <c>-$01.00</c>, zero vertical speed.
    /// </summary>
    public void BeginFallingAfterRainbowBeam()
    {
        CustomXVelocity = 0xff00;
        CustomYVelocity = 0;
    }

    /// <summary>
    /// Ports <c>$A9:BBE1</c>: ease horizontal speed toward zero by <c>$00.02</c>, add
    /// <c>$00.18</c> vertical speed, then move through the two hardcoded arena clamps.
    /// </summary>
    public MotherBrainForcedSamusMovementResult StepFallingAfterRainbowBeam(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);

        // Native ADC wraps to 16 bits and BMI examines the resulting sign bit. As soon as
        // the negative speed crosses into $0000..$7FFF it is clamped to exact zero.
        ushort acceleratedX = unchecked((ushort)(CustomXVelocity + 0x0002));
        CustomXVelocity = unchecked((short)acceleratedX < 0 ? acceleratedX : (ushort)0);
        bool reachedWall = MoveHorizontallyTowardWall(samus, CustomXVelocity);

        CustomYVelocity = unchecked((ushort)(CustomYVelocity + 0x0018));
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, CustomYVelocity);

        // `$A9:BAD1` branches on the carry returned by the vertical helper. Horizontal wall
        // carry is intentionally discarded by the following LDA/ADC sequence.
        return CreateResult(
            before,
            samus,
            CustomXVelocity,
            CustomYVelocity,
            reachedWall,
            reachedVerticalBoundary,
            nativeCarry: reachedVerticalBoundary);
    }

    /// <summary>Derives the vertical 8.8 velocity component from beam angle and movement speed.</summary>
    /// <param name="speed">Unsigned 8.8 movement speed whose magnitude is projected onto the sine axis.</param>
    /// <param name="angle">Current low-byte beam angle used to select the perpendicular sine-table component.</param>
    /// <returns>The signed vertical component in the same 8.8 representation as <paramref name="speed"/>.</returns>
    private static ushort CalculateYVelocity(
        ushort speed,
        SnesAngle angle)
    {
        // `$86:C27A` multiplies unsigned speed by the magnitude, selects product bits 8..23,
        // then reapplies the table entry's sign with 16-bit two's-complement negation.
        return EnemyTrigonometryTables.MultiplySignedSine(
            speed, angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
    }

    /// <summary>Applies native vertical movement, clamps at the arena ceiling or floor, and synchronizes previous-position words.</summary>
    /// <param name="samus">Samus state whose current and previous vertical coordinates are updated.</param>
    /// <param name="velocity">Signed 8.8 vertical displacement applied for this call.</param>
    /// <returns><see langword="true"/> when the candidate position reaches either hardcoded vertical boundary.</returns>
    private static bool MoveVerticallyTowardCeilingOrFloor(
        SamusState samus,
        ushort velocity)
    {
        ushort candidate = AddNativeEightEightVelocity(
            samus.YPosition,
            samus.Kinematics.YSubposition,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.YSubposition = newSubposition;
        // `$A9:BC06` stores the same fraction byte into SamusPreviousYSubPosition+1, and every
        // exit stores the new Y into SamusPreviousYPosition: the camera sees no movement.
        samus.WritePreviousYSubposition(0xff00, newSubposition);

        // BMI/BPL are signed tests of the subtraction result, not unsigned C# comparisons.
        // This distinction is observable if a malformed/debug position wraps around zero.
        ushort? boundary = unchecked((short)(candidate - 0x0030)) < 0 ? (ushort)0x0030
            : unchecked((short)(candidate - 0x00c0)) >= 0 ? (ushort)0x00c0
            : null;
        if (boundary is ushort clamped)
        {
            samus.YPosition = clamped;
            samus.Kinematics.YSubposition = 0;
            samus.WritePreviousYPosition(clamped);
            samus.WritePreviousYSubposition(0xffff, 0);
            return true;
        }

        samus.YPosition = candidate;
        samus.WritePreviousYPosition(candidate);
        return false;
    }

    /// <summary>Applies native horizontal movement and clamps Samus at the rainbow-beam arena wall.</summary>
    /// <param name="samus">Samus state whose current and previous horizontal coordinates are updated.</param>
    /// <param name="velocity">16-bit 8.8 horizontal displacement; its high byte is interpreted as signed.</param>
    /// <returns><see langword="true"/> when the candidate position reaches or passes the wall at X <c>$00EB</c>.</returns>
    private static bool MoveHorizontallyTowardWall(SamusState samus, ushort velocity)
    {
        ushort candidate = AddNativeEightEightVelocity(
            samus.XPosition,
            samus.Kinematics.XSubposition,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.XSubposition = newSubposition;
        // `$A9:BC48/$BC61/$BC6B/$BC71` mirror every store into the previous-X words.
        samus.WritePreviousXSubposition(0xff00, newSubposition);

        if (unchecked((short)(candidate - 0x00eb)) >= 0)
        {
            samus.XPosition = 0x00eb;
            samus.Kinematics.XSubposition = 0;
            samus.WritePreviousXPosition(0x00eb);
            samus.WritePreviousXSubposition(0xffff, 0);
            return true;
        }

        samus.XPosition = candidate;
        samus.WritePreviousXPosition(candidate);
        return false;
    }

    /// <summary>Reproduces the native byte-wise 8.8 addition while preserving the untouched low subposition byte.</summary>
    /// <param name="wholePosition">Current whole-pixel coordinate.</param>
    /// <param name="subposition">Current 16-bit fractional coordinate; only its high byte participates in movement.</param>
    /// <param name="velocity">Signed 8.8 displacement encoded as a 16-bit word.</param>
    /// <param name="newSubposition">Receives the updated fraction with its low byte copied unchanged from <paramref name="subposition"/>.</param>
    /// <returns>The wrapped 16-bit whole-pixel coordinate after applying velocity and fractional carry.</returns>
    private static ushort AddNativeEightEightVelocity(
        ushort wholePosition,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // SEP #$20 leaves the velocity's high byte in B while the low accumulator adds only
        // to WRAM subposition byte +1. Byte +0 is never read or written by `$A9:BBFD/BC3F`.
        int fractionalSum = (subposition >> 8) + (velocity & 0x00ff);
        byte newHighFraction = unchecked((byte)fractionalSum);
        newSubposition = unchecked((ushort)((newHighFraction << 8) | (subposition & 0x00ff)));

        // REP #$20, AND #$FF00, XBA, sign extension reconstruct the signed whole byte. The
        // carry left by the 8-bit ADC is still set and participates in the final ADC.
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        return unchecked((ushort)(wholePosition + wholeDelta));
    }

    /// <summary>Captures the four Samus position words that the forced-movement result reports as its starting point.</summary>
    /// <param name="samus">State from which whole and fractional X/Y coordinates are read.</param>
    /// <returns>A snapshot of Samus's current camera-space position words.</returns>
    private static SamusCameraPoint Capture(SamusState samus) => new(
        samus.XPosition,
        samus.Kinematics.XSubposition,
        samus.YPosition,
        samus.Kinematics.YSubposition);

    /// <summary>Builds the per-call witness returned by the public forced-movement operations.</summary>
    /// <param name="before">Position snapshot captured before this movement operation.</param>
    /// <param name="samus">Samus state after the operation, used to capture the resulting position.</param>
    /// <param name="xVelocity">Horizontal component supplied for this helper call; it does not affect the returned witness.</param>
    /// <param name="yVelocity">Vertical component supplied for this helper call; it does not affect the returned witness.</param>
    /// <param name="reachedWall">Wall-clamp outcome supplied by the caller; it does not affect the returned witness.</param>
    /// <param name="reachedVerticalBoundary">Vertical-clamp outcome supplied by the caller; it does not affect the returned witness.</param>
    /// <param name="nativeCarry">Carry state selected by the native helper sequence and stored in the witness.</param>
    /// <returns>The starting position and selected native carry outcome.</returns>
    private static MotherBrainForcedSamusMovementResult CreateResult(
        SamusCameraPoint before,
        SamusState samus,
        ushort xVelocity,
        ushort yVelocity,
        bool reachedWall,
        bool reachedVerticalBoundary,
        bool nativeCarry) => new(
            before,
            nativeCarry);
}

/// <summary>One-call witness for Mother Brain's forced Samus coordinate helpers.</summary>
/// <param name="Before">Samus camera-space position immediately before the forced movement step.</param>
/// <param name="NativeCarry">Carry flag outcome observed by the native caller; its meaning depends on the selected helper path.</param>
public readonly record struct MotherBrainForcedSamusMovementResult(
    SamusCameraPoint Before,
    bool NativeCarry);
