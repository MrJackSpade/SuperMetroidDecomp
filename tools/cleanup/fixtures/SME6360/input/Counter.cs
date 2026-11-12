namespace Fixture;

/// <summary>A counter whose hand-written code is split across two parts.</summary>
public sealed partial class Counter
{
    /// <summary>The value counted, initialized after the first.</summary>
    private readonly int _count = Start();

    /// <summary>The value counted.</summary>
    public int Count => _count;

    /// <summary>The value both fields start from.</summary>
    private static int Start() => 1;
}
