using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Installed editable pages for legs, Baby, attack restoration, and the exploded door.</summary>
public sealed class MotherBrainSpecialSpriteArtworkCatalog
{
    /// <summary>Canonical selected presentation data, keyed by each sheet's place in <see cref="HashOrder"/>; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        "enemy-mother-brain-special-v1", sheets.ToDictionary(pair => HashOrder.PositionOf(pair.Key), pair => pair.Value), atlas => atlas.Characters);

    /// <summary>The special sheets in the order that keys their content identity.</summary>
    private static readonly IdentitySequence<MotherBrainSpritePageSheet> HashOrder = new(
        MotherBrainSpritePageSheet.Legs, MotherBrainSpritePageSheet.BabyMetroid,
        MotherBrainSpritePageSheet.Attack, MotherBrainSpritePageSheet.ExplodedDoor);

    /// <summary>The four installed sheets, by the sheet each is.</summary>
    private readonly Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> sheets;

    /// <summary>Installs the four complete sheets for phase-two legs, Baby Metroid, restored attack characters, and the exploded escape door, copying the lookup while retaining each compiled atlas.</summary>
    /// <param name="sheets">Exactly the four sheets of <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>: legs with eight $0200-byte pages, Baby Metroid and attack four each, and the exploded door two.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sheets"/> is null.</exception>
    /// <exception cref="InvalidDataException">A required sheet is missing/null, its compiled byte length differs from the native page extent, or the mapping contains additional identities.</exception>
    public MotherBrainSpecialSpriteArtworkCatalog(
        IReadOnlyDictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> sheets)
    {
        Ensure.NotNull(sheets);
        this.sheets = new Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas>(sheets);
        if (this.sheets.Count != MotherBrainSpecialSpriteArtworkDefinitions.All.Count ||
            MotherBrainSpecialSpriteArtworkDefinitions.All.Any(definition =>
                !this.sheets.TryGetValue(definition.Sheet, out RoomCharacterAtlas? artwork) ||
                artwork is null || artwork.Characters.Length != definition.ByteCount))
            throw new InvalidDataException(
                "Mother Brain special artwork requires all four nonnull native source sheets with complete transfer pages.");
    }

    /// <summary>Returns the validated installed characters of <paramref name="sheet"/>.</summary>
    public RoomCharacterAtlas Get(MotherBrainSpritePageSheet sheet) =>
        sheets.TryGetValue(sheet, out RoomCharacterAtlas? artwork)
            ? artwork
            : throw new InvalidDataException(
                $"Installed Mother Brain special artwork is missing the {sheet} sheet.");
}
