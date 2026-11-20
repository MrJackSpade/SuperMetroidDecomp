namespace Engine.Shared;

/// <summary>A timer measured in updates. It counts down to zero and stays there.</summary>
public readonly record struct Countdown
{
    private Countdown(int remaining) => Remaining = remaining;

    /// <summary>A countdown that has already ended.</summary>
    public static Countdown Over => default;

    /// <summary>A countdown that ends after the given number of updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number of updates is negative.</exception>
    public static Countdown Of(int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        return new(updates);
    }

    /// <summary>The updates left before it ends.</summary>
    public int Remaining { get; }

    /// <summary>Whether updates are left.</summary>
    public bool IsRunning => Remaining > 0;

    /// <summary>Whether it has ended.</summary>
    public bool IsOver => Remaining == 0;

    /// <summary>The countdown one update later. One that is over stays over.</summary>
    public Countdown Stepped() => IsOver ? this : new(Remaining - 1);
}
