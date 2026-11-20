namespace Engine.Shared;

/// <summary>
/// A count that wraps: stepped past its last value it continues from zero, and stepped below zero it continues from
/// its last value. For the cases where the wrap is something the player can observe; an ordinary count is an int.
/// </summary>
public readonly record struct WrappingCounter
{
    /// <summary>A counter holding <paramref name="value"/> out of <paramref name="size"/> values, 0 through size - 1.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The size is not positive, or the value is not one of its values.</exception>
    public WrappingCounter(int value, int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (value < 0 || value >= size)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"A counter of {size} values holds 0 through {size - 1}.");
        Value = value;
        Size = size;
    }

    /// <summary>The count, 0 through <see cref="Size"/> - 1.</summary>
    public int Value { get; }

    /// <summary>How many values the counter has before it wraps.</summary>
    public int Size { get; }

    /// <summary>The counter moved on by <paramref name="amount"/>, which may be negative, wrapping at either end.</summary>
    public WrappingCounter Plus(int amount)
    {
        long moved = ((long)Value + amount) % Size;
        if (moved < 0)
            moved += Size;
        return new((int)moved, Size);
    }
}
