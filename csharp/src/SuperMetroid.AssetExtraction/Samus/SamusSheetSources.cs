using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>What each of Samus's sheets composes from the installed stock artwork (#1392).</summary>
internal static class SamusSheetSources
{
    /// <summary>What one of Samus's sheets composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is not one of Samus's.</exception>
    internal static SpriteSheetSources For(SamusSheetInputs inputs, SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The escape timer is not one of Samus's sheets."),
        SpriteSheetId.PauseHud => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The pause HUD composes from the HUD's images, not sprite parts."),
        SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The menus' pictures compose from their pages, not sprite parts."),
        SpriteSheetId.Grapple => GrappleSheetComposer.Sources(inputs),
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.Sources(inputs, sheet),
        SpriteSheetId.SamusOverlays => SamusOverlaySheetComposer.Sources(inputs),
        SpriteSheetId.SamusAtmosphere => SamusAtmosphereSheetComposer.Sources(inputs),
        SpriteSheetId.SamusDeath => SamusDeathSheetComposer.Sources(inputs),
        SpriteSheetId.GameOver or SpriteSheetId.FileSelect or SpriteSheetId.GameOptions or
            SpriteSheetId.MapSprites or SpriteSheetId.PauseSprites or SpriteSheetId.Title or
            SpriteSheetId.IntroSprites or SpriteSheetId.CeresFlightSprites or
            SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites or
            SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
            SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
            SpriteSheetId.EndingShootingSprites or SpriteSheetId.EndingShootingLogoQuarterSprites or
            SpriteSheetId.EndingShootingLogoHalfSprites or SpriteSheetId.EndingShootingLogoThreeQuartersSprites or
            SpriteSheetId.EndingLogoSprites or SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or
            SpriteSheetId.ProjectilesWave or SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or
            SpriteSheetId.ProjectilesSpazer or SpriteSheetId.ProjectilesSpazerWave or
            SpriteSheetId.ProjectilesSpazerIce or SpriteSheetId.ProjectilesSpazerIceWave or
            SpriteSheetId.ProjectilesPlasma or SpriteSheetId.ProjectilesPlasmaWave or
            SpriteSheetId.ProjectilesPlasmaIce or SpriteSheetId.ProjectilesPlasmaIceWave or
            SpriteSheetId.ProjectilesSpazerPlasmaWave or SpriteSheetId.ProjectilesSpazerPlasmaIce or
            SpriteSheetId.AreaMapForegroundCrateria or SpriteSheetId.AreaMapForegroundBrinstar or
            SpriteSheetId.AreaMapForegroundNorfair or SpriteSheetId.AreaMapForegroundWreckedShip or
            SpriteSheetId.AreaMapForegroundMaridia or SpriteSheetId.AreaMapForegroundTourian or
            SpriteSheetId.AreaMapBackgroundCrateria or SpriteSheetId.AreaMapBackgroundBrinstar or
            SpriteSheetId.AreaMapBackgroundNorfair or SpriteSheetId.AreaMapBackgroundWreckedShip or
            SpriteSheetId.AreaMapBackgroundMaridia or SpriteSheetId.AreaMapBackgroundTourian or
            SpriteSheetId.PauseMapTiles or SpriteSheetId.PauseInterfaceTiles or
            SpriteSheetId.IntroSceneTiles or SpriteSheetId.IntroFontTiles or SpriteSheetId.CeresFlightObjectTiles or
            SpriteSheetId.CeresIntroObjectTiles or SpriteSheetId.ZebesRevealTiles or
            SpriteSheetId.EndingCreditsRows or SpriteSheetId.EndingPostCreditsPictures or
            SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures or
            SpriteSheetId.TitleFloorTiles or SpriteSheetId.TitleBabyTiles or SpriteSheetId.CeresFlightFloorTiles or
            SpriteSheetId.EndingEscapeAFloorTiles or SpriteSheetId.EndingEscapeBFloorTiles or
            SpriteSheetId.EndingPlanetExplosionFloorTiles or SpriteSheetId.EndingFlyawayFloorTiles or
            SpriteSheetId.EndingRewardIconFloorTiles =>
            ProjectileSheetComposer.Sources(inputs, sheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
