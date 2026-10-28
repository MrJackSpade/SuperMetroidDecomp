using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ProjectileSheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which sheet and frames each projectile beam combination draws from.</summary>
public static class ProjectileSheetFrames
{
    /// <summary>The gameplay sheet drawn while <paramref name="beams"/> is the loaded beam combination.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no sheet.</exception>
    public static SpriteSheetId For(SamusBeamCombination beams) =>
        Sheets.TryGetValue(beams, out SpriteSheetId value) ? value : throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination.");

    /// <summary>The frame of a timed projectile or bomb composition.</summary>
    public static string ProjectileFrame(ProjectileSpriteId composition) => "Projectile." + composition.Name;

    /// <summary>The frame of a charge-flare composition.</summary>
    public static string FlareFrame(ProjectileSpriteId composition) => "Flare." + composition.Name;

    /// <summary>The frame of a trail pose.</summary>
    public static string TrailFrame(ProjectileTrailFrame pose) => "Trail." + pose;

    /// <summary>Every frame a projectile sheet holds: each projectile and flare composition, each trail pose and the unshown trail.</summary>
    public static IEnumerable<string> Frames =>
        ProjectileSpriteDefinitions.All.Select(ProjectileFrame)
            .Concat(FlareCompositions.Select(FlareFrame))
            .Concat(Enum.GetValues<ProjectileTrailFrame>().Select(TrailFrame))
            .Append(UnshownTrailFrame);
}
