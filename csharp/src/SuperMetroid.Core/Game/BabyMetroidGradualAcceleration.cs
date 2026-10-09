namespace SuperMetroid.Core.Game;

/// <summary>
/// <c>GradduallyAccelerateTowardsPoint</c> ($A9:F46B) and <c>GraduallyAccelerateHorizontally</c>
/// ($A9:F4E6), shared by the Tourian Shitroid and the Mother Brain baby-Metroid cutscene.
/// The native arithmetic is an ADC/SBC chain whose carry is never normalised: the
/// position subtraction's carry reaches the step taken while already moving toward the
/// target, and each reversal step borrows from the one before. Both axes are reproduced
/// as that chain rather than as signed integer arithmetic.
/// </summary>
internal static class BabyMetroidGradualAcceleration
{
    /// <summary>Unsigned acceleration added to velocity when motion reverses toward the target.</summary>
    private const ushort ReversalStep = 0x0008;
    /// <summary>Positive and negative horizontal velocity clamp magnitude used by the native X-axis routine.</summary>
    private const ushort MaxXSpeed = 0x0800;
    /// <summary>Positive and negative vertical velocity clamp magnitude used by the native Y-axis routine.</summary>
    private const ushort MaxYSpeed = 0x0500;

    /// <summary>
    /// Returns the new X velocity. <paramref name="isVaguelyOffScreen"/> runs only on the
    /// reversal path, as <c>CheckIfEnemyIsVagulyOnScreen</c> does natively.
    /// </summary>
    public static ushort AccelerateHorizontally(
        ushort position,
        ushort target,
        ushort velocity,
        byte divisor,
        ushort wrongWayOffScreenSpeed,
        Func<bool> isVaguelyOffScreen)
    {
        ArgumentNullException.ThrowIfNull(isVaguelyOffScreen);
        bool carry = true;
        ushort distance = Sbc(position, target, ref carry);
        if (distance == 0)
            return velocity;
        if (IsNegative(distance))
        {
            ushort step = Step(unchecked((ushort)-distance), divisor);
            if (IsNegative(velocity))
            {
                // A carry-set off-screen result makes this ADC add one extra unit.
                carry = isVaguelyOffScreen();
                if (carry)
                    velocity = Adc(velocity, wrongWayOffScreenSpeed, ref carry);
                carry = false;
                velocity = Adc(velocity, ReversalStep, ref carry);
                velocity = Adc(velocity, step, ref carry);
            }
            velocity = Adc(velocity, step, ref carry);
            return IsNegative(unchecked((ushort)(velocity - MaxXSpeed))) ? velocity : MaxXSpeed;
        }
        else
        {
            ushort step = Step(distance, divisor);
            if (!IsNegative(velocity))
            {
                carry = isVaguelyOffScreen();
                if (carry)
                    velocity = Sbc(velocity, wrongWayOffScreenSpeed, ref carry);
                carry = true;
                velocity = Sbc(velocity, ReversalStep, ref carry);
                velocity = Sbc(velocity, step, ref carry);
            }
            velocity = Sbc(velocity, step, ref carry);
            ushort limit = unchecked((ushort)-MaxXSpeed);
            return IsNegative(unchecked((ushort)(velocity - limit))) ? limit : velocity;
        }
    }

    /// <summary>Returns the new Y velocity (the vertical half of $A9:F46B).</summary>
    public static ushort AccelerateVertically(ushort position, ushort target, ushort velocity, byte divisor)
    {
        bool carry = true;
        ushort distance = Sbc(position, target, ref carry);
        if (distance == 0)
            return velocity;
        if (IsNegative(distance))
        {
            ushort step = Step(unchecked((ushort)-distance), divisor);
            if (IsNegative(velocity))
            {
                carry = false;
                velocity = Adc(velocity, ReversalStep, ref carry);
                velocity = Adc(velocity, step, ref carry);
            }
            velocity = Adc(velocity, step, ref carry);
            return IsNegative(unchecked((ushort)(velocity - MaxYSpeed))) ? velocity : MaxYSpeed;
        }
        else
        {
            ushort step = Step(distance, divisor);
            if (!IsNegative(velocity))
            {
                carry = true;
                velocity = Sbc(velocity, ReversalStep, ref carry);
                velocity = Sbc(velocity, step, ref carry);
            }
            velocity = Sbc(velocity, step, ref carry);
            ushort limit = unchecked((ushort)-MaxYSpeed);
            return IsNegative(unchecked((ushort)(velocity - limit))) ? limit : velocity;
        }
    }

    // $4204/$4206 divide the distance by the 8-bit divisor; a zero quotient becomes 1.
    /// <summary>Computes the per-update acceleration from distance and the native divisor, with a minimum step of one.</summary>
    /// <param name="distance">Unsigned distance remaining along the selected axis.</param>
    /// <param name="divisor">Nonzero divisor from the cutscene's acceleration table.</param>
    /// <returns>The truncated quotient, or one when the quotient would be zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The divisor is zero.</exception>
    private static ushort Step(ushort distance, byte divisor)
    {
        if (divisor == 0)
            throw new ArgumentOutOfRangeException(nameof(divisor), "The divisor table holds $10..$01.");
        ushort quotient = (ushort)(distance / divisor);
        return quotient == 0 ? (ushort)1 : quotient;
    }

    /// <summary>Tests the high bit used as the sign indicator by the emulated 16-bit arithmetic.</summary>
    /// <param name="value">Wrapped 16-bit result to inspect.</param>
    /// <returns>True when bit 15 is set.</returns>
    private static bool IsNegative(ushort value) => (value & 0x8000) != 0;

    /// <summary>Adds two wrapped 16-bit values and the incoming carry, updating carry from unsigned overflow.</summary>
    /// <param name="value">Accumulator value.</param>
    /// <param name="addend">Value added to the accumulator.</param>
    /// <param name="carry">Incoming carry and outgoing overflow flag.</param>
    /// <returns>The low 16 bits of the sum.</returns>
    private static ushort Adc(ushort value, ushort addend, ref bool carry)
    {
        int sum = value + addend + (carry ? 1 : 0);
        carry = sum > 0xffff;
        return unchecked((ushort)sum);
    }

    /// <summary>Subtracts with the processor's inverted borrow convention and stores whether the unsigned subtraction did not borrow.</summary>
    /// <param name="value">Accumulator value.</param>
    /// <param name="subtrahend">Value subtracted from the accumulator.</param>
    /// <param name="carry">Incoming no-borrow flag and outgoing no-borrow result.</param>
    /// <returns>The wrapped low 16 bits of the difference.</returns>
    private static ushort Sbc(ushort value, ushort subtrahend, ref bool carry)
    {
        int difference = value - subtrahend - (carry ? 0 : 1);
        carry = difference >= 0;
        return unchecked((ushort)difference);
    }
}
