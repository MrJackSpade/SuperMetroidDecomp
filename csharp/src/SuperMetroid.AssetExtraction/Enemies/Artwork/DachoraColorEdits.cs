using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Compares Dachora's supplied speed and shine frames with the frames calculated from the normal palette and keeps only
/// the colours that differ, the selection the game resolves its colours from.
/// </summary>
internal static class DachoraColorEdits
{
    /// <summary>Selects the normal palette and only the deviations from the calculated speed and shine frames.</summary>
    internal static DachoraColorSelection Select(RgbColor[] normal, RgbColor[][] speed, RgbColor[][] shine) =>
        new(normal, SpeedEdits(normal, speed), ShineEdits(normal, shine));

    /// <summary>The supplied speed-frame colours that differ from the calculated blue ramp.</summary>
    private static Dictionary<ColourStep, RgbColor> SpeedEdits(RgbColor[] normal, RgbColor[][] speed)
    {
        var edits = new Dictionary<ColourStep, RgbColor>();
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        for (int color = 0; color < DachoraColorFormat.ColorsPerFrame; color++)
            if (speed[frame][color] != DachoraColors.SpeedColor(normal, frame, color)) edits.Add(new ColourStep(frame, color), speed[frame][color]);
        return edits;
    }

    /// <summary>The supplied shine-frame colours that differ from the calculated fade toward white.</summary>
    private static Dictionary<ColourStep, RgbColor> ShineEdits(RgbColor[] normal, RgbColor[][] shine)
    {
        var edits = new Dictionary<ColourStep, RgbColor>();
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        for (int color = 0; color < DachoraColorFormat.ColorsPerFrame; color++)
            if (shine[frame][color] != DachoraColors.ShineColor(normal, frame, color)) edits.Add(new ColourStep(frame, color), shine[frame][color]);
        return edits;
    }
}
