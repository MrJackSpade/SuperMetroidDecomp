namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous, tile-aligned visual source for Torizo $814B uploads.</summary>
/// <param name="SourceAddress">ROM byte address from which the native upload reads this tile page.</param>
/// <param name="ByteCount">Exact byte length of the contiguous, tile-aligned source region.</param>
/// <param name="FileName">Installed PNG asset that supplies the page's replacement characters.</param>
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
        /// <summary>Shared death and recovery tiles used by both Torizo variants.</summary>
        SharedDeath,
        /// <summary>Alternating tile frames used while the statue crumbles.</summary>
        StatueCrumble,
        /// <summary>Bomb Torizo's left-facing attack and death tile page.</summary>
        LeftAttack,
        /// <summary>Bomb Torizo's right-facing attack and death tile page.</summary>
        RightAttack,
        /// <summary>Golden Torizo's initial awakening tile upload.</summary>
        GoldenAwakening,
        /// <summary>Golden Torizo's alternate left-facing attack page.</summary>
        GoldenLeftAttack,
        /// <summary>Golden Torizo's alternate right-facing attack page.</summary>
        GoldenRightAttack,
        /// <summary>Chozo fragments uploaded by the statue-crumble PLM.</summary>
        ChozoDebris,
    }

    /// <summary>Eight fixed artwork pages in the order expected by manifests and content identity.</summary>
    internal static PageSequence All => default;

    /// <summary>Read-only ordered view of the eight Torizo instruction tile pages.</summary>
    internal readonly struct PageSequence : IReadOnlyList<TorizoInstructionTileSheetDefinition>
    {
        /// <summary>Gets the fixed number of upload pages.</summary>
        public int Count => 8;
        /// <summary>Gets the page count for callers using sequence terminology.</summary>
        public int Length => Count;
        /// <summary>Gets a tile-page definition by its stable artwork role.</summary>
        /// <param name="index">Zero-based position in the upload and content-hash order.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the eight defined roles.</exception>
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
        /// <summary>Enumerates tile-page definitions in their stable upload order.</summary>
        public IEnumerator<TorizoInstructionTileSheetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
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

    /// <summary>Loaded replacement atlases paired with the definitions in <see cref="TorizoInstructionVramArtworkDefinitions.All"/> order.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Creates the artwork set after verifying that every required page has its exact native byte count.</summary>
    /// <param name="pages">Loaded character atlases in the fixed definition order.</param>
    /// <exception cref="ArgumentNullException">The array is null.</exception>
    /// <exception cref="InvalidDataException">A required page is missing or has a transfer length different from its source definition.</exception>
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

    /// <summary>Resolves a requested source range wholly contained in one installed tile page.</summary>
    /// <param name="sourceAddress">ROM byte address at the start of the requested character range.</param>
    /// <param name="byteCount">Positive number of bytes requested from that address.</param>
    /// <param name="characters">Receives a memory slice from the matching page, or the default value on failure.</param>
    /// <returns>True when the complete byte range is within a known Torizo upload page.</returns>
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
