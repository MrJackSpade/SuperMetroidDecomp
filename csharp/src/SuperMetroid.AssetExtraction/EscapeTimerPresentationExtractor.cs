using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports the timer's visual compositions without exposing its countdown mechanics.</summary>
public static class EscapeTimerPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, EscapeTimerVisualPart[]>(StringComparer.Ordinal)
        {
            [EscapeTimerPresentationDefinitions.LabelFrame] =
                ReadSpritemap(bus, EscapeTimerPresentationDefinitions.LabelSpritemap),
        };
        for (int digit = 0; digit < 10; digit++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                EscapeTimerPresentationDefinitions.DigitPointerTable + digit * sizeof(ushort));
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
                ["Label"] = new(0, 0),
                ["Minutes"] = new(-28, 0),
                ["Seconds"] = new(-4, 0),
                ["Centiseconds"] = new(20, 0),
            },
            Frames = frames,
        });
        return output.ToArray();
    }

    private static EscapeTimerVisualPart[] ReadSpritemap(ISnesAddressSpace bus, int address)
    {
        int count = RomDataReader.ReadWordFixedBank(bus, address);
        if (count > EscapeTimerPresentationDefinitions.MaximumParts)
            throw new InvalidDataException($"Escape timer spritemap ${address:X6} exceeds OAM capacity.");
        var parts = new EscapeTimerVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + sizeof(ushort) + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(bus, source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, source + 3));
            parts[index] = new()
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)bus.ReadByte(source + 2)),
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
