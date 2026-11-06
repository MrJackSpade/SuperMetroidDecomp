using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheets Samus's projectiles, bombs, charge flare and trails draw from (#1392): one per beam
/// combination, whose characters and beam colours differ, and one for the intro's flashbacks, and the names of the
/// frames each holds.
/// </summary>
public static class ProjectileSheetDefinitions
{
    /// <summary>The frame a trail side shows before its first pose: the cleared attribute word, sprite character 0.</summary>
    public const string UnshownTrailFrame = "Trail.Unshown";

    /// <summary>The value each key selects; For looks it up.</summary>
    public static readonly IReadOnlyDictionary<SamusBeamCombination, SpriteSheetId> Sheets = new Dictionary<SamusBeamCombination, SpriteSheetId>
    {
        [SamusBeamCombination.Power] = SpriteSheetId.ProjectilesPower,
        [SamusBeamCombination.Wave] = SpriteSheetId.ProjectilesWave,
        [SamusBeamCombination.Ice] = SpriteSheetId.ProjectilesIce,
        [SamusBeamCombination.IceWave] = SpriteSheetId.ProjectilesIceWave,
        [SamusBeamCombination.Spazer] = SpriteSheetId.ProjectilesSpazer,
        [SamusBeamCombination.SpazerWave] = SpriteSheetId.ProjectilesSpazerWave,
        [SamusBeamCombination.SpazerIce] = SpriteSheetId.ProjectilesSpazerIce,
        [SamusBeamCombination.SpazerIceWave] = SpriteSheetId.ProjectilesSpazerIceWave,
        [SamusBeamCombination.Plasma] = SpriteSheetId.ProjectilesPlasma,
        [SamusBeamCombination.PlasmaWave] = SpriteSheetId.ProjectilesPlasmaWave,
        [SamusBeamCombination.PlasmaIce] = SpriteSheetId.ProjectilesPlasmaIce,
        [SamusBeamCombination.PlasmaIceWave] = SpriteSheetId.ProjectilesPlasmaIceWave,
        [SamusBeamCombination.SpazerPlasmaWave] = SpriteSheetId.ProjectilesSpazerPlasmaWave,
        [SamusBeamCombination.SpazerPlasmaIce] = SpriteSheetId.ProjectilesSpazerPlasmaIce,
    };
}
