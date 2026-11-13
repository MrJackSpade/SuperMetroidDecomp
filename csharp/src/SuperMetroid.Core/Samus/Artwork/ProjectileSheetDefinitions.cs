using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheets Samus's projectiles, bombs, charge flare and trails draw from (#1392): one per beam
/// combination with pictures of its own, whose characters and beam colours differ, and one for the intro's flashbacks,
/// and the names of the frames each holds.
/// </summary>
public static class ProjectileSheetDefinitions
{
    /// <summary>
    /// The sheet the projectiles of each beam combination are pictured from (#1586). Spazer with Plasma, and Spazer with
    /// Plasma, Ice and Wave (the Golden Torizo code's Murder Beam), have no pictures of their own: their uncharged shot
    /// fires a charged Power or a charged Ice-Wave shot, and their projectiles show that combination's sheet, which holds
    /// everything they fire or place.
    /// </summary>
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
        [SamusBeamCombination.SpazerPlasma] = SpriteSheetId.ProjectilesPower,
        [SamusBeamCombination.SpazerPlasmaWave] = SpriteSheetId.ProjectilesSpazerPlasmaWave,
        [SamusBeamCombination.SpazerPlasmaIce] = SpriteSheetId.ProjectilesSpazerPlasmaIce,
        [SamusBeamCombination.SpazerPlasmaIceWave] = SpriteSheetId.ProjectilesIceWave,
    };
}
