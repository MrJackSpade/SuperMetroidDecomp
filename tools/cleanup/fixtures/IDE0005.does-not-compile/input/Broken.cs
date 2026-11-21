namespace Fixture;

/// <summary>The project's compiler error, which the fix leaves for the build to report.</summary>
public static class Broken
{
    /// <summary>A number that is text.</summary>
    public static int Count => "four";
}
