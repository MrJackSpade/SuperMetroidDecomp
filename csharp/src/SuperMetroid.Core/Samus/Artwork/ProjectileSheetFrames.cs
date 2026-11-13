using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ProjectileSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which sheet and frames each projectile beam combination draws from.</summary>
public static class ProjectileSheetFrames
{
    /// <summary>The gameplay sheet of a draw pictured with <paramref name="beams"/> (#1586).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no sheet.</exception>
    public static SpriteSheetId For(SamusBeamCombination beams) =>
        Sheets.TryGetValue(beams, out SpriteSheetId value) ? value : throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination.");

    /// <summary>The frame of a timed projectile or bomb composition.</summary>
    public static string ProjectileFrame(ProjectileSpriteId composition) => "Projectile." + composition.Name;

    /// <summary>The frame of a charge-flare composition.</summary>
    public static string FlareFrame(ProjectileSpriteId composition) => "Flare." + composition.Name;

    /// <summary>The frame of a trail pose.</summary>
    public static string TrailFrame(ProjectileTrailFrame pose) => "Trail." + pose;

    /// <summary>The frame of a composition of the burst a hit shows at an enemy it hurts (#1713).</summary>
    public static string HitBurstFrame(ProjectileSpriteId composition) => "Hit." + composition.Name;
}
