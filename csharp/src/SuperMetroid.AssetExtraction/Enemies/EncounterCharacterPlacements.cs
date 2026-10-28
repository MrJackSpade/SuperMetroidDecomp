using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where each encounter's characters land among the sprite characters (#1392, #1406): the cartridge's placement, which
/// extraction alone knows, paired with what each encounter shows to make the installed upload catalogue.
/// </summary>
internal static class EncounterCharacterPlacements
{

    /// <summary>Every encounter's uploads, each placed where the cartridge shows it.</summary>
    internal static EncounterCharacterCatalog Catalog { get; } = new(EncounterCharacterDefinitions.Encounters.ToDictionary(
        entry => entry.Key, entry => (IReadOnlyList<SpriteArtworkPlacement>)[.. entry.Value.Select(Placed)]));

    /// <summary>The upload of <paramref name="source"/> where the cartridge shows it.</summary>
    /// <exception cref="InvalidDataException">The cartridge shows no such source among the sprite characters.</exception>
    internal static SpriteArtworkPlacement Placed(SpriteArtworkSource source) => source switch
    {
        InstalledArtworkCharacters { Artwork: >= InstalledArtworkId.GunshipLiftoffFirstTiles and <= InstalledArtworkId.GunshipLiftoffFirstTiles + 4 } liftoff =>
            new(source, EncounterCharacterPlacementDefinitions.GunshipFirstCharacter + (liftoff.Artwork - InstalledArtworkId.GunshipLiftoffFirstTiles) * Characters(GunshipLiftoffTransferDefinitions.ByteCount),
                Characters(GunshipLiftoffTransferDefinitions.ByteCount)),
        InstalledArtworkCharacters { Artwork: InstalledArtworkId.EscapeTimerFirstTiles } => Timer(source, EscapeTimerTilePage.First),
        InstalledArtworkCharacters { Artwork: InstalledArtworkId.EscapeTimerSecondTiles } => Timer(source, EscapeTimerTilePage.Second),
        TorizoArtworkSwapCharacters swap =>
            new(source, EncounterCharacterPlacementDefinitions.TorizoDestinations[swap.Entry].FirstCharacter, Characters(TorizoArtworkSwapDefinitions.Entries[swap.Entry].ByteCount)),
        CrocomireSkeletonChunkCharacters chunk => new(source, SkeletonFirstCharacter(chunk.Entry),
            Characters(SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireSkeletonTransferDefinitions.ChunkByteCount)),
        EnemyTileCharacters { Enemy: EnemyDefinitionId.Evir } => new(source, EncounterCharacterPlacementDefinitions.EvirFirstCharacter,
            Characters(SuperMetroid.Core.Bosses.Draygon.DraygonIntroPresentationDefinitions.EvirTilesByteCount)),
        CeresDoorCharacters => new(source, EncounterCharacterPlacementDefinitions.CeresDoorFirstCharacter, Characters(SuperMetroid.Core.Areas.Ceres.Artwork.CeresDoorVisualDefinitions.CharacterBytes)),
        CeresEscapeTileCharacters warning => new(source,
            SuperMetroid.Core.Areas.Ceres.Artwork.CeresEscapeOverlayTilemapDefinitions.WarningFirstCharacter + Characters(warning.Offset), Characters(warning.ByteCount)),
        MotherBrainSpecialSpriteCharacters special => new(source,
            MotherBrainSpecialSpriteArtworkDefinitions.All[special.Sheet].PageFirstCharacter(special.Page), Characters(special.ByteCount)),
        MotherBrainCorpseCharacters corpse => new(source,
            MotherBrainCorpseArtworkDefinitions.PageFirstCharacter(corpse.Page), Characters(MotherBrainCorpseArtworkDefinitions.PageByteCount)),
        MotherBrainEscapeTextCharacters text => new(source,
            EscapeTextArtworkDefinitions.PageFirstCharacter(text.Page), Characters(EscapeTextArtworkDefinitions.PageByteCount(text.Page))),
        _ => throw new InvalidDataException($"The cartridge shows no {source} among the sprite characters."),
    };

    /// <summary>Escape timer page <paramref name="page"/>, shown as <paramref name="source"/>.</summary>
    private static SpriteArtworkPlacement Timer(SpriteArtworkSource source, EscapeTimerTilePage page) =>
        new(source, EscapeTimerTilePages.FirstCharacterOf(page), Characters(EscapeTimerTilePages.ByteCountOf(page)));

    /// <summary>The whole sprite characters <paramref name="byteCount"/> bytes of the cartridge's copy hold.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters.</exception>
    private static int Characters(int byteCount) => byteCount % SpriteArtworkPlacements.CharacterByteCount == 0
        ? byteCount / SpriteArtworkPlacements.CharacterByteCount
        : throw new InvalidDataException($"{byteCount} bytes are not whole sprite characters.");

    /// <summary>The character skeleton chunk <paramref name="chunk"/> begins at: rows of the first region, then of the second.</summary>
    private static int SkeletonFirstCharacter(int chunk) => chunk < EncounterCharacterPlacementDefinitions.SkeletonFirstRegionRows
        ? EncounterCharacterPlacementDefinitions.SkeletonFirstRegionCharacter + chunk * EncounterCharacterPlacementDefinitions.SkeletonCharactersPerRow
        : EncounterCharacterPlacementDefinitions.SkeletonSecondRegionCharacter + (chunk - EncounterCharacterPlacementDefinitions.SkeletonFirstRegionRows) * EncounterCharacterPlacementDefinitions.SkeletonCharactersPerRow;
}
