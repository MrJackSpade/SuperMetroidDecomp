using System.Diagnostics;

namespace Fixture;

[DebuggerDisplay("{Count}")]
public sealed partial class Counter
{
    /// <summary>The value the counter starts from, initialized first.</summary>
    private readonly int _first = Start();

    /// <summary>The sum of both values.</summary>
    public int Total => _first + _count;
}
