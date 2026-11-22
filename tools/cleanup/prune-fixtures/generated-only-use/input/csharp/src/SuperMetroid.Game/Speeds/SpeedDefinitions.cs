namespace SuperMetroid.Game.Speeds;

/// <summary>The speeds production lists, which only the code the compiler writes reads.</summary>
internal static class SpeedDefinitions
{
    /// <summary>A walk's speed.</summary>
    public static int Walk { get; } = 1;

    /// <summary>A run's speed.</summary>
    public static int Run { get; } = 2;
}
