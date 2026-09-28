namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous, tile-aligned visual source for Torizo $814B uploads.</summary>
internal readonly record struct TorizoInstructionTileSheetDefinition(
    int SourceAddress, int ByteCount, string FileName);

/// <summary>Named PNG sources used by the compiled Bomb/Golden Torizo upload descriptors.</summary>
internal static class TorizoInstructionVramArtworkDefinitions
{
    /// <summary>Shared death/recovery characters at $AA:B0A5.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition SharedDeath =
        new(0xaab0a5, 0x0040, "torizo-shared-death-tiles.png");

    /// <summary>Alternating statue-crumble characters at $AA:B279..B378.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition StatueCrumble =
        new(0xaab279, 0x0100, "torizo-statue-crumble-tiles.png");

    /// <summary>Left-facing attack/death characters at $AA:B479..B5B8.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition LeftAttack =
        new(0xaab479, 0x0140, "torizo-left-attack-tiles.png");

    /// <summary>Right-facing attack/death characters at $AA:B679..B7B8.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition RightAttack =
        new(0xaab679, 0x0140, "torizo-right-attack-tiles.png");

    /// <summary>Golden Torizo's initial character upload at $AF:E200..E7FF.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenAwakening =
        new(0xafe200, 0x0600, "golden-torizo-awakening-tiles.png");

    /// <summary>Golden Torizo's alternate left-side attack characters at $AF:C800.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenLeftAttack =
        new(0xafc800, 0x0040, "golden-torizo-left-attack-tiles.png");

    /// <summary>Golden Torizo's alternate right-side attack characters at $AF:CA00.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenRightAttack =
        new(0xafca00, 0x0040, "golden-torizo-right-attack-tiles.png");

    private static readonly TorizoInstructionTileSheetDefinition[] Pages =
    [
        SharedDeath, StatueCrumble, LeftAttack, RightAttack,
        GoldenAwakening, GoldenLeftAttack, GoldenRightAttack,
    ];

    internal static ReadOnlySpan<TorizoInstructionTileSheetDefinition> All => Pages;
}

/// <summary>
/// Editable indexed characters for the native Torizo instruction uploads.
/// Address and destination ownership remain in compiled control data.
/// </summary>
public sealed class TorizoInstructionVramArtwork
{
    private readonly RoomCharacterAtlas[] pages;

    internal TorizoInstructionVramArtwork(RoomCharacterAtlas[] pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        if (pages.Length != TorizoInstructionVramArtworkDefinitions.All.Length)
            throw new InvalidDataException("Torizo instruction artwork requires every tile page.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Transfer.Length !=
                TorizoInstructionVramArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Torizo instruction tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        for (int index = 0; index < pages.Length; index++)
        {
            TorizoInstructionTileSheetDefinition page =
                TorizoInstructionVramArtworkDefinitions.All[index];
            int offset = sourceAddress - page.SourceAddress;
            if (offset < 0 || byteCount <= 0 ||
                offset > page.ByteCount - byteCount)
                continue;
            characters = pages[index].Transfer.Slice(offset, byteCount);
            return true;
        }
        characters = default;
        return false;
    }
}
