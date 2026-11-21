namespace Fixture;

/// <summary>A type without a constructor whose field's initializer has an effect.</summary>
public sealed class Seeded
{
    private readonly int _seed = Environment.TickCount;
}

/// <summary>A type whose expression-bodied constructor gets a body for a kept initializer.</summary>
public sealed class Bodied
{
    private readonly int _seed = Environment.TickCount;

    /// <summary>Creates it at <paramref name="level"/>.</summary>
    /// <param name="level">The level.</param>
    public Bodied(int level) => Level = level;

    /// <summary>The level.</summary>
    public int Level { get; }
}

/// <summary>A type whose primary constructor has no body for a kept initializer.</summary>
/// <param name="level">The level.</param>
public sealed class Primary(int level)
{
    private readonly int _seed = Environment.TickCount;

    /// <summary>The level.</summary>
    public int Level => level;
}

/// <summary>A type writing its unread field through a receiver whose evaluation has effects.</summary>
public sealed class Receiver
{
    private int _calls;

    /// <summary>Writes through a new instance.</summary>
    public static void Touch() => Make()._calls = 5;

    private static Receiver Make() => new();
}

/// <summary>A type whose unread field's assignment is used with the field's type.</summary>
public sealed class Narrow
{
    private int _last;

    /// <summary>Stores <paramref name="value"/> and returns it widened.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The value.</returns>
    public long Store(byte value) => _last = value;
}

/// <summary>A type whose virtual method's parameter only fed the unread field.</summary>
public class Virtual
{
    private int _last;

    /// <summary>Stores <paramref name="value"/>.</summary>
    /// <param name="value">The value.</param>
    public virtual void Store(int value)
    {
        _last = value;
    }
}
