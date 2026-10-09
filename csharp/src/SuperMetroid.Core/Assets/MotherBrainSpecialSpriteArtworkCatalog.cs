namespace SuperMetroid.Core.Assets;

/// <summary>Installed editable pages for legs, Baby, attack restoration, and the exploded door.</summary>
public sealed class MotherBrainSpecialSpriteArtworkCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        "enemy-mother-brain-special-v1", sheets, atlas => atlas.Transfer);

    private readonly Dictionary<int, RoomCharacterAtlas> sheets;

    /// <summary>Installs the four complete native OBJ source sheets for phase-two legs, Baby Metroid, restored attack characters, and the exploded escape door, copying the source lookup while retaining each compiled atlas.</summary>
    /// <param name="sheets">Exact full-source-address mapping: $B7:9000 has eight $0200-byte pages, $B1:8800 and $B7:A000 four each, and $AB:F400 two; individual transfer addresses inside a sheet are not separate keys.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sheets"/> is null.</exception>
    /// <exception cref="InvalidDataException">A required sheet is missing/null, its compiled byte length differs from the native page extent, or the mapping contains additional identities.</exception>
    public MotherBrainSpecialSpriteArtworkCatalog(
        IReadOnlyDictionary<int, RoomCharacterAtlas> sheets)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        this.sheets = new Dictionary<int, RoomCharacterAtlas>(sheets);
        if (this.sheets.Count != MotherBrainSpecialSpriteArtworkDefinitions.All.Count ||
            MotherBrainSpecialSpriteArtworkDefinitions.All.Any(definition =>
                !this.sheets.TryGetValue(definition.SourceAddress, out RoomCharacterAtlas? artwork) ||
                artwork is null || artwork.Transfer.Length != definition.ByteCount))
            throw new InvalidDataException(
                "Mother Brain special artwork requires all four nonnull native source sheets with complete transfer pages.");
    }

    /// <summary>Returns the validated installed characters for a native transfer list.</summary>
    public RoomCharacterAtlas Get(int sourceAddress) =>
        sheets.TryGetValue(sourceAddress, out RoomCharacterAtlas? artwork)
            ? artwork
            : throw new InvalidDataException(
                $"Installed Mother Brain special artwork is missing source ${sourceAddress:X6}.");
}
