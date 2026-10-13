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

    /// <summary>PLM-uploaded Bomb Torizo Chozo fragments, <c>Tiles_BombTorizosCrumblingChozo</c> at $AD:B200.</summary>
    internal static readonly TorizoInstructionTileSheetDefinition ChozoDebris =
        new(TorizoInstructionTileRomData.ChozoDebrisSource,
            TorizoInstructionTileRomData.ChozoDebrisByteCount, "torizo-chozo-debris-tiles.png");

    /// <summary>Mutually exclusive artwork roles in the installed manifest and content-hash order.</summary>
    private enum PageRole
    {
        /// <summary>Characters shared by the death and recovery uploads.</summary>
        SharedDeath,
        /// <summary>Alternating statue crumble characters.</summary>
        StatueCrumble,
        /// <summary>Left-facing attack and death characters.</summary>
        LeftAttack,
        /// <summary>Right-facing attack and death characters.</summary>
        RightAttack,
        /// <summary>Golden Torizo's awakening characters.</summary>
        GoldenAwakening,
        /// <summary>Golden Torizo's alternate left attack characters.</summary>
        GoldenLeftAttack,
        /// <summary>Golden Torizo's alternate right attack characters.</summary>
        GoldenRightAttack,
        /// <summary>Bomb Torizo's PLM-uploaded Chozo debris characters.</summary>
        ChozoDebris,
    }

    /// <summary>Enumerates the eight tile pages in stable installed-manifest and hash order.</summary>
    internal static PageSequence All => default;

    /// <summary>Indexable view of the fixed set of Torizo instruction tile sources.</summary>
    internal readonly struct PageSequence : IReadOnlyList<TorizoInstructionTileSheetDefinition>
    {
        /// <summary>Gets the number of required tile sheets.</summary>
        public int Count => 8;
        /// <summary>Gets the sequence length for callers using array-style access.</summary>
        public int Length => Count;
        /// <summary>Gets the tile sheet assigned to a stable page-role index.</summary>
        public TorizoInstructionTileSheetDefinition this[int index] => (PageRole)index switch
        {
            PageRole.SharedDeath => SharedDeath,
            PageRole.StatueCrumble => StatueCrumble,
            PageRole.LeftAttack => LeftAttack,
            PageRole.RightAttack => RightAttack,
            PageRole.GoldenAwakening => GoldenAwakening,
            PageRole.GoldenLeftAttack => GoldenLeftAttack,
            PageRole.GoldenRightAttack => GoldenRightAttack,
            PageRole.ChozoDebris => ChozoDebris,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Yields every required tile sheet in page-role order.</summary>
        public IEnumerator<TorizoInstructionTileSheetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same page-role order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
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

    /// <summary>Validated selected character transfers in tile-sheet definition order.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Copies and validates one installed character atlas for every required Torizo upload page.</summary>
    internal TorizoInstructionVramArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
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

    /// <summary>Finds the selected bytes for a native upload range wholly contained in one tile page.</summary>
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
