namespace SuperMetroid.Core.Assets;

/// <summary>Families of owned sprite artwork that whole-frame draw entries can bind.</summary>
/// <remarks>Append-only: numeric values appear in serialized display packets and debugger state.</remarks>
public enum SpriteArtworkKind
{
    /// <summary>The 25 escape-timer characters (digits, separators and the TIME label).</summary>
    EscapeTimerTiles,
    /// <summary>The installed standard OBJ sheet as a 512-character page; characters past the sheet are blank.</summary>
    StandardObjects,
    /// <summary>Samus's 32 body characters from one top and one bottom transfer definition.</summary>
    SamusBody,
    /// <summary>The standard OBJ page with Samus's 32 body characters over its first 32.</summary>
    SamusBodyOnStandardObjects,
    /// <summary>One arm-cannon cover character.</summary>
    SamusArmCannonCover,
    /// <summary>The grapple endpoint character followed by the four rope-segment characters.</summary>
    GrappleTiles,
    /// <summary>The 160 death-explosion characters in character order.</summary>
    SamusDeathExplosion,
    /// <summary>The standard OBJ page with the grapple endpoint and rope characters over characters $20-$24.</summary>
    GrappleTilesOnStandardObjects,
    /// <summary>An OBJ page with the beam characters of the combination its variant names and both trail sheets.</summary>
    ProjectileObjects,
    /// <summary>The intro's OBJ page: its intro sheet with the cinematic characters over its tail.</summary>
    IntroObjects,
    /// <summary>The gameplay OBJ character table as its uploads bind it, which enemy-format sprites draw from.</summary>
    GameplayObjects,
    /// <summary>The OBJ characters a frontend screen's OAM records name in its VRAM, until #1358.</summary>
    FrontendObjects,
}
