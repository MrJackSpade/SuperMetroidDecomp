namespace SuperMetroid.Core.Assets;

/// <summary>Installed editable pages for legs, Baby, attack restoration, and the exploded door.</summary>
public sealed class MotherBrainSpecialSpriteArtworkCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        "enemy-mother-brain-special-v1", sheets, atlas => atlas.Transfer);

    private readonly Dictionary<int, RoomCharacterAtlas> sheets;

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
