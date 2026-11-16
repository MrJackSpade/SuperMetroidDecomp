namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The Alcoon's three authored fireball launches.</summary>
internal static class AlcoonFireballLaunchDefinitions
{
    /// <summary>The upward spit: the fireball rises one pixel per frame.</summary>
    internal static AlcoonFireballLaunch Upward { get; } = new(-SignedEightEight.FromParts(1, 0));

    /// <summary>The straight-ahead spit: the fireball keeps its height.</summary>
    internal static AlcoonFireballLaunch Level { get; } = new(SignedEightEight.Zero);

    /// <summary>The downward spit: the fireball falls one pixel per frame.</summary>
    internal static AlcoonFireballLaunch Downward { get; } = new(SignedEightEight.FromParts(1, 0));

    /// <summary>
    /// Where a fireball leaves an Alcoon facing right: sixteen pixels ahead of it and twelve above; one facing left spits
    /// it as far to its left.
    /// </summary>
    internal static Offset16<RoomPixels> MuzzleFacingRight { get; } = new(16, -12);

    /// <summary>The sideways speed every fireball is spat at, toward the Alcoon's facing: four pixels an update.</summary>
    internal static SignedEightEight LaunchSpeed { get; } = SignedEightEight.FromParts(4, 0);

    /// <summary>The sideways speed a fireball slows to and then keeps: two pixels an update.</summary>
    internal static SignedEightEight TerminalSpeed { get; } = SignedEightEight.FromParts(2, 0);

    /// <summary>How much a fireball's sideways speed slows each update until it reaches its terminal speed: a quarter pixel.</summary>
    internal static SignedEightEight Deceleration { get; } = SignedEightEight.FromParts(0, 64);
}
