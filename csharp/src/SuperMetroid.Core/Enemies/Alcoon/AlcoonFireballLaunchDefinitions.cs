namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The Alcoon's three authored fireball launches.</summary>
internal static class AlcoonFireballLaunchDefinitions
{
    /// <summary>The upward spit: the fireball rises one pixel per frame.</summary>
    internal static AlcoonFireballLaunch Upward { get; } = new(SignedEightEight.FromSignedRaw(-256));

    /// <summary>The straight-ahead spit: the fireball keeps its height.</summary>
    internal static AlcoonFireballLaunch Level { get; } = new(SignedEightEight.FromSignedRaw(0));

    /// <summary>The downward spit: the fireball falls one pixel per frame.</summary>
    internal static AlcoonFireballLaunch Downward { get; } = new(SignedEightEight.FromSignedRaw(256));
}
