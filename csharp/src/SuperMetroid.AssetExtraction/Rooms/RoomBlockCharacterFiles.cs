using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed record of the characters each block of a block set reads (#1431), one file per block set beside the
/// block sheets: for every block, by its index in what a room of the set loads, the runs of characters its picture reads.
/// It holds no tile positions, colours or mirroring; those are drawn into the block sheet, which a player override replaces.
/// </summary>
public static class RoomBlockCharacterFiles
{
    /// <summary>The schema revision.</summary>
    private const int Version = 1;

    /// <summary>The installed file name of <paramref name="set"/>'s record.</summary>
    public static string FileNameOf(RoomBlockSet set) => $"{set}.block-characters.json";

    /// <summary>Writes <paramref name="characters"/>' record into <paramref name="directory"/>.</summary>
    /// <exception cref="IOException">The file already exists or cannot be written.</exception>
    internal static void Write(string directory, RoomBlockCharacters characters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(characters);
        var document = new BlockCharactersDocument(Version, characters.Set.ToString(),
            [.. Enumerable.Range(0, characters.Count).Select(block => characters.RunsOf(block)
                .Select(run => new RunDocument(run.FirstCharacter, run.CharacterCount)).ToArray())]);
        using var output = new FileStream(Path.Combine(directory, FileNameOf(characters.Set)), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(output, document, JsonOptions);
    }

    /// <summary>Reads <paramref name="set"/>'s record from <paramref name="directory"/>.</summary>
    /// <exception cref="InvalidDataException">The record is not this revision's, names another block set or describes invalid runs.</exception>
    internal static RoomBlockCharacters Read(string directory, RoomBlockSet set)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, FileNameOf(set));
        BlockCharactersDocument document = JsonSerializer.Deserialize<BlockCharactersDocument>(File.ReadAllBytes(path), JsonOptions)
            ?? throw new InvalidDataException($"Block character record {path} is empty.");
        if (document.Version != Version || document.Blocks is null)
            throw new InvalidDataException($"Block character record {path} is not revision {Version}.");
        if (document.Set != set.ToString())
            throw new InvalidDataException($"Block character record {path} describes {document.Set}, not {set}.");
        try
        {
            return new RoomBlockCharacters(set, document.Blocks.Select((runs, block) =>
                (runs ?? throw new InvalidDataException($"Block {block} of record {path} has no runs."))
                    .Select(run => run is null ? throw new InvalidDataException($"Block {block} of record {path} has a null run.")
                        : new RoomBlockCharacterRun(run.First, run.Count))));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid block character record {path}: {error.Message}", error);
        }
    }

    /// <summary>One block set's record.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Set">The block set's name.</param>
    /// <param name="Blocks">Each block's runs, by block index.</param>
    private sealed record BlockCharactersDocument(int Version, string Set, RunDocument?[]?[] Blocks);

    /// <summary>One run of characters a block reads.</summary>
    /// <param name="First">The first character, as tile words number it.</param>
    /// <param name="Count">The characters from it the run spans.</param>
    private sealed record RunDocument(int First, int Count);

    /// <summary>Strict camel-case record fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = false,
    };
}
