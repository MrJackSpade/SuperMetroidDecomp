using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Decodes the ending's colours from the cartridge (#1544), which the import pre-colours the ending's sheets in and takes
/// each scene's backdrop from. Nothing installs them as a colour table: the game draws the pre-coloured sheets and shows
/// the installed backdrops.
/// </summary>
internal static class EndingPaletteImport
{
    /// <summary>Decodes all seven ending colour resources, recognising the logo's calculated crossfade.</summary>
    /// <param name="bus">Cartridge import source for the colour words and the logo crossfade's pointer-selected backward copies.</param>
    /// <exception cref="InvalidDataException">A colour has an unrepresentable high bit.</exception>
    internal static EndingPaletteSet Read(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return new EndingPaletteSet(ReadOne(bus, EndingPaletteId.Escape), ReadOne(bus, EndingPaletteId.PostCredits),
            ReadOne(bus, EndingPaletteId.Credits), ReadOne(bus, EndingPaletteId.Explosion), ReadOne(bus, EndingPaletteId.FinalGunship),
            ReadOne(bus, EndingPaletteId.LogoInitial), ReadOne(bus, EndingPaletteId.LogoCrossfade));
    }

    /// <summary>The colour count of resource <paramref name="id"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined resource.</exception>
    internal static int ColorCount(EndingPaletteId id) => Ensure.Listed(EndingPaletteDefinitions.ColorCounts, id);

    /// <summary>Decodes resource <paramref name="id"/>: a whole screen's, a run's or the logo crossfade's colours.</summary>
    /// <exception cref="InvalidDataException">A colour has an unrepresentable high bit.</exception>
    private static EndingPalette ReadOne(ISnesAddressSpace bus, EndingPaletteId id)
    {
        int count = ColorCount(id);
        byte[] native = ReadNativePalette(bus, id, count);
        var colours = new RgbColor[count];
        for (int index = 0; index < count; index++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)));
            if (!RgbColor.IsColorWord(checked((ushort)word)))
                throw new InvalidDataException($"Ending {id} palette color {index} has an unrepresentable high bit.");
            colours[index] = RgbColor.FromWord(checked((ushort)word));
        }
        if (count == ScreenColourImage.ColorCount)
            return EndingPaletteResources.FromScreen(colours);
        return id == EndingPaletteId.LogoCrossfade
            ? EndingPaletteResources.FromCrossfade(EndingLogoCrossfadeImage.Decode(colours))
            : EndingPaletteResources.FromRun(colours);
    }

    /// <summary>
    /// $8B:E58A copies each logo fade palette backwards from its selected $8C pointer.
    /// Flatten the native destination order as [step][BG or OBJ palette][color], keeping the
    /// pointer table and copy direction in code.
    /// </summary>
    internal static byte[] ReadNativePalette(ISnesAddressSpace bus, EndingPaletteId id,
        int count)
    {
        if (id != EndingPaletteId.LogoCrossfade)
            return RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                EndingPaletteSources.Source(id), count * sizeof(ushort));

        var native = new byte[count * sizeof(ushort)];
        for (int step = 0; step < EndingLogoDefinitions.CrossfadeSteps; step++)
        for (int palette = 0; palette < 2; palette++)
        for (int color = 0; color < 16; color++)
        {
            SnesAddress source = EndingPaletteSources.LogoCrossfadeSource(step, palette)
                .AddWithinBank(-(15 - color) * sizeof(ushort));
            int index = (step * 2 + palette) * 16 + color;
            BinaryPrimitives.WriteUInt16LittleEndian(
                native.AsSpan(index * sizeof(ushort)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CartridgeBankWindow.At(EndingSourceDefinitions.LogoPaletteBank, source.Offset)));
        }
        return native;
    }
}
