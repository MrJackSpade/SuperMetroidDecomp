using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed record of the appearance inputs each block of a tileset's rooms depends on (#1466), one file per tileset
/// beside the block sheets: for every block, by the name its pictures begin with (<see cref="RoomBlockPictureNames.OfBlock"/>),
/// the inputs its picture changes with; or no entry at all for a block no room of the tileset shows, which the sheet does
/// not picture (#1636). It holds no characters,
/// tile positions or colours; those are drawn into the block sheet, which a player edits in place.
/// </summary>
public static class RoomBlockAppearanceFiles
{
    /// <summary>
    /// The schema revision: revision 2 records a collectible slot only on the blocks a collectible writes where it lies,
    /// alone, and only in tilesets whose rooms place one (#1513); revision 4 records no collectible slot at all, a
    /// collectible's pictures being named by its kind and frame, and names an encounter's look by the encounter and the
    /// look, not by a step of its artwork sequence (#1538); revision 5 names a Tourian statue's pictures by the statue's
    /// animation, not by the animated-tile object that copied them (#1584); revision 6 records no inputs, as null, for a
    /// block no room of the tileset shows, which has no picture (#1636); revision 7 records no blocks at all over the
    /// tileset's artwork, as null, for a tileset no room of which shows its blocks over it, Ceres Ridley's arena's (#1648);
    /// revision 8 names a block animation's frames by its block animation, not by the hardware's tiles (#1726); revision 9
    /// records each block's inputs once, Ceres Ridley's arena's among them, with no second record over other artwork (#1791);
    /// revision 10 records the tileset's block count and the inputs of only the blocks its rooms show, by block, so a block
    /// no room shows has no entry rather than a null one (#2558); revision 11 keys each block's inputs by the name its
    /// pictures begin with, not by a bare index (#2565).
    /// </summary>
    private const int Version = 11;

    /// <summary>The installed file name of <paramref name="tileset"/>'s record.</summary>
    internal static string FileNameOf(TilesetDefinition tileset) => $"{RoomBlockSheet.NameOf(tileset)}.block-appearances.json";

    /// <summary>Writes <paramref name="tileset"/>'s record of <paramref name="appearances"/> into <paramref name="directory"/>.</summary>
    /// <exception cref="IOException">The file already exists or cannot be written.</exception>
    internal static void Write(string directory, TilesetDefinition tileset, RoomBlockAppearances appearances)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(appearances);
        var document = new BlockAppearancesDocument(Version, RoomBlockSheet.NameOf(tileset), appearances.Count, Tokens(appearances));
        using var output = new FileStream(Path.Combine(directory, FileNameOf(tileset)), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(output, document, JsonOptions);
    }

    /// <summary>Reads <paramref name="tileset"/>'s record from <paramref name="directory"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The record is not this revision's, names another tileset, spells an unknown input or records no blocks.
    /// </exception>
    internal static RoomBlockAppearances Read(string directory, TilesetDefinition tileset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, FileNameOf(tileset));
        BlockAppearancesDocument document = JsonSerializer.Deserialize<BlockAppearancesDocument>(File.ReadAllBytes(path), JsonOptions)
            ?? throw new InvalidDataException($"Block appearance record {path} is empty.");
        if (document.Version != Version || document.Tileset is null)
            throw new InvalidDataException($"Block appearance record {path} is not revision {Version}.");
        if (document.Tileset != RoomBlockSheet.NameOf(tileset))
            throw new InvalidDataException($"Block appearance record {path} describes {document.Tileset}, not {RoomBlockSheet.NameOf(tileset)}.");
        try
        {
            Dictionary<string, string?[]?> blocks = document.Blocks ?? throw new InvalidDataException("A block appearance record lists no blocks.");
            if (document.Count < 0)
                throw new InvalidDataException($"A block appearance record holds {document.Count} blocks.");
            Dictionary<string, int> blockNamed = Enumerable.Range(0, document.Count).ToDictionary(RoomBlockPictureNames.OfBlock, StringComparer.Ordinal);
            // Each block's inputs, as the record spells them; none for a block no room shows, which the record does not list.
            ImmutableArray<AppearanceInput>[] inputs = [.. Enumerable.Repeat(ImmutableArray<AppearanceInput>.Empty, document.Count)];
            var shown = new List<int>();
            foreach ((string name, string?[]? tokens) in blocks.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (!blockNamed.TryGetValue(name, out int block))
                    throw new InvalidDataException($"A block appearance record of {document.Count} blocks lists {name}, not among them.");
                inputs[block] = ImmutableArray.CreateRange((tokens ?? throw new InvalidDataException($"{name} lists null inputs.")).Select(token =>
                    AppearanceInputSpellings.Parse(token ?? throw new InvalidDataException($"{name} lists a null input."))));
                shown.Add(block);
            }
            return new RoomBlockAppearances(inputs, shown);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid block appearance record {path}: {error.Message}", error);
        }
    }

    /// <summary>The inputs of each block a room shows, as the record spells them, by the name the block's pictures begin with.</summary>
    private static Dictionary<string, string?[]?> Tokens(RoomBlockAppearances appearances) =>
        Enumerable.Range(0, appearances.Count).Where(appearances.Shows)
            .ToDictionary(RoomBlockPictureNames.OfBlock, block => (string?[]?)[.. appearances.Of(block).Select(input => (string?)input.Token)], StringComparer.Ordinal);

    /// <summary>One tileset's record.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tileset">The tileset's installed name.</param>
    /// <param name="Count">How many blocks the tileset holds.</param>
    /// <param name="Blocks">
    /// The inputs of each block a room shows, by the name the block's pictures begin with; a block no room shows has no entry.
    /// </param>
    private sealed record BlockAppearancesDocument(int Version, string? Tileset, int Count, Dictionary<string, string?[]?>? Blocks);

    /// <summary>Strict camel-case record fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
    };
}
