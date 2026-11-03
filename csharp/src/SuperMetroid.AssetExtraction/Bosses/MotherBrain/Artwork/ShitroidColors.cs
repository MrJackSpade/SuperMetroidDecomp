using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reads the live Shitroid scene's installed colours: the normal pulse, the initialization targets and their identity.</summary>
internal static class ShitroidColors
{
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
            return color == 0 ? colors.selection.ShitroidTransparentSlot : colors.selection.Shitroid.Resolve(color - 1);
        }
        if (target == ShitroidColorTarget.Sidehopper) return colors.selection.Sidehopper.Resolve(color);
        return target == ShitroidColorTarget.DeadSidehopper ? colors.selection.DeadSidehopper.Resolve(color) :
            throw new ArgumentOutOfRangeException(nameof(target));
    }
}
