namespace Fixture;

/// <summary>Counts through a type only this file sees.</summary>
public static class Local
{
    /// <summary>The count.</summary>
    public static int Count => Hidden.Value;
}

/// <summary>A value only this file sees.</summary>
file static class Hidden
{
    /// <summary>The value.</summary>
    internal const int Value = 1;
}
