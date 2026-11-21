namespace Fixture;

/// <summary>Counts through a type only this file sees.</summary>
public static class Local
{
    /// <summary>The count.</summary>
    public static int Count => Hidden.Value;
}
