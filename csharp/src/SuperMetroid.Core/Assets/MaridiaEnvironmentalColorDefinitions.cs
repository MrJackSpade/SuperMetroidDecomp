using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native $8D:F4EF..F61A sand/waterfall color rotations. Sixteen first-row colors remain required source inputs.</summary>
internal static class MaridiaEnvironmentalColorDefinitions
{
    internal static bool TrySourcePointer(ushort pointer, out ushort source)
    {
        foreach (var program in MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int offset = pointer - program.ColorPointer(0, 0);
            if (offset < 0 || offset >= program.FrameCount * program.FrameByteCount
                || (offset & 1) != 0 || offset % program.FrameByteCount >= 2 * program.ColorsPerFrame) continue;
            int frame = offset / program.FrameByteCount;
            int color = offset % program.FrameByteCount / 2;
            int group = color / program.FrameCount;
            int rotated = (color + frame) % program.FrameCount;
            if (program.Owner == MaridiaEnvironmentalPaletteOwner.SandFalls)
            {
                // Sand falls use the lower sand-pit band, preserving their own edits.
                var pits = MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All[0];
                source = pits.ColorPointer(0, pits.FrameCount + rotated);
            }
            else source = program.ColorPointer(0, group * program.FrameCount + rotated);
            return true;
        }
        source = 0;
        return false;
    }

    /// <summary>$8D:F4EF..F4FD and F57F..F58D: eight required sand colors and eight required waterfall colors, selected by cyclic phase.</summary>
    internal static bool TryReadColor(ushort pointer, IReadOnlyDictionary<ushort, Bgr555> inputs, out Bgr555 color)
    {
        color = Bgr555.Black;
        return TrySourcePointer(pointer, out ushort source) && inputs.TryGetValue(source, out color);
    }
}
