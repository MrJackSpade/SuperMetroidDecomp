using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports the timer's visual compositions without exposing its countdown mechanics.</summary>
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
        ArgumentNullException.ThrowIfNull(bus);
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
                ["Label"] = new(0, 0),
                ["Minutes"] = new(-28, 0),
                ["Seconds"] = new(-4, 0),
                ["Centiseconds"] = new(20, 0),
            },
            Frames = frames,
        });
        return output.ToArray();
    }

    /// <summary>Decodes one bounded native timer spritemap into renderer-facing parts.</summary>
    /// <param name="bus">Cartridge address space containing the OAM records.</param>
    /// <param name="address">Absolute SNES address of the spritemap count word.</param>
    /// <returns>Parts with signed offsets and their native tile attributes.</returns>
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
