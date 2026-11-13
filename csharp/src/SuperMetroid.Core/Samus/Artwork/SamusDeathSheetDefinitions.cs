namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's suit explosion as she dies (#1392): each explosion frame facing right and left,
/// which Samus's death drawing shows over her death pose.
/// </summary>
public static class SamusDeathSheetDefinitions
{
    /// <summary>The explosion in frame <paramref name="frame"/> facing <paramref name="facing"/>.</summary>
    public static string ExplosionFrame(HorizontalDirection facing, int frame) => $"Explosion.{facing}.{frame}";
}
