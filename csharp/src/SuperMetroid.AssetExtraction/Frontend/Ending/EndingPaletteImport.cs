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
internal static partial class EndingPaletteImport
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

    /// <summary>Decodes resource <paramref name="id"/>: a whole screen's, a run's or the logo crossfade's colours.</summary>
    /// <exception cref="InvalidDataException">A colour has an unrepresentable high bit.</exception>
    private static EndingPalette ReadOne(ISnesAddressSpace bus, EndingPaletteId id)
    {
        EndingPaletteDefinition definition = Ensure.Listed(EndingPaletteDefinitions.Palettes, id);
        int count = definition.ColorCount;
        byte[] native = ReadNativePalette(bus, id, count);
        var colours = new RgbColor[count];
        for (int index = 0; index < count; index++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)));
            if (!RgbColor.IsColorWord(checked((ushort)word)))
                throw new InvalidDataException($"Ending {id} palette color {index} has an unrepresentable high bit.");
            colours[index] = RgbColor.FromWord(checked((ushort)word));
        }
        return definition.Kind switch
        {
            EndingPaletteKind.WholeScreen => EndingPaletteResources.FromScreen(colours),
            EndingPaletteKind.Run => EndingPaletteResources.FromRun(colours),
            EndingPaletteKind.LogoCrossfade => EndingPaletteResources.FromCrossfade(EndingLogoCrossfadeImage.Decode(colours)),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, $"Undefined {nameof(EndingPaletteKind)} {definition.Kind}."),
        };
    }

    /// <summary>The native colour words of resource <paramref name="id"/>, <paramref name="count"/> colours: the crossfade's in its destination order, any other resource's as stored.</summary>
    internal static byte[] ReadNativePalette(ISnesAddressSpace bus, EndingPaletteId id, int count) =>
        Ensure.Listed(EndingPaletteDefinitions.Palettes, id).Kind == EndingPaletteKind.LogoCrossfade
            ? ReadNativeCrossfade(bus, count)
            : RomDataReader.ReadFixedBank(bus, EndingPaletteSources.Source(id), count * sizeof(ushort));

    /// <summary>
    /// $8B:E58A copies each logo fade palette backwards from its selected $8C pointer.
    /// Flatten the native destination order as [step][BG or OBJ palette][color], keeping the
    /// pointer table and copy direction in code.
    /// </summary>
    private static byte[] ReadNativeCrossfade(ISnesAddressSpace bus, int count)
    {
        var native = new byte[count * sizeof(ushort)];
        for (int step = 0; step < EndingLogoDefinitions.CrossfadeSteps; step++)
        for (int palette = 0; palette < EndingPaletteSources.CrossfadeLayers.Count; palette++)
        for (int color = 0; color < EndingLogoCrossfadeColours.ColoursPerLayer; color++)
        {
            SnesAddress source = EndingPaletteSources.LogoCrossfadeSource(step, EndingPaletteSources.CrossfadeLayers[palette])
                .AddWithinBank(-(15 - color) * sizeof(ushort));
            int index = EndingLogoCrossfadeImage.Position(step, EndingPaletteSources.CrossfadeLayers[palette], color);
            BinaryPrimitives.WriteUInt16LittleEndian(
                native.AsSpan(index * sizeof(ushort)),
                RomDataReader.ReadWordFixedBank(bus,
                    CartridgeBankWindow.At(EndingSourceDefinitions.LogoPaletteBank, source.Offset)));
        }
        return native;
    }
}
