namespace Engine.Shared;

/// <summary>
/// One number drawn from the gameplay random generator, from which several things can be settled, each by its own
/// part of the number, without drawing again.
/// </summary>
/// <param name="Number">The number drawn.</param>
public readonly record struct RandomDraw(uint Number)
{
    /// <summary>
    /// The value of <paramref name="width"/> binary digits of the number, starting <paramref name="from"/> digits up
    /// from its lowest: 0 up to, not including, two to the power of the width.
    /// </summary>
    public int Part(int from, int width)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(from);
        if (width < 1 || from + width > 32)
            throw new ArgumentOutOfRangeException(nameof(width), width, "A part of a draw is 1 or more digits that lie inside the number.");
        return (int)((Number >> from) & ((1u << width) - 1));
    }

    /// <summary>A number from 0 up to, not including, <paramref name="count"/>, from the whole draw.</summary>
    public int Below(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        return (int)(Number % (uint)count);
    }
}
