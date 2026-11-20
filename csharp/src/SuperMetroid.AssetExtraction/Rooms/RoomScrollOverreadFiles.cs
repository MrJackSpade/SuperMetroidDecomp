using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts what each explicit room scroll layout's load writes over the kept scroll cells past the room's screens,
/// decoded from the player's cartridge (#2356), and loads it as the <see cref="ScrollCellsPastRoomScreens{TCell}"/> service.
/// </summary>
/// <remarks>
/// The original loader copies every kept cell from the layout's table, so the cells past a room's screens are the table's
/// bytes after them, and past a table that holds only the room's screens the data stored after it. Each byte reads as the
/// camera reads a cell: zero closed, one open with the camera aligned to the screen's top, and any other value open.
/// </remarks>
[EncodingOwner("room scroll cell bytes past a room's screens")]
public static class RoomScrollOverreadFiles
{
    /// <summary>
    /// Decodes every explicit layout's cells past its screens and writes them as JSON. No manifest is written: the
    /// document names its own format, and the cartridge it was decoded from is the installation receipt's.
    /// </summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file is replaced.</param>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        var cells = new Dictionary<string, IReadOnlyList<RoomScrollState>>();
        foreach (RoomScrollDefinition layout in RoomScrollDefinitions.Explicit)
        {
            int table = Ensure.Listed(RoomScrollTableSourceDefinitions.Tables, layout);
            var past = new RoomScrollState[RoomScrollGrid.KeptCellCount - layout.Cells.Count];
            for (int cell = 0; cell < past.Length; cell++)
                past[cell] = CellOf(cartridge.ReadCartridgeByte(table + layout.Cells.Count + cell));
            cells.Add(layout.Name, past);
        }
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            new RoomScrollOverreadDocument(RoomScrollOverreadFormat.Version, cells), RoomScrollOverreadFormat.JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, RoomScrollOverreadFormat.FileName), bytes);
    }

    /// <summary>Loads the installed file, as the player may have edited it.</summary>
    /// <param name="directory">Installed room scroll overread directory holding the file.</param>
    /// <exception cref="InvalidDataException">
    /// The file's format is unsupported, or it does not hold, for exactly the explicit layouts, the cells that fill the
    /// kept cells past each one's screens.
    /// </exception>
    public static ScrollCellsPastRoomScreens<RoomScrollState> Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, RoomScrollOverreadFormat.FileName));
        RoomScrollOverreadDocument document = JsonAssetDocument.Read<RoomScrollOverreadDocument>(
            new MemoryStream(bytes, writable: false), RoomScrollOverreadFormat.JsonOptions, "room scroll overread");
        if (document.Version != RoomScrollOverreadFormat.Version)
            throw new InvalidDataException("Room scroll overread file has an unsupported version.");
        IReadOnlyDictionary<string, IReadOnlyList<RoomScrollState>> held = document.CellsPastScreens ??
            throw new InvalidDataException("Room scroll overread file holds no cells.");
        if (held.Count != RoomScrollDefinitions.Explicit.Count)
            throw new InvalidDataException(
                $"Room scroll overread file holds {held.Count} layouts, not the {RoomScrollDefinitions.Explicit.Count} explicit ones.");
        var cells = new Dictionary<string, IReadOnlyList<RoomScrollState>>();
        foreach (RoomScrollDefinition layout in RoomScrollDefinitions.Explicit)
        {
            if (!held.TryGetValue(layout.Name, out IReadOnlyList<RoomScrollState>? past) || past is null)
                throw new InvalidDataException($"Room scroll overread file has no cells for {layout.Name}.");
            int expected = RoomScrollGrid.KeptCellCount - layout.Cells.Count;
            if (past.Count != expected || past.Any(state => !Enum.IsDefined(state)))
                throw new InvalidDataException(
                    $"Room scroll overread file holds {past.Count} cells for {layout.Name}, not {expected} scroll states.");
            cells.Add(layout.Name, [.. past]);
        }
        return new ScrollCellsPastRoomScreens<RoomScrollState>(cells);
    }

    /// <summary>The state the camera reads a stored scroll byte as.</summary>
    private static RoomScrollState CellOf(byte stored) => stored switch
    {
        0 => RoomScrollState.Closed,
        1 => RoomScrollState.OpenAlignedToTop,
        _ => RoomScrollState.Open,
    };
}
