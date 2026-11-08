using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// Import-time reading of the title's native direct-mode HDMA tables: the only reader of their header, repeat-count
/// and colour-write bytes, which it decodes at their own widths and turns into the gradient's stops once (#1468).
/// </summary>
[EncodingOwner("title gradient direct-mode HDMA tables")]
public static class TitleGradientDecoder
{
    /// <summary>
    /// Decodes the selected title gradient's fixed-color and control HDMA tables into its stops: one wherever a line's
    /// colour or blend differs from the line above, the first on line 0, the last band running to the screen's bottom.
    /// </summary>
    /// <param name="cartridge">Cartridge source containing the native title HDMA tables.</param>
    /// <param name="zoom">Native zoom value whose low-byte bits 4 through 7 select the fixed-color table.</param>
    /// <returns>The decoded title gradient.</returns>
    public static ScreenGradient Decode(IImportCartridgeSource cartridge, ushort zoom)
    {
        int pointer = (int)FrontendSourceDefinitions.FixedColorPointers.AddWithinBank((zoom >> 4) % 16 * sizeof(ushort));
        int table = FrontendSourceDefinitions.TitleGradientColorBank |
            cartridge.ReadCartridgeWord(pointer);
        byte[] fixedWrites = Expand(cartridge, table);
        byte[] control = Expand(cartridge, FrontendSourceDefinitions.TitleGradientBlendTable);
        var stops = ImmutableArray.CreateBuilder<GradientStop>();
        byte red = 0, green = 0, blue = 0;
        for (int line = 0; line < ScreenGeometry.ScreenHeightPixels; line++)
        {
            byte write = fixedWrites[line], component = (byte)(write & 31);
            if ((write & 0x20) != 0) red = component; // cartridge-encoding: allow(Register) - COLDATA red-select bit
            if ((write & 0x40) != 0) green = component; // cartridge-encoding: allow(Register) - COLDATA green-select bit
            if ((write & 0x80) != 0) blue = component; // cartridge-encoding: allow(Register) - COLDATA blue-select bit
            var colour = new FadeLevelColour(red, green, blue).ToColour();
            GradientBlend blend = Blend(control[line], line);
            if (stops.Count == 0 || stops[^1].Colour != colour || stops[^1].Blend != blend)
                stops.Add(new GradientStop(line, colour, blend));
        }
        return new ScreenGradient(stops.ToImmutable(), ScreenGeometry.ScreenHeightPixels);
    }

    /// <summary>
    /// What a line's colour-math control byte has the gradient's colour do (#1514): subtracted from the floor and the
    /// backdrop, or added to them and the sprites. The title writes no other control.
    /// </summary>
    /// <exception cref="InvalidDataException">The byte selects any other blend.</exception>
    private static GradientBlend Blend(byte control, int line)
    {
        const int floor = 0x01; // cartridge-encoding: allow(Register) - CGADSUB BG1 enable bit
        const int sprites = 0x10; // cartridge-encoding: allow(Register) - CGADSUB OBJ enable bit
        const int backdrop = 0x20; // cartridge-encoding: allow(Register) - CGADSUB backdrop enable bit
        const int subtract = 0x80; // cartridge-encoding: allow(Register) - CGADSUB subtract bit
        if (control == (subtract | backdrop | floor))
            return GradientBlend.DarkensFloorAndBackdrop;
        if (control == (backdrop | sprites | floor))
            return GradientBlend.BrightensFloorBackdropAndSprites;
        throw new InvalidDataException(
            $"The title gradient's line {line} blends by control {control}, neither darkening the floor and the backdrop nor brightening them and the sprites.");
    }

    /// <summary>Expands one direct-mode HDMA table, retaining the last write after its zero terminator.</summary>
    private static byte[] Expand(IImportCartridgeSource cartridge, int cursor)
    {
        var result = new byte[ScreenGeometry.ScreenHeightPixels];
        int line = 0;
        byte previous = 0;
        while (line < result.Length)
        {
            byte header = cartridge.ReadCartridgeByte(cursor++);
            if (header == 0)
            {
                result.AsSpan(line).Fill(previous);
                break;
            }
            int count = (header & 127) == 0 ? 128 : header & 127;
            bool transferEveryLine = (header & 128) != 0;
            if (!transferEveryLine) previous = cartridge.ReadCartridgeByte(cursor++);
            for (int tick = 0; tick < count && line < result.Length; tick++)
            {
                if (transferEveryLine) previous = cartridge.ReadCartridgeByte(cursor++);
                result[line++] = previous;
            }
        }
        return result;
    }
}
