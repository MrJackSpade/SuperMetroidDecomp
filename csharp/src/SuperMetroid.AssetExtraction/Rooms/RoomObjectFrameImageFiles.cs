using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of a room object's frames (#1557). At install the import reads each frame's draw list from the
/// cartridge, checks its runs and collision against the geometry the game draws by, and pictures what each cell shows;
/// the installed file holds every frame's picture, cell by cell, which the player edits in place. The game reads the
/// pictures as installed: there is no compiled copy and no override.
/// </summary>
internal static class RoomObjectFrameImageFiles
{
    /// <summary>The installed document's format; a document naming another is rejected and the content is extracted again.</summary>
    /// <remarks>
    /// Revision 3 names no cartridge (#1693): the installation receipt does. Revision 4 names the collectibles' frames by what
    /// each shows, not by its place in its group (#1762).
    /// </remarks>
    internal const int FormatVersion = 4;

    /// <summary>Camel-case field names, mirroring by name and readable output for the installed document.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// A room object whose frames are keyed by their draw: each of <paramref name="frames"/> laid out from the runs the
    /// game draws it by, under its published name.
    /// </summary>
    /// <param name="fileName">The installed file holding the frames' pictures.</param>
    /// <param name="name">The object's name in diagnostics.</param>
    /// <param name="frames">Every frame the object draws, as the game draws it, in the installed file's order.</param>
    /// <param name="visualId">The published name of a frame.</param>
    /// <exception cref="InvalidDataException">A frame has no run, or its first run does not start at its origin.</exception>
    /// <exception cref="ArgumentException">Two frames are the same draw.</exception>
    internal static RoomObjectFrameFamily<RoomObjectDraw> DrawFamily(string fileName, string name,
        IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> frames, Func<RoomObjectDraw, string> visualId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Ensure.NotNull(visualId);
        var layouts = new Dictionary<RoomObjectDraw, RoomObjectFrameLayout>();
        var ids = new Dictionary<RoomObjectDraw, string>();
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList frame in Ensure.NotNull(frames))
        {
            layouts.Add(frame.Draw, RoomObjectFrameLayout.Of(frame));
            ids.Add(frame.Draw, visualId(frame.Draw));
        }
        return new(fileName, name, layouts, ids);
    }

    /// <summary>Pictures every frame of <paramref name="family"/> from the cartridge and writes the installed document.</summary>
    /// <param name="bus">The cartridge the frames' draw lists are read from.</param>
    /// <param name="directory">The family's installed directory, created if absent; its document must not already exist.</param>
    /// <param name="family">The room object whose frames are pictured.</param>
    /// <exception cref="InvalidDataException">A frame's draw list differs from the geometry or collision the game draws by.</exception>
    /// <exception cref="IOException">The document already exists or cannot be written.</exception>
    internal static void Extract<TFrame>(ISnesAddressSpace bus, string directory, RoomObjectFrameFamily<TFrame> family)
        where TFrame : notnull
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(family);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var frames = new List<FrameEntry>();
        foreach (TFrame frame in family.Layouts.Keys)
        {
            RoomObjectFrameImage image = Picture(bus, family.Name, Layout(family, frame));
            frames.Add(new FrameEntry(Id(family, frame), image.Cells
                .Select(cell => new FrameCell(cell.Key.X, cell.Key.Y, cell.Value.Block, cell.Value.Mirroring)).ToArray()));
        }
        Directory.CreateDirectory(directory);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new FrameDocument(FormatVersion, frames.ToArray()), JsonOptions);
        using var output = new FileStream(Path.Combine(directory, family.FileName), FileMode.CreateNew, FileAccess.Write);
        output.Write(json);
    }

    /// <summary>Loads the installed pictures of every frame of <paramref name="family"/> as the player left them, without the cartridge.</summary>
    /// <param name="directory">The family's installed directory.</param>
    /// <param name="family">The room object whose frames are loaded.</param>
    /// <exception cref="InvalidDataException">
    /// The document's format is not this importer's, a frame is missing, repeated or unknown, or a frame's
    /// picture does not cover exactly the frame's cells.
    /// </exception>
    /// <exception cref="IOException">The document cannot be read.</exception>
    internal static RoomObjectFrameImages<TFrame> Load<TFrame>(string directory, RoomObjectFrameFamily<TFrame> family) where TFrame : notnull
    {
        Ensure.NotNull(family);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, family.FileName);
        FrameDocument document = Read(path, family.Name);
        try
        {
            return Create(document, family);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid {family.Name} pictures {path}: {error.Message}", error);
        }
    }

    /// <summary>
    /// Reads <paramref name="layout"/>'s draw list from the cartridge, checks each run's header, cell collision and
    /// continuation against the layout, and pictures what each cell shows. A cell the list writes twice shows what it
    /// writes last, as the room does.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw list differs from the layout.</exception>
    internal static RoomObjectFrameImage Picture(ISnesAddressSpace bus, string family, RoomObjectFrameLayout layout)
    {
        Ensure.NotNull(bus);
        int pointer = RoomObjectDrawAddressCodec.Of(layout.Draw);
        var cells = new Dictionary<Offset16<RoomBlocks>, RoomBlockAppearance>();
        int cursor = pointer;
        ReadOnlySpan<RoomObjectShotBlockDrawDefinitions.Run> runs = layout.Runs.Span;
        for (int runIndex = 0; runIndex < runs.Length; runIndex++)
        {
            RoomObjectShotBlockDrawDefinitions.Run run = runs[runIndex];
            if (ReadWord(bus, cursor) != RoomObjectRunHeaderCodec.Encode(run.Direction, run.Count))
                throw new InvalidDataException($"{family} frame {layout.Draw} differs in run {runIndex}'s shape.");
            for (int cell = 0; cell < run.Count; cell++)
            {
                RoomTerrainBlock written = RoomLevelWordCodec.Decode(ReadWord(bus, checked(cursor + 2 + cell * 2)));
                if (written.Collision != run.Collisions.Span[cell])
                    throw new InvalidDataException($"{family} frame {layout.Draw} collides differently at run {runIndex}, cell {cell}.");
                cells[run.CellAt(cell)] = written.Appearance;
            }
            if (ReadWord(bus, checked(cursor + 2 + run.Count * 2)) != RoomObjectRunContinuationCodec.Encode(layout, runIndex))
                throw new InvalidDataException($"{family} frame {layout.Draw} differs at run {runIndex}'s continuation.");
            cursor = checked(cursor + 4 + run.Count * 2);
        }
        return new RoomObjectFrameImage(cells);
    }

    /// <summary>Builds the frames' pictures from an installed document, checking every frame covers exactly its cells.</summary>
    private static RoomObjectFrameImages<TFrame> Create<TFrame>(FrameDocument document, RoomObjectFrameFamily<TFrame> family) where TFrame : notnull
    {
        if (document.Version != FormatVersion || document.Frames is null)
            throw new InvalidDataException("the document's format is not this importer's.");
        var byId = family.Ids.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);
        var images = new Dictionary<TFrame, RoomObjectFrameImage>();
        foreach (FrameEntry entry in document.Frames)
        {
            if (entry?.Id is null || !byId.TryGetValue(entry.Id, out TFrame? frame))
                throw new InvalidDataException($"frame {entry?.Id ?? "(unnamed)"} is not one of {family.Name}'s frames.");
            if (images.ContainsKey(frame))
                throw new InvalidDataException($"frame {entry.Id} is pictured twice.");
            images.Add(frame, Image(entry, Layout(family, frame).Cells()));
        }
        foreach (TFrame drawn in family.Layouts.Keys)
            if (!images.ContainsKey(drawn))
                throw new InvalidDataException($"frame {Id(family, drawn)} is not pictured.");
        return new RoomObjectFrameImages<TFrame>(images);
    }

    /// <summary>One frame's picture from its installed entry, which must cover exactly <paramref name="covered"/>.</summary>
    private static RoomObjectFrameImage Image(FrameEntry entry, HashSet<Offset16<RoomBlocks>> covered)
    {
        if (entry.Cells is null)
            throw new InvalidDataException($"frame {entry.Id} has no cells.");
        var cells = new Dictionary<Offset16<RoomBlocks>, RoomBlockAppearance>();
        foreach (FrameCell cell in entry.Cells)
        {
            if (cell is null)
                throw new InvalidDataException($"frame {entry.Id} has an empty cell.");
            var place = new Offset16<RoomBlocks>(cell.X, cell.Y);
            if (!covered.Contains(place))
                throw new InvalidDataException($"frame {entry.Id} pictures ({cell.X},{cell.Y}), which the frame does not cover.");
            RoomBlockAppearance shown;
            try
            {
                shown = RoomBlockAppearance.Of(cell.Block, cell.Mirroring);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException($"frame {entry.Id} shows an invalid block at ({cell.X},{cell.Y}).", error);
            }
            if (!cells.TryAdd(place, shown))
                throw new InvalidDataException($"frame {entry.Id} pictures ({cell.X},{cell.Y}) twice.");
        }
        if (cells.Count != covered.Count)
            throw new InvalidDataException($"frame {entry.Id} pictures {cells.Count} of its {covered.Count} cells.");
        return new RoomObjectFrameImage(cells);
    }

    /// <summary>The geometry of <paramref name="frame"/>.</summary>
    private static RoomObjectFrameLayout Layout<TFrame>(RoomObjectFrameFamily<TFrame> family, TFrame frame) where TFrame : notnull =>
        family.Layouts.TryGetValue(frame, out RoomObjectFrameLayout layout)
            ? layout
            : throw new InvalidOperationException($"{family.Name} frame {frame} has no geometry.");

    /// <summary>The published name of <paramref name="frame"/>.</summary>
    private static string Id<TFrame>(RoomObjectFrameFamily<TFrame> family, TFrame frame) where TFrame : notnull =>
        family.Ids.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"{family.Name} frame {frame} has no published name.");

    /// <summary>Reads and parses an installed document, naming its path in a rejection.</summary>
    private static FrameDocument Read(string path, string family)
    {
        try
        {
            return JsonSerializer.Deserialize<FrameDocument>(File.ReadAllBytes(path), JsonOptions)
                ?? throw new InvalidDataException($"{family} pictures {path} are empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid {family} pictures {path}.", error);
        }
    }

    /// <summary>Reads a draw-list word from the room objects' bank, rejecting a word that straddles the bank's end.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int pointer) =>
        pointer >= ushort.MaxValue
            ? throw new InvalidDataException("A room-object draw-list word cannot start at the bank's last byte.")
            : CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, pointer));

    /// <summary>The installed document: its format and every frame's picture.</summary>
    private sealed record FrameDocument(int Version, FrameEntry[] Frames);

    /// <summary>One frame's picture: its published name and every cell it shows.</summary>
    private sealed record FrameEntry(string Id, FrameCell[] Cells);

    /// <summary>One pictured cell: its place from the object's origin, the block it shows and how that block is mirrored.</summary>
    [SpatialComponents("External format: a room object's frame document stores each pictured cell's offset from the object's origin as x/y blocks.")]
    private sealed record FrameCell(int X, int Y, int Block, BlockMirroring Mirroring);
}
