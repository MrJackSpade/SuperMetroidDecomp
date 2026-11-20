namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple beam's enemy test for fixtures whose rooms hold no enemy the beam can meet (#2049).</summary>
internal static class GrappleEnemyFixtures
{
    /// <summary>An enemy test that finds no enemy at any endpoint.</summary>
    internal static Func<Position16<RoomPixels>, GrappleEnemyCollision?> NoEnemy { get; } = _ => null;
}
