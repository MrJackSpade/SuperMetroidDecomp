using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Imports the timer's visual compositions without exposing its countdown mechanics.</summary>
[CpuBusBoundary("codec: ROM escape-timer OBJ compositions → escape timer presentation")]
public static class EscapeTimerPresentationExtractor
{
    /// <summary>Imports the escape-timer label and ten digit OAM compositions with their compiled visual placement defaults.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the native timer spritemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing label and digit frames, pixel-space anchors, eight-pixel digit spacing, and inherited OBJ palette 5.</returns>
    /// <remarks>Countdown arithmetic, activation, and display-number selection are not extracted as presentation data.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A spritemap exceeds the bounded OAM part capacity or the presentation fails validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, EscapeTimerVisualPart[]>(StringComparer.Ordinal)
        {
            [EscapeTimerPresentationDefinitions.LabelFrame] =
                ReadSpritemap(bus, EscapeTimerPresentationDefinitions.LabelSpritemap),
        };
        for (int digit = 0; digit < 10; digit++)
        {
            ushort pointer = EscapeTimerPresentationDefinitions.DigitSpritemapPointer(digit);
            frames.Add(EscapeTimerPresentationDefinitions.DigitFrame(digit),
                ReadSpritemap(bus, EscapeTimerPresentationDefinitions.SpritemapBank | pointer));
        }

        using var output = new MemoryStream();
        EscapeTimerPresentation.Write(output, new()
        {
            Version = EscapeTimerPresentationDefinitions.Version,
            Palette = 5,
            DigitSpacing = 8,
            Anchors = new(StringComparer.Ordinal)
            {
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Label)] = new(0, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Minutes)] = new(-28, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Seconds)] = new(-4, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Centiseconds)] = new(20, 0),
            },
            Frames = frames,
        });
        return output.ToArray();
    }

    /// <summary>Decodes one native five-byte-per-part OAM map into bounded pixel offsets and tile attributes.</summary>
    /// <remarks>The map's own palette bits are discarded so the presentation's fixed OBJ palette applies to every part.</remarks>
    private static EscapeTimerVisualPart[] ReadSpritemap(ISnesAddressSpace bus, int address)
    {
        int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
        if (count > EscapeTimerPresentationDefinitions.MaximumParts)
            throw new InvalidDataException($"Escape timer spritemap ${address:X6} exceeds OAM capacity.");
        var parts = new EscapeTimerVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + sizeof(ushort) + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source + 3));
            parts[index] = new()
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(source + 2)),
                TileNumber = attributes.TileNumber,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                Palette = null,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
