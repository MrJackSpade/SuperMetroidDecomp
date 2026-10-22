using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>What each of Samus's sheets composes from the installed stock artwork (#1392).</summary>
internal static class SamusSheetSources
{
    /// <summary>The OBJ palette row of parts that do not choose their own, as Samus's draws leave it.</summary>
    private const int DefaultPalette = 0;

    /// <summary>The priority of parts that do not choose their own, as Samus's draws leave it.</summary>
    private const int DefaultPriority = 0;

    /// <summary>Composes one of Samus's sheets.</summary>
    internal static ComposedSpriteSheet Compose(SamusSheetInputs inputs, SpriteSheetId sheet) =>
        SpriteSheetComposer.Compose(For(inputs, sheet), DefaultPalette, DefaultPriority);

    /// <summary>What one of Samus's sheets composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is not one of Samus's.</exception>
    internal static SpriteSheetSources For(SamusSheetInputs inputs, SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The escape timer is not one of Samus's sheets."),
        SpriteSheetId.Grapple => GrappleSheetComposer.Sources(inputs),
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.Sources(inputs, sheet),
        SpriteSheetId.SamusOverlays => SamusOverlaySheetComposer.Sources(inputs),
        SpriteSheetId.SamusAtmosphere => SamusAtmosphereSheetComposer.Sources(inputs),
        _ => ProjectileSheetComposer.Sources(inputs, sheet),
    };
}
