using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Reaches the boss palette ranges through the room's enemy presentation (#1395).</summary>
internal static class EnemyPresentationBossColours
{
    /// <summary>The boss palette ranges of <paramref name="presentation"/>'s CGRAM.</summary>
    internal static BossColours BossColours(this EnemyPresentation presentation) => new(Ensure.NotNull(presentation).Cgram);
}
