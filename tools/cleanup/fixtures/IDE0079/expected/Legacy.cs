namespace Fixture;

/// <summary>A value kept only for older callers.</summary>
public static class Legacy
{
    /// <summary>The old value.</summary>
    [Obsolete("Use the current value.")]
    public static int Value => 1;
}
