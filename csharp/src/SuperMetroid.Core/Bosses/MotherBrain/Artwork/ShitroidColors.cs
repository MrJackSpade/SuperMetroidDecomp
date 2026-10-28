using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reads the live Shitroid scene's installed colours: the normal pulse, the initialization targets and their identity.</summary>
internal static class ShitroidColors
{
    /// <summary>Hashes the Sidehopper, Shitroid and dead-Sidehopper targets, then the eight normal pulse rows.</summary>
    internal static string ContentIdentity(ShitroidColorCatalog colors) =>
        SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
        {
            Span<RgbColor> selectedSidehopper = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedSidehopper.Length; color++) selectedSidehopper[color] = colors.sidehopper.Resolve(color);
            content.AppendColors("sidehopper", selectedSidehopper);
            Span<RgbColor> selectedShitroid = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedShitroid.Length; color++) selectedShitroid[color] = colors.TargetColor(ShitroidColorTarget.Shitroid, color);
            content.AppendColors("shitroid", selectedShitroid);
            Span<RgbColor> selectedCorpse = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedCorpse.Length; color++) selectedCorpse[color] = colors.deadSidehopper.Resolve(color);
            content.AppendColors("deadSidehopper", selectedCorpse);
            content.Append("normal", ShitroidColorDefinitions.NormalFrameCount);
            for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
            {
                var row = new RgbColor[ShitroidColorDefinitions.NormalColorsPerFrame];
                for (int color = 0; color < row.Length; color++) row[color] = colors.NormalColor(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Gets a packed RGB5 ink from normal pulse phase 0–7 and ink 0–3.</summary>
    internal static RgbColor NormalColor(ShitroidColorPulse normal, int frame, int color) =>
        (uint)frame < ShitroidColorDefinitions.NormalFrameCount && (uint)color < ShitroidColorDefinitions.NormalColorsPerFrame
            ? normal.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Shitroid normal frame {frame}, color {color} is outside the authored image.");

    /// <summary>Gets packed RGB5 color 0–15 from a selected initialization target, including its retained transparent-slot word.</summary>
    internal static RgbColor TargetColor(ShitroidColorCatalog colors, ShitroidColorTarget target, int color)
    {
        if (target == ShitroidColorTarget.Shitroid)
        {
            if ((uint)color >= ShitroidColorDefinitions.TargetColorCount) throw new ArgumentOutOfRangeException(nameof(color));
            return color == 0 ? colors.shitroidTransparentSlot : colors.shitroid.Resolve(color - 1);
        }
        if (target == ShitroidColorTarget.Sidehopper) return colors.sidehopper.Resolve(color);
        return target == ShitroidColorTarget.DeadSidehopper ? colors.deadSidehopper.Resolve(color) :
            throw new ArgumentOutOfRangeException(nameof(target));
    }
}
