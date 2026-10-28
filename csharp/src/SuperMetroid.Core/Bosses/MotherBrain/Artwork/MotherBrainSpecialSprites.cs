using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's special sprite sheets for legs, Baby, attack restoration and the exploded door: their installed
/// validation, lookup and the character each page shows from.
/// </summary>
public static class MotherBrainSpecialSprites
{
    /// <summary>Copies the installed sheets after requiring exactly the four complete native sheets.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="sheets"/> is null.</exception>
    /// <exception cref="InvalidDataException">A required sheet is missing/null, its compiled byte length differs from the native page extent, or the mapping contains additional identities.</exception>
    internal static Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> Validated(
        IReadOnlyDictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas> sheets)
    {
        Ensure.NotNull(sheets);
        var copied = new Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas>(sheets);
        if (copied.Count != MotherBrainSpecialSpriteArtworkDefinitions.All.Count ||
            MotherBrainSpecialSpriteArtworkDefinitions.All.Any(definition =>
                !copied.TryGetValue(definition.Sheet, out RoomCharacterAtlas? artwork) ||
                artwork is null || artwork.Characters.Length != definition.ByteCount))
            throw new InvalidDataException(
                "Mother Brain special artwork requires all four nonnull native source sheets with complete transfer pages.");
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
        ? sheet.FirstCharacter + page * MotherBrainSpecialSpriteSheetDefinition.CharactersPerPage
        : throw new ArgumentOutOfRangeException(nameof(page));
}
