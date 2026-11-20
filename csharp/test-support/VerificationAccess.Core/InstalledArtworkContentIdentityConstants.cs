using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge values and helpers of <see cref="InstalledArtworkContentIdentity"/> that only verification reads.</summary>
internal static class InstalledArtworkContentIdentityConstants
{
    /// <summary>The installed content identity of a <see cref="InstalledSamusDeathPalettes"/>.</summary>
    extension(InstalledSamusDeathPalettes palettes)
    {
        /// <summary>SHA-256 of selected suited/suitless colors, whiteout shades and explosion selectors.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSamusDeathPalettes), content =>
        {
            Span<RgbColor> stepColours = stackalloc RgbColor[InstalledSamusDeathPalettes.ColorCount];
            for (int suit = 0; suit < InstalledSamusDeathPalettes.SuitCount; suit++)
            for (int palette = 0; palette < SamusShadeDefinitions.Death.ExplosionShadeCount; palette++)
            {
                for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++) stepColours[color] = palettes.SuitedColor(SuperMetroid.AssetExtraction.Samus.SamusSuitListLayout.Order[suit], palette, color);
                content.AppendColors("suited row", stepColours);
            }
            for (int palette = 0; palette < SamusShadeDefinitions.Death.ExplosionShadeCount; palette++)
            {
                for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++) stepColours[color] = palettes.SuitlessColor(palette, color);
                content.AppendColors("suitless row", stepColours);
            }
            Span<RgbColor> whiteoutColors = stackalloc RgbColor[SamusShadeDefinitions.Death.WhiteoutShadeCount];
            for (int index = 0; index < whiteoutColors.Length; index++) whiteoutColors[index] = palettes.WhiteoutColor(index);
            content.AppendColors("whiteout", whiteoutColors);
            Span<int> selectors = stackalloc int[SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount];
            for (int frame = 0; frame < selectors.Length; frame++) selectors[frame] = palettes.ExplosionPaletteIndex(frame);
            content.AppendWords("explosion palette indices", selectors);
        });
    }
}
