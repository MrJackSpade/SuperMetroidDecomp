namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Whether a Powamp placement is its balloon, and how far the grapple pulls it (#1472).</summary>
/// <param name="IsBalloon">Whether it is the balloon.</param>
/// <param name="GrappleTravelDistance">The distance, 0 through 65535 pixels, the grapple pulls the balloon; a nonzero distance also refuses contact.</param>
internal sealed record PowampSpawnSettings(bool IsBalloon, int GrappleTravelDistance) : EnemySpawnSettings
{
    /// <summary>The distance, 0 through 65535 pixels, the grapple pulls the balloon; a nonzero distance also refuses contact.</summary>
    public int GrappleTravelDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, GrappleTravelDistance);
}
