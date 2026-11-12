using System.Diagnostics;

namespace Fixture;

/// <summary>A counter whose hand-written code is split across two parts.</summary>
[DebuggerDisplay("{Count}")]
public sealed class Counter
{
    /// <summary>The value the counter starts from, initialized first.</summary>
    private readonly int _first = Start();

    /// <summary>The sum of both values.</summary>
    public int Total => _first + _count;

    /// <summary>The value counted, initialized after the first.</summary>
    private readonly int _count = Start();

    /// <summary>The value counted.</summary>
    public int Count => _count;

    /// <summary>The value both fields start from.</summary>
    private static int Start() => 1;
}
