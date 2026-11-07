using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's special sprite sheets for legs, Baby, attack restoration and the exploded door: their installed
/// validation, lookup and the character each page shows from.
/// </summary>
public static class MotherBrainSpecialSprites
{
    /// <summary>Every page holds sixteen characters, $200 bytes.</summary>
    private const int SheetPageByteCount = 512;

    /// <summary>The sprite characters one page shows.</summary>
    private const int SheetCharactersPerPage = 16;

    /// <summary>Which sheet each special sheet is, and the sprite character its first page shows from.</summary>
    private static readonly Dictionary<MotherBrainSpecialSpriteSheetDefinition, MotherBrainSpecialSheetPlacement> Placements = new()
    {
        [MotherBrainSpecialSpriteSheetDefinitions.Legs] = new(MotherBrainSpritePageSheet.Legs, 320),
        [MotherBrainSpecialSpriteSheetDefinitions.BabyMetroid] = new(MotherBrainSpritePageSheet.BabyMetroid, 448),
        [MotherBrainSpecialSpriteSheetDefinitions.Attack] = new(MotherBrainSpritePageSheet.Attack, 448),
        [MotherBrainSpecialSpriteSheetDefinitions.ExplodedDoor] = new(MotherBrainSpritePageSheet.ExplodedDoor, 256),
    };

    /// <summary>The page geometry every special sheet shares.</summary>
    extension(MotherBrainSpecialSpriteSheetDefinition)
    {
        /// <summary>Every page holds sixteen characters, $200 bytes.</summary>
        internal static int PageByteCount => SheetPageByteCount;
    }

    /// <summary>The import's knowledge of one special sheet: which sheet it is, where its pages show and its length.</summary>
    extension(MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        /// <summary>The sheet this is.</summary>
        /// <exception cref="InvalidDataException">The definition is not one of the four special sheets.</exception>
        internal MotherBrainSpritePageSheet Sheet => Placement(sheet).Sheet;

        /// <summary>The sprite character the sheet's first page shows from; each later page follows the one before.</summary>
        /// <exception cref="InvalidDataException">The definition is not one of the four special sheets.</exception>
        internal int FirstCharacter => Placement(sheet).FirstCharacter;

        /// <summary>Total tile-aligned source length encoded by this sheet.</summary>
        internal int ByteCount => checked(sheet.PageCount * SheetPageByteCount);
    }

    /// <summary>The sheet identity and first character of one of the four special sheets.</summary>
    /// <exception cref="InvalidDataException"><paramref name="sheet"/> is not one of the four special sheets.</exception>
    private static MotherBrainSpecialSheetPlacement Placement(MotherBrainSpecialSpriteSheetDefinition sheet) =>
        Placements.TryGetValue(sheet, out var placement)
            ? placement
            : throw new InvalidDataException($"{sheet.FileName} is not one of Mother Brain's special sheets.");

    /// <summary>Copies the installed sheets after requiring exactly the four complete sheets.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="sheets"/> is null.</exception>
    /// <exception cref="InvalidDataException">A required sheet is missing/null, its compiled byte length differs from the page extent, or the mapping contains additional identities.</exception>
    internal static Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> Validated(
        IReadOnlyDictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> sheets)
    {
        Ensure.NotNull(sheets);
        var copied = new Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas>(sheets);
        if (copied.Count != MotherBrainSpecialSpriteSheetDefinitions.All.Count ||
            MotherBrainSpecialSpriteSheetDefinitions.All.Any(definition =>
                !copied.TryGetValue(definition.Sheet, out RoomCharacterAtlas? artwork) ||
                artwork is null || artwork.Characters.Length != definition.ByteCount))
            throw new InvalidDataException(
                "Mother Brain special artwork requires all four nonnull source sheets with complete transfer pages.");
        return copied;
    }

    /// <summary>Returns the validated installed characters of <paramref name="sheet"/>.</summary>
    internal static RoomCharacterAtlas Get(Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> sheets, MotherBrainSpritePageSheet sheet) =>
        sheets.TryGetValue(sheet, out RoomCharacterAtlas? artwork)
            ? artwork
            : throw new InvalidDataException(
                $"Installed Mother Brain special artwork is missing the {sheet} sheet.");

    /// <summary>The sprite character page <paramref name="page"/> of <paramref name="sheet"/> shows from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet has no such page.</exception>
    public static int PageFirstCharacter(MotherBrainSpecialSpriteSheetDefinition sheet, int page) => (uint)page < sheet.PageCount
        ? sheet.FirstCharacter + page * SheetCharactersPerPage
        : throw new ArgumentOutOfRangeException(nameof(page));
}
