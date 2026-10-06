using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native $8D:C914..C95E and C96A..C9B4 PLANET ZEBES text colors from three independently required bright endpoints.</summary>
internal static class PlanetZebesTextColorDefinitions
{
    internal static bool TryCoordinates(ushort pointer, out PlanetZebesTextPaletteFxProgramOwner owner, out int frame, out int color)
    {
        foreach (var program in PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        {
            int offset = pointer - program.ColorPointer(0, 0);
            int stride = PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount;
            if (offset >= 0 && offset < stride * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount
                && (offset & 1) == 0 && offset % stride < 2 * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            {
                owner = program.Owner; frame = offset / stride; color = offset % stride / 2;
                return true;
            }
        }
        owner = default; frame = color = 0;
        return false;
    }

    /// <summary>$8D:C95A/C95C/C95E: REQUIRED selected bright text endpoints. No color or intensity exception is claimed.</summary>
    internal static ushort EndpointPointer(int color) => PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All[0]
        .ColorPointer(PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount - 1, color);

    /// <summary>Round each RGB5 endpoint channel to nearest over seven native intervals; fade-out reverses the phase.</summary>
    internal static bool TryCalculate(ushort pointer, IReadOnlyDictionary<ushort, ushort> inputs, out ushort color)
    {
        color = 0;
        if (!TryCoordinates(pointer, out var owner, out int frame, out int index)
            || !inputs.TryGetValue(EndpointPointer(index), out ushort endpoint)) return false;
        int intervals = PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount - 1;
        int phase = owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn ? frame : intervals - frame;
        int red = ((endpoint & 31) * phase + intervals / 2) / intervals;
        int green = (((endpoint >> 5) & 31) * phase + intervals / 2) / intervals;
        int blue = (((endpoint >> 10) & 31) * phase + intervals / 2) / intervals;
        color = (ushort)(red | green << 5 | blue << 10);
        return true;
    }
}
