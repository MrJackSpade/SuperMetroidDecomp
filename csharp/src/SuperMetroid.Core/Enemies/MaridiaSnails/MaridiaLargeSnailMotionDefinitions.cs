namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>How the large Maridia snail pushes and shoves Samus (#2043).</summary>
internal static class MaridiaLargeSnailMotionDefinitions
{
    /// <summary>
    /// How far toward Samus the snail reaches while it pushes her, in pixels: its move tests the terrain that much
    /// further ahead, then draws back.
    /// </summary>
    internal const int PushReach = 16;

    /// <summary>How far below her centre the snail reckons Samus's touch from above, in pixels.</summary>
    internal const int SamusTouchBias = 3;

    /// <summary>How far the snail shoves Samus sideways when she touches it, in pixels.</summary>
    internal const int ShoveDistance = 4;
}
