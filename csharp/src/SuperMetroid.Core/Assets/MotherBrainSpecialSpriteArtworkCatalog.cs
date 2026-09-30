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
        if (sheets.Count != MotherBrainSpecialSpriteArtworkDefinitions.All.Count ||
            MotherBrainSpecialSpriteArtworkDefinitions.All
                .Any(definition => !sheets.ContainsKey(definition.SourceAddress)))
            throw new InvalidDataException(
                "Mother Brain special artwork requires all four native source sheets.");
        this.sheets = new Dictionary<int, RoomCharacterAtlas>(sheets);
    }

    /// <summary>Returns the validated installed characters for a native transfer list.</summary>
    public RoomCharacterAtlas Get(MotherBrainSpecialSpriteSheetDefinition definition) =>
        sheets.TryGetValue(definition.SourceAddress, out RoomCharacterAtlas? artwork)
            ? artwork
            : throw new InvalidDataException(
                $"Installed enemy artwork is missing {definition.FileName}.");
}
