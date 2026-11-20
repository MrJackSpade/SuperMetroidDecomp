using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>What each of Samus's sheets composes from the installed stock artwork (#1392).</summary>
internal static class SamusSheetSources
{
    /// <summary>
    /// Each of Samus's sheets, bound to the composer that builds it from her stock artwork: Grapple, her body, its
    /// overlays, her atmosphere and death, and every projectile sheet <see cref="ProjectileSheetComposer"/> composes.
    /// </summary>
    internal static IReadOnlyDictionary<SpriteSheetId, Func<SamusSheetInputs, SpriteSheetSources>> Sheets { get; } = Bind();

    /// <summary>What one of Samus's sheets composes.</summary>
    /// <exception cref="KeyNotFoundException">The sheet is not one of Samus's.</exception>
    internal static SpriteSheetSources For(SamusSheetInputs inputs, SpriteSheetId sheet) => Sheets[sheet](inputs);

    /// <summary>Binds each of Samus's sheets to its composer.</summary>
    private static Dictionary<SpriteSheetId, Func<SamusSheetInputs, SpriteSheetSources>> Bind()
    {
        var sheets = new Dictionary<SpriteSheetId, Func<SamusSheetInputs, SpriteSheetSources>>
        {
            [SpriteSheetId.Grapple] = GrappleSheetComposer.Sources,
            [SpriteSheetId.SamusBody] = SamusBodySheetComposer.Sources,
            [SpriteSheetId.SamusOverlays] = SamusOverlaySheetComposer.Sources,
            [SpriteSheetId.SamusAtmosphere] = SamusAtmosphereSheetComposer.Sources,
            [SpriteSheetId.SamusDeath] = SamusDeathSheetComposer.Sources,
        };
        foreach (SpriteSheetId projectiles in ProjectileSheetComposer.Sheets)
            sheets.Add(projectiles, inputs => ProjectileSheetComposer.Sources(inputs, projectiles));
        return sheets;
    }
}
