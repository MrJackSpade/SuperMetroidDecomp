namespace Fixture;

/// <summary>A counter whose other part holds directives.</summary>
public sealed class Counter
{
    /// <summary>The count.</summary>
    public int Count { get; set; }

#if DEBUG
    /// <summary>Whether nothing is counted.</summary>
    public bool IsZero => Count == 0;
#endif

    /// <summary>The count doubled.</summary>
    public int Doubled => Count * 2;
}
