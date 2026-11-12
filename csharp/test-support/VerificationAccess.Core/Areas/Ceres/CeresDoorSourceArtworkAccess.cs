using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresDoorSourceArtwork"/> members production does not use.</summary>
internal static class CeresDoorSourceArtworkAccess
{
    /// <summary>Exposes <see cref="CeresDoorSourceArtwork"/> state that only verification reads.</summary>
    extension(CeresDoorSourceArtwork self)
    {
        /// <summary>Canonical selected presentation data: the door's characters and every colour it shows.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
        {
            content.Append("tiles", self.tiles.Characters.Span);
            Span<RgbColor> normalColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = self.Visuals.NormalColor(color);
            content.AppendColors("normal", normalColors);
            Span<RgbColor> escapeColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = self.Visuals.EscapeColor(color);
            content.AppendColors("escape", escapeColors);
            content.Append("animation", CeresDoorVisualDefinitions.AnimationStepCount);
            Span<RgbColor> rowColors = stackalloc RgbColor[CeresDoorVisualDefinitions.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualDefinitions.AnimationStepCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = self.Visuals.AnimationColor(row, color);
                content.AppendColors("row", rowColors);
            }
        });
    }
}
