using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheets Samus's projectiles, bombs, charge flare and trails draw from (#1392): one per beam
/// combination, whose characters and beam colours differ, and one for the intro's flashbacks, and the names of the
/// frames each holds.
/// </summary>
public static class ProjectileSheetDefinitions
{
    /// <summary>The frame a trail side shows before its first pose: the cleared attribute word, OBJ character 0.</summary>
    public const string UnshownTrailFrame = "Trail.Unshown";

    /// <summary>
    /// The combinations with a sheet: every one the game can load. Spazer with Plasma, with or without Ice and Wave,
    /// has no translated beam palette, so loading it fails before anything is drawn.
    /// </summary>
    public static IReadOnlyList<SamusBeamCombination> Combinations { get; } =
    [
        SamusBeamCombination.Power, SamusBeamCombination.Wave, SamusBeamCombination.Ice, SamusBeamCombination.IceWave,
        SamusBeamCombination.Spazer, SamusBeamCombination.SpazerWave, SamusBeamCombination.SpazerIce, SamusBeamCombination.SpazerIceWave,
        SamusBeamCombination.Plasma, SamusBeamCombination.PlasmaWave, SamusBeamCombination.PlasmaIce, SamusBeamCombination.PlasmaIceWave,
        SamusBeamCombination.SpazerPlasmaWave, SamusBeamCombination.SpazerPlasmaIce,
    ];

    /// <summary>The gameplay sheet drawn while <paramref name="beams"/> is the loaded beam combination.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no sheet.</exception>
    public static SpriteSheetId For(SamusBeamCombination beams) => beams switch
    {
        SamusBeamCombination.Power => SpriteSheetId.ProjectilesPower,
        SamusBeamCombination.Wave => SpriteSheetId.ProjectilesWave,
        SamusBeamCombination.Ice => SpriteSheetId.ProjectilesIce,
        SamusBeamCombination.IceWave => SpriteSheetId.ProjectilesIceWave,
        SamusBeamCombination.Spazer => SpriteSheetId.ProjectilesSpazer,
        SamusBeamCombination.SpazerWave => SpriteSheetId.ProjectilesSpazerWave,
        SamusBeamCombination.SpazerIce => SpriteSheetId.ProjectilesSpazerIce,
        SamusBeamCombination.SpazerIceWave => SpriteSheetId.ProjectilesSpazerIceWave,
        SamusBeamCombination.Plasma => SpriteSheetId.ProjectilesPlasma,
        SamusBeamCombination.PlasmaWave => SpriteSheetId.ProjectilesPlasmaWave,
        SamusBeamCombination.PlasmaIce => SpriteSheetId.ProjectilesPlasmaIce,
        SamusBeamCombination.PlasmaIceWave => SpriteSheetId.ProjectilesPlasmaIceWave,
        SamusBeamCombination.SpazerPlasmaWave => SpriteSheetId.ProjectilesSpazerPlasmaWave,
        SamusBeamCombination.SpazerPlasmaIce => SpriteSheetId.ProjectilesSpazerPlasmaIce,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(beams), beams, "The beam combination has no translated palette, so no projectile sheet."),
        _ => throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination."),
    };

    /// <summary>The frame of a timed projectile or bomb composition.</summary>
    public static string ProjectileFrame(ProjectileSpriteId composition) => "Projectile." + composition.Name;

    /// <summary>The frame of a charge-flare composition.</summary>
    public static string FlareFrame(ProjectileSpriteId composition) => "Flare." + composition.Name;

    /// <summary>The frame of a trail pose.</summary>
    public static string TrailFrame(ProjectileTrailFrame pose) => "Trail." + pose;

    /// <summary>The flare compositions a projectile sheet holds: those the flare's selectors name.</summary>
    public static IEnumerable<ProjectileSpriteId> FlareCompositions => ChargeFlareSpriteDefinitions.Selectors.Distinct();

    /// <summary>Every frame a projectile sheet holds: each projectile and flare composition, each trail pose and the unshown trail.</summary>
    public static IEnumerable<string> Frames =>
        ProjectileSpriteDefinitions.All.Select(ProjectileFrame)
            .Concat(FlareCompositions.Select(FlareFrame))
            .Concat(Enum.GetValues<ProjectileTrailFrame>().Select(TrailFrame))
            .Append(UnshownTrailFrame);
}
