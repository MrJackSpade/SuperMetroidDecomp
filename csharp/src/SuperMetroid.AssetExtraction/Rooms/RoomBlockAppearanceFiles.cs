using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed record of the appearance inputs each block of a tileset's rooms depends on (#1466), one file per tileset
/// beside the block sheets: for every block, by its index in what a room of the tileset loads, the inputs its picture
/// changes with over the tileset's artwork and, where a room of the tileset shows them, over the sprites' artwork. It holds
/// no characters, tile positions or colours; those are drawn into the block sheet, which a player override replaces.
/// </summary>
public static class RoomBlockAppearanceFiles
{
    /// <summary>The schema revision.</summary>
    private const int Version = 1;

    /// <summary>The installed file name of <paramref name="tileset"/>'s record.</summary>
    public static string FileNameOf(TilesetDefinition tileset) => $"{RoomBlockSheet.NameOf(tileset)}.block-appearances.json";

    /// <summary>Writes <paramref name="tileset"/>'s record of <paramref name="appearances"/> into <paramref name="directory"/>.</summary>
    /// <exception cref="IOException">The file already exists or cannot be written.</exception>
    internal static void Write(string directory, TilesetDefinition tileset, RoomBlockAppearances appearances)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(appearances);
        var document = new BlockAppearancesDocument(Version, RoomBlockSheet.NameOf(tileset), Tokens(appearances, RoomBlockArtwork.Tileset),
            appearances.ShowsSprites ? Tokens(appearances, RoomBlockArtwork.Sprites) : null);
        using var output = new FileStream(Path.Combine(directory, FileNameOf(tileset)), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(output, document, JsonOptions);
    }

    /// <summary>Reads <paramref name="tileset"/>'s record from <paramref name="directory"/>.</summary>
    /// <exception cref="InvalidDataException">The record is not this revision's, names another tileset or spells an unknown input.</exception>
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
            return new RoomBlockAppearances(Inputs(document.Blocks), document.SpriteBlocks is null ? null : Inputs(document.SpriteBlocks));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid block appearance record {path}: {error.Message}", error);
        }

        // Each block's inputs, as the record spells them.
        static ImmutableArray<AppearanceInput>[] Inputs(string?[]?[]? blocks) =>
            [.. (blocks ?? throw new InvalidDataException("A block appearance record lists no blocks.")).Select((tokens, block) =>
                ImmutableArray.CreateRange((tokens ?? throw new InvalidDataException($"Block {block} lists no inputs.")).Select(token =>
                    AppearanceInputTokens.Parse(token ?? throw new InvalidDataException($"Block {block} lists a null input."),
                        EncounterCharacterPlacements.Catalog.LookNamed))))];
    }

    /// <summary>Each block's inputs over <paramref name="artwork"/>, as the record spells them.</summary>
    private static string?[]?[] Tokens(RoomBlockAppearances appearances, RoomBlockArtwork artwork) =>
        [.. Enumerable.Range(0, appearances.Count).Select(block => appearances.Of(block, artwork).Select(input => (string?)input.Token).ToArray())];

    /// <summary>One tileset's record.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tileset">The tileset's installed name.</param>
    /// <param name="Blocks">Each block's inputs over the tileset's artwork, by block index.</param>
    /// <param name="SpriteBlocks">Each block's inputs over the sprites' artwork, or null where no room of the tileset shows it.</param>
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
