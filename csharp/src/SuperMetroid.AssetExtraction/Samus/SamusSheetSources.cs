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
            SpriteSheetId.MenuForegroundTiles or SpriteSheetId.MenuMapTiles or
            SpriteSheetId.AreaMapForegroundTilesCrateria or SpriteSheetId.AreaMapForegroundTilesBrinstar or
            SpriteSheetId.AreaMapForegroundTilesNorfair or SpriteSheetId.AreaMapForegroundTilesWreckedShip or
            SpriteSheetId.AreaMapForegroundTilesMaridia or SpriteSheetId.AreaMapForegroundTilesTourian or
            SpriteSheetId.AreaMapBackgroundTilesCrateria or SpriteSheetId.AreaMapBackgroundTilesBrinstar or
            SpriteSheetId.AreaMapBackgroundTilesNorfair or SpriteSheetId.AreaMapBackgroundTilesWreckedShip or
            SpriteSheetId.AreaMapBackgroundTilesMaridia or SpriteSheetId.AreaMapBackgroundTilesTourian or
            SpriteSheetId.PauseMapTiles or SpriteSheetId.PauseInterfaceTiles or SpriteSheetId.PauseHudTiles or
            SpriteSheetId.IntroSceneTiles or SpriteSheetId.IntroFontTiles or SpriteSheetId.CeresFlightObjectTiles or
            SpriteSheetId.CeresIntroObjectTiles or SpriteSheetId.ZebesRevealTiles or
            SpriteSheetId.EndingCreditsFontTiles or SpriteSheetId.EndingTextFontTiles or
            SpriteSheetId.EndingSubtitleTiles or SpriteSheetId.EndingLogoTiles or SpriteSheetId.EndingWaitingTiles or
            SpriteSheetId.EndingWavySamusTiles or SpriteSheetId.EndingExplosionTiles or
            SpriteSheetId.TitleMode7Tiles or SpriteSheetId.TitleBabyTiles or SpriteSheetId.CeresFlightMode7Tiles or
            SpriteSheetId.EndingEscapeAMode7Tiles or SpriteSheetId.EndingEscapeBMode7Tiles or
            SpriteSheetId.EndingPlanetExplosionMode7Tiles or SpriteSheetId.EndingFlyawayMode7Tiles or
            SpriteSheetId.EndingRewardIconMode7Tiles =>
            ProjectileSheetComposer.Sources(inputs, sheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
