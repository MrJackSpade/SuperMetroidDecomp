namespace SuperMetroid.Core.Assets;

/// <summary>Each installed sprite sheet's file name.</summary>
public static class SpriteSheetDefinitions
{
    /// <summary>Every installed sprite sheet.</summary>
    public static IReadOnlyList<SpriteSheetId> Sheets { get; } = Enum.GetValues<SpriteSheetId>();

    /// <summary>The installed name of a sheet's image and manifest, without extension.</summary>
    public static string FileName(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => "escape-timer",
        SpriteSheetId.Grapple => "grapple",
        SpriteSheetId.SamusBodyTop => "samus-body-top",
        SpriteSheetId.SamusBodyBottom => "samus-body-bottom",
        SpriteSheetId.SamusOverlays => "samus-overlays",
        SpriteSheetId.SamusAtmosphere => "samus-atmosphere",
        SpriteSheetId.SamusDeath => "samus-death",
        SpriteSheetId.IntroProjectiles => "intro-projectiles",
        SpriteSheetId.ProjectilesPower => "projectiles-power",
        SpriteSheetId.ProjectilesWave => "projectiles-wave",
        SpriteSheetId.ProjectilesIce => "projectiles-ice",
        SpriteSheetId.ProjectilesIceWave => "projectiles-ice-wave",
        SpriteSheetId.ProjectilesSpazer => "projectiles-spazer",
        SpriteSheetId.ProjectilesSpazerWave => "projectiles-spazer-wave",
        SpriteSheetId.ProjectilesSpazerIce => "projectiles-spazer-ice",
        SpriteSheetId.ProjectilesSpazerIceWave => "projectiles-spazer-ice-wave",
        SpriteSheetId.ProjectilesPlasma => "projectiles-plasma",
        SpriteSheetId.ProjectilesPlasmaWave => "projectiles-plasma-wave",
        SpriteSheetId.ProjectilesPlasmaIce => "projectiles-plasma-ice",
        SpriteSheetId.ProjectilesPlasmaIceWave => "projectiles-plasma-ice-wave",
        SpriteSheetId.ProjectilesSpazerPlasmaWave => "projectiles-spazer-plasma-wave",
        SpriteSheetId.ProjectilesSpazerPlasmaIce => "projectiles-spazer-plasma-ice",
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
