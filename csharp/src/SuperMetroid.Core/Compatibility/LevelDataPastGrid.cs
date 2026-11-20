using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a room's installed level holds past its grid (#1469, #2550): the rest of the room's level data that followed the
/// grid, read as blocks, and how many blocks the room's level space holds past the grid in all, the ones past the
/// following data being solid. A probe or a room object that reaches past the room's last block finds these first.
/// </summary>
/// <remarks>
/// Decoded once by the import from the level the game decompressed over its solid-filled level space, and consulted only
/// by <see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>, which each room load builds from it. The service is written in
/// its own terrain type, which the room module supplies. Evidence: the original game filled the room's whole level
/// space with solid blocks before decompressing the room over its beginning, so the data the decompression left past the
/// grid and then the solid fill lie there; a door's bottom-edge column fill below a short room shows the following data.
/// </remarks>
/// <typeparam name="TTerrain">The room's terrain block.</typeparam>
[CompatibilityService("a probe past a room's grid finds the level data that followed the grid, then solid blocks to the end of the room's level space")]
public sealed record LevelDataPastGrid<TTerrain>
{
    /// <summary>Keeps the following data and the level space past the grid.</summary>
    /// <param name="blocks">The level data that followed the grid, read as blocks.</param>
    /// <param name="spaceBlocks">How many blocks the room's level space holds past its grid, the following data among them.</param>
    /// <exception cref="InvalidDataException">The following data exceeds the space past the grid.</exception>
    public LevelDataPastGrid(ImmutableArray<TTerrain> blocks, int spaceBlocks)
    {
        if (spaceBlocks < blocks.Length)
            throw new InvalidDataException(
                $"A room level holds {blocks.Length} blocks of data past its grid in a space of {spaceBlocks}.");
        Blocks = blocks;
        SpaceBlocks = spaceBlocks;
    }

    /// <summary>The level data that followed the grid, read as blocks.</summary>
    public ImmutableArray<TTerrain> Blocks { get; }

    /// <summary>How many blocks the room's level space holds past its grid; those past <see cref="Blocks"/> are solid.</summary>
    public int SpaceBlocks { get; }
}
