using System.Diagnostics.CodeAnalysis;

namespace Fixture;

/// <summary>Code whose suppressions are needed, unneeded, or needed for one of their codes only.</summary>
public sealed class Suppressions
{
    private int _count;

    /// <summary>The old value, read under a needed suppression.</summary>
    public static int Needed()
    {
#pragma warning disable CS0618
        return Legacy.Value;
#pragma warning restore CS0618
    }

    /// <summary>Twice <paramref name="value"/>, under a suppression that suppresses nothing.</summary>
    public static int Unneeded(int value)
    {
        return value * 2;
    }

    /// <summary>The old value plus one, under a suppression that needs only the first of its codes.</summary>
    public static int Partly()
    {
#pragma warning disable CS0618
        return Legacy.Value + 1;
#pragma warning restore CS0618
    }

    /// <summary>One, from an instance method that reads no instance state.</summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Fixture: an instance member by design.")]
    public int One() => 1;

    /// <summary>The next count, from an instance method that reads instance state.</summary>
    public int Next() => ++_count;
}
