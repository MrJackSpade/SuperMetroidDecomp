namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous, tile-aligned visual source for Torizo $814B uploads.</summary>
internal readonly record struct TorizoInstructionTileSheetDefinition(
    int SourceAddress, int ByteCount, string FileName);

/// <summary>Named PNG sources used by the compiled Bomb/Golden Torizo upload descriptors.</summary>
internal static class TorizoInstructionVramArtworkDefinitions
{
    /// <summary>Shared death/recovery characters at $AA:B0A5.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition SharedDeath =
        new(TorizoInstructionTileRomData.SharedDeathSource,
            TorizoInstructionTileRomData.SharedDeathByteCount, "torizo-shared-death-tiles.png");

    /// <summary>Alternating statue-crumble characters at $AA:B279..B378.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition StatueCrumble =
        new(TorizoInstructionTileRomData.StatueCrumbleSource,
            TorizoInstructionTileRomData.StatueCrumbleByteCount, "torizo-statue-crumble-tiles.png");

    /// <summary>Left-facing attack/death characters at $AA:B479..B5B8.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition LeftAttack =
        new(TorizoInstructionTileRomData.LeftAttackSource,
            TorizoInstructionTileRomData.LeftAttackByteCount, "torizo-left-attack-tiles.png");

    /// <summary>Right-facing attack/death characters at $AA:B679..B7B8.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition RightAttack =
        new(TorizoInstructionTileRomData.RightAttackSource,
            TorizoInstructionTileRomData.RightAttackByteCount, "torizo-right-attack-tiles.png");

    /// <summary>Golden Torizo's initial character upload at $AF:E200..E7FF.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenAwakening =
        new(TorizoInstructionTileRomData.GoldenAwakeningSource,
            TorizoInstructionTileRomData.GoldenAwakeningByteCount, "golden-torizo-awakening-tiles.png");

    /// <summary>Golden Torizo's alternate left-side attack characters at $AF:C800.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenLeftAttack =
        new(TorizoInstructionTileRomData.GoldenLeftAttackSource,
            TorizoInstructionTileRomData.GoldenLeftAttackByteCount, "golden-torizo-left-attack-tiles.png");

    /// <summary>Golden Torizo's alternate right-side attack characters at $AF:CA00.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition GoldenRightAttack =
        new(TorizoInstructionTileRomData.GoldenRightAttackSource,
            TorizoInstructionTileRomData.GoldenRightAttackByteCount, "golden-torizo-right-attack-tiles.png");

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
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
        {
            content.Append("pages", pages.Length);
            foreach (RoomCharacterAtlas page in pages)
                content.Append("tiles", page.Transfer.Span);
        });

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
