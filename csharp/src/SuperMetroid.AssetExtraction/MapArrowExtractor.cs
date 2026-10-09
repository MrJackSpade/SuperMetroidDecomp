using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports the visual portion of map arrow records without exporting controller masks.</summary>
public static class MapArrowExtractor
{
    /// <summary>Exports the four map-scroll arrows' drawing anchors and cosmetic phase durations while verifying their fixed native spritemap bindings.</summary>
    /// <param name="bus">Import-capable cartridge source for mixed arrow records, sprite-program tables, and byte-timed animation streams.</param>
    /// <returns>A new UTF-8 JSON buffer keyed by direction, with screen-pixel X/Y anchors and positive update-tick durations.</returns>
    /// <remarks>Y is normalized to native record Y minus one. Controller masks and animation shape operands are not exported; the shape must remain fixed through a bounded, $FF-terminated program.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">An animation ID, spritemap binding, phase shape, termination, anchor, or duration is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var arrows = new Dictionary<string, MapArrowEntry>();
        for (int index = 0; index < MapArrowDefinitions.Count; index++)
        {
            var direction = (MapScrollDirection)(index + 1);
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            int animation = Read(record + 4);
            if (animation is < 1 or > 9) throw new InvalidDataException("Invalid cartridge map arrow animation.");
            int program = Read(MapAnimationRomData.SpritePrograms + (animation - 1) * 2);
            int variants = Read(MapAnimationRomData.SpriteBases + (animation - 1) * 2);
            if (Read(FileSelectMapRomData.MenuObjectBank | variants) != MapArrowDefinitions.SpriteBase(direction))
                throw new InvalidDataException($"Cartridge arrow shape differs for {direction}.");
            var durations = new List<int>();
            for (int phase = 0; ; phase++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (program + phase * 3);
                byte duration = bus.ReadCartridgeByte(address);
                if (duration == byte.MaxValue) break;
                if (phase == MapArrowFormat.MaximumPhases) throw new InvalidDataException("Unterminated map arrow animation.");
                // Native ignores the middle byte and uses only the final byte as
                // a shape offset. All four retail arrows use their one fixed shape.
                if (bus.ReadCartridgeByte(address + 2) != 0) throw new InvalidDataException("Unexpected varying map arrow shape.");
                durations.Add(duration);
            }
            arrows.Add(direction.ToString(), new() { X = Read(record), Y = Read(record + 2) - 1, DurationTicks = durations.ToArray() });
        }
        using var stream = new MemoryStream();
        MapArrowPresentation.Write(stream, new() { Version = MapArrowFormat.Version, Arrows = arrows });
        return stream.ToArray();
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
    }
}
