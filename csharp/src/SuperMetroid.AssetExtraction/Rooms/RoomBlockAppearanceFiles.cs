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
/// beside the block sheets: for every block, by its index in what a room of the tileset loads, the inputs its picture
/// changes with over each artwork a room of the tileset shows its blocks over, the tileset's own and the sprites' (#1648);
/// or no entry at all for a block no room of the tileset shows, which the sheet does not picture (#1636). It holds no characters,
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
    /// revision 8 names a block animation's frames by its block animation, not by the hardware's tiles (#1726).
    /// </summary>
    private const int Version = 8;

    /// <summary>The installed file name of <paramref name="tileset"/>'s record.</summary>
    public static string FileNameOf(TilesetDefinition tileset) => $"{RoomBlockSheet.NameOf(tileset)}.block-appearances.json";

    /// <summary>Writes <paramref name="tileset"/>'s record of <paramref name="appearances"/> into <paramref name="directory"/>.</summary>
    /// <exception cref="IOException">The file already exists or cannot be written.</exception>
    internal static void Write(string directory, TilesetDefinition tileset, RoomBlockAppearances appearances)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(appearances);
        var document = new BlockAppearancesDocument(Version, RoomBlockSheet.NameOf(tileset), Tokens(appearances, RoomBlockArtwork.Tileset),
            Tokens(appearances, RoomBlockArtwork.Sprites));
        using var output = new FileStream(Path.Combine(directory, FileNameOf(tileset)), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(output, document, JsonOptions);
    }

    /// <summary>Reads <paramref name="tileset"/>'s record from <paramref name="directory"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The record is not this revision's, names another tileset, spells an unknown input, records the blocks over no artwork,
    /// or records a block over one artwork that it records as shown by no room over the other.
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
            string?[]?[] blocks = document.Blocks ?? document.SpriteBlocks
                ?? throw new InvalidDataException("A block appearance record lists no blocks over any artwork.");
            int[] shown = [.. Enumerable.Range(0, blocks.Length).Where(block => blocks[block] is not null)];
            if (document.Blocks is { } overTileset && document.SpriteBlocks is { } overSprites && (overSprites.Length != overTileset.Length
                || Enumerable.Range(0, overTileset.Length).Any(block => (overTileset[block] is null) != (overSprites[block] is null))))
                throw new InvalidDataException("A block appearance record shows other blocks over the sprites' artwork than over the tileset's.");
            return new RoomBlockAppearances(document.Blocks is null ? null : Inputs(document.Blocks),
                document.SpriteBlocks is null ? null : Inputs(document.SpriteBlocks), shown);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid block appearance record {path}: {error.Message}", error);
        }

        // Each block's inputs, as the record spells them; none for a block no room shows.
        static ImmutableArray<AppearanceInput>[] Inputs(string?[]?[] blocks) =>
            [.. blocks.Select((tokens, block) => tokens is null ? ImmutableArray<AppearanceInput>.Empty
                : ImmutableArray.CreateRange(tokens.Select(token =>
                    AppearanceInputSpellings.Parse(token ?? throw new InvalidDataException($"Block {block} lists a null input.")))))];
    }

    /// <summary>
    /// Each block's inputs over <paramref name="artwork"/>, as the record spells them, null for a block no room shows; or null
    /// where no room of the tileset shows its blocks over that artwork (#1648).
    /// </summary>
    private static string?[]?[]? Tokens(RoomBlockAppearances appearances, RoomBlockArtwork artwork) =>
        appearances.ShowsOver(artwork)
            ? [.. Enumerable.Range(0, appearances.Count).Select(block => appearances.Shows(block)
                ? appearances.Of(block, artwork).Select(input => (string?)input.Token).ToArray() : null)]
            : null;

    /// <summary>One tileset's record.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tileset">The tileset's installed name.</param>
    /// <param name="Blocks">
    /// Each block's inputs over the tileset's artwork, by block index, null for a block no room shows; or null where no room of
    /// the tileset shows its blocks over that artwork.
    /// </param>
    /// <param name="SpriteBlocks">
    /// Each block's inputs over the sprites' artwork, null for a block no room shows; or null where no room of the tileset
    /// shows that artwork.
    /// </param>
    private sealed record BlockAppearancesDocument(int Version, string? Tileset, string?[]?[]? Blocks, string?[]?[]? SpriteBlocks);

    /// <summary>Strict camel-case record fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
    };
}
