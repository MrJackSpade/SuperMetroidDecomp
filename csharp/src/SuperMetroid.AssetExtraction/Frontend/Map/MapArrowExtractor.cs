using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Imports the visual portion of map arrow records without exporting controller masks.</summary>
public static class MapArrowExtractor
{
    /// <summary>The scroll direction of each cartridge arrow record, in record order.</summary>
    private static readonly MapScrollDirection[] ArrowDirections =
        [MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down];

    /// <summary>The most phases an arrow's $FF-terminated animation program may hold before it is rejected as unterminated.</summary>
    private const int MaximumPhases = 255;

    /// <summary>
    /// Exports the four map-scroll arrows' drawing anchors while verifying their fixed native spritemap bindings and that each
    /// arrow is a still picture: its animation program's phases change no picture, so their timing is not exported (#2359).
    /// </summary>
    /// <param name="bus">Import-capable cartridge source for mixed arrow records, sprite-program tables, and byte-timed animation streams.</param>
    /// <returns>A new UTF-8 JSON buffer keyed by direction, with screen-pixel X/Y anchors.</returns>
    /// <remarks>Y is normalized to native record Y minus one. Controller masks and animation shape operands are not exported; the shape must remain fixed through a bounded, $FF-terminated program.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">An animation ID, spritemap binding, phase shape, termination or anchor is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var arrows = new Dictionary<string, MapArrowEntry>();
        for (int index = 0; index < ArrowDirections.Length; index++)
        {
            MapScrollDirection direction = ArrowDirections[index];
            int record = MapSourceDefinitions.FileSelectMap.ScrollArrows + index * MapSourceDefinitions.FileSelectMap.ScrollArrowRecordBytes;
            int animation = Read(record + 4);
            if (animation is < 1 or > 9) throw new InvalidDataException("Invalid cartridge map arrow animation.");
            int program = Read(MapSourceDefinitions.Animation.SpritePrograms + (animation - 1) * 2);
            int variants = Read(MapSourceDefinitions.Animation.SpriteBases + (animation - 1) * 2);
            if (Read(MapSourceDefinitions.Menu.ObjectBank | variants) != MapSpriteOrdinals.Spritemaps.Word(MapArrowSprites.SpriteBase(direction)))
                throw new InvalidDataException($"Cartridge arrow shape differs for {direction}.");
            for (int phase = 0; ; phase++)
            {
                int address = MapSourceDefinitions.Menu.ObjectBank | (program + phase * 3);
                if (bus.ReadCartridgeByte(address) == byte.MaxValue) break;
                if (phase == MaximumPhases) throw new InvalidDataException("Unterminated map arrow animation.");
                // Native ignores the middle byte and uses only the final byte as
                // a shape offset. All four retail arrows use their one fixed shape.
                if (bus.ReadCartridgeByte(address + 2) != 0) throw new InvalidDataException("Unexpected varying map arrow shape.");
            }
            arrows.Add(MapScrollDirectionKeys.Installed.Of(direction), new() { X = Read(record), Y = Read(record + 2) - 1 });
        }
        using var stream = new MemoryStream();
        MapArrowFile.Write(stream, new() { Version = MapArrowFormat.Version, Arrows = arrows });
        return stream.ToArray();
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
    }
}
