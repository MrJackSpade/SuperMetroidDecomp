using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Installed editable pages for legs, Baby, attack restoration, and the exploded door.</summary>
public sealed class MotherBrainSpecialSpriteArtworkCatalog
{

    /// <summary>The four installed sheets, by the sheet each is.</summary>
    private readonly Dictionary<MotherBrainSpriteSheet, RoomCharacterAtlas> sheets;

    /// <summary>Installs the four complete sheets for phase-two legs, Baby Metroid, restored attack characters, and the exploded escape door, copying the lookup while retaining each compiled atlas.</summary>
    /// <param name="sheets">Exactly the four sheets of <see cref="MotherBrainSpecialSpriteSheetDefinitions.All"/>: legs with eight 512-byte pages, Baby Metroid and attack four each, and the exploded door two.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sheets"/> is null.</exception>
    /// <exception cref="InvalidDataException">A required sheet is missing/null, its compiled byte length differs from the page extent, or the mapping contains additional identities.</exception>
    public MotherBrainSpecialSpriteArtworkCatalog(
        IReadOnlyDictionary<MotherBrainSpriteSheet, RoomCharacterAtlas> sheets) =>
        this.sheets = MotherBrainSpecialSprites.Validated(sheets);

    /// <summary>Returns the validated installed characters of <paramref name="sheet"/>.</summary>
    public RoomCharacterAtlas Get(MotherBrainSpriteSheet sheet) => MotherBrainSpecialSprites.Get(sheets, sheet);
}
