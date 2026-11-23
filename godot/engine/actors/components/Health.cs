namespace Engine.Actors.Components;

/// <summary>How much damage something can still take. It reports when it runs out; its owner decides what follows.</summary>
public sealed class Health
{
    /// <summary>Full health of <paramref name="maximum"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The maximum is not positive.</exception>
    public Health(int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximum);
        Maximum = maximum;
        Current = maximum;
    }

    /// <summary>The most it can hold.</summary>
    public int Maximum { get; }

    /// <summary>What is left, zero through <see cref="Maximum"/>.</summary>
    public int Current { get; private set; }

    /// <summary>Whether none is left.</summary>
    public bool IsEmpty => Current == 0;

    /// <summary>Whether <paramref name="damage"/> would leave none.</summary>
    public bool WouldBeEmptiedBy(int damage) => damage >= Current;

    /// <summary>Takes damage off, stopping at zero.</summary>
    public void Take(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);
        Current = Math.Max(0, Current - damage);
    }

    /// <summary>Gives health back, stopping at the maximum.</summary>
    public void Restore(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Current = Math.Min(Maximum, Current + amount);
    }
}
