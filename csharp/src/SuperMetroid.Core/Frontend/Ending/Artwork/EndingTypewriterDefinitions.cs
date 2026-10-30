namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The ending typewriter's cadence: how long it waits before typing, between characters and after the percentage (#1467).</summary>
public static class EndingTypewriterDefinitions
{
    /// <summary>Updates held before a typewriter sequence begins.</summary>
    public const ushort InitialDelayFrames = 64;
    /// <summary>Updates between consecutive typewriter characters.</summary>
    public const ushort CharacterDelayFrames = 4;
    /// <summary>Updates that the completed item-percentage text remains visible.</summary>
    public const ushort PercentageHoldFrames = 128;
}
