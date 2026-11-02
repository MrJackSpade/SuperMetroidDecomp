using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Resolves reserve visual data once, validating the compiled native fill-to-shape mapping.</summary>
public static class PauseReserveTankExtractor
{
    /// <summary>Validates the native reserve fill-to-shape bindings, resolves tank anchors and palette, and serializes every referenced sprite frame.</summary>
    /// <param name="bus">Supported-cartridge address space containing reserve mappings, positions, palette attributes, and spritemaps.</param>
    /// <returns>UTF-8 JSON bytes for reserve-tank presentation data without energy mechanics.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        for (int index = 0; index < PauseMenuSourceDefinitions.ReserveTankPartialFillCount; index++)
            if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankPartialFills + index * 2) != PauseReserveTankVisualWords.Ordinals.Word(PartialMap(index)))
                throw new InvalidDataException("Unexpected native reserve fill binding.");
        var anchors = new Position16<SpritePixels>[PauseReserveTankDefinitions.AnchorCount];
        int y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankYPosition) - 1));
        for (int index = 0; index < anchors.Length; index++)
            anchors[index] = new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankXPositions + index * 2), checked((ushort)y));
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in PauseReserveTankParts.Frames()) frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, PauseReserveTankVisualWords.Ordinals.Word(definition.Id)));
        using var output = new MemoryStream();
        PauseReserveTankSourcePresentation.Write(output, new() { Version = PauseReserveTankFormat.Version,
            Palette = PauseReserveTankFormat.Palette, Anchors = anchors, Frames = frames });
        return output.ToArray();
    }

    /// <summary>Maps each of eight fill-step indices to $20..$27; both tables agree.</summary>
    /// <param name="index">Entry index, 0..15, covering both identical eight-entry tables.</param>
    /// <returns>The Empty-through-Fill7 spritemap identity selected by index modulo eight; gameplay computes the index using fourteen energy per step.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the sixteen entries.</exception>
    private static PauseReserveTankVisual PartialMap(int index) => (uint)index < PauseReserveTankFormat.PartialMapEntries
        ? PauseReserveTankParts.FillLevel(index % PauseReserveTankDefinitions.FillLevels.Length)
        : throw new ArgumentOutOfRangeException(nameof(index));
}
