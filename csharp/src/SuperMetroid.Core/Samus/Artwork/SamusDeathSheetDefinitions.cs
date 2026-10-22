using SuperMetroid.Core.Samus.Death;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's suit explosion as she dies (#1392): each explosion frame facing right and left,
/// which Samus's spritemap table draws over the death characters.
/// </summary>
public static class SamusDeathSheetDefinitions
{
    /// <summary>Every facing the explosion is drawn in.</summary>
    public static IReadOnlyList<HorizontalDirection> Facings { get; } = [HorizontalDirection.Right, HorizontalDirection.Left];

    /// <summary>The explosion frames each facing shows.</summary>
    public const int ExplosionFrameCount = SamusDeathExplosionTimingDefinitions.RecordCount;

    /// <summary>The explosion in frame <paramref name="frame"/> facing <paramref name="facing"/>.</summary>
    public static string ExplosionFrame(HorizontalDirection facing, int frame) => $"Explosion.{facing}.{frame}";

    /// <summary>Every frame the sheet holds.</summary>
    public static IEnumerable<string> Frames =>
        Facings.SelectMany(facing => Enumerable.Range(0, ExplosionFrameCount).Select(frame => ExplosionFrame(facing, frame)));
}
