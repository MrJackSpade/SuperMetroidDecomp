namespace Fixture;

public sealed partial class Counter
{
#if DEBUG
    /// <summary>Whether nothing is counted.</summary>
    public bool IsZero => Count == 0;
#endif

    /// <summary>The count doubled.</summary>
    public int Doubled => Count * 2;
}
