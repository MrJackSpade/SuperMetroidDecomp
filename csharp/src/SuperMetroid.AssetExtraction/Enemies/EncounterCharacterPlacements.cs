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
    /// <summary>The character the gunship's first liftoff page begins at, $160, from the destination list at $A2:AC11.</summary>
    private const int GunshipFirstCharacter = 0x160;

    /// <summary>The character the Ceres door's characters begin at, $100.</summary>
    private const int CeresDoorFirstCharacter = 0x100;

    /// <summary>The character the Evir tiles Draygon's opening shows begin at: sixteen past the enemy tile area's start.</summary>
    private const int EvirFirstCharacter = EnemyGraphicsSet.EnemyTileAreaFirstCharacter + 0x10;

    /// <summary>The character Crocomire's skeleton's first character region begins at, $160 ($A4:99CB).</summary>
    private const int SkeletonFirstRegionCharacter = 0x160;

    /// <summary>The rows of the skeleton's first region ($A4:99CB..99D1); the rest go to its second.</summary>
    private const int SkeletonFirstRegionRows = 4;

    /// <summary>The character the skeleton's second character region begins at, $1E0 ($A4:99D3).</summary>
    private const int SkeletonSecondRegionCharacter = 0x1e0;

    /// <summary>The characters from one skeleton row to the next.</summary>
    private const int SkeletonCharactersPerRow = 16;

    /// <summary>The character each Torizo swap's characters begin at, in swap order ($AA, bank-$A0 $814B destinations).</summary>
    private static readonly int[] TorizoFirstCharacters =
    [
        0x130, 0x140, 0x1e7, 0x1f7, 0x1e5, 0x1f5, 0x1c8, 0x1ca,
        0x1d8, 0x1da, 0x1e8, 0x1f8, 0x19f, 0x1d8, 0x1d8, 0x1d8,
        0x1d8, 0x1d8, 0x1d8, 0x1d8, 0x1d8, 0x0d0, 0x1d8, 0x1d8,
        0x1d8, 0x1d8, 0x1d8, 0x1d8, 0x1d8, 0x1d8, 0x130, 0x140,
        0x130, 0x140, 0x130, 0x140, 0x130, 0x140, 0x130, 0x140,
        0x130, 0x140, 0x130, 0x140, 0x1d8, 0x1d8, 0x1d8, 0x1d8,
        0x1d8, 0x1d8, 0x1d8, 0x1d8,
    ];

    /// <summary>Every encounter's uploads, each placed where the cartridge shows it.</summary>
    internal static EncounterCharacterCatalog Catalog { get; } = new(EncounterCharacterDefinitions.Encounters.ToDictionary(
        entry => entry.Key, entry => (IReadOnlyList<SpriteCharacterPlacement>)[.. entry.Value.Select(Placed)]));

    /// <summary>The upload of <paramref name="source"/> where the cartridge shows it.</summary>
    /// <exception cref="InvalidDataException">The cartridge shows no such source among the sprite characters.</exception>
    internal static SpriteCharacterPlacement Placed(ObjCharacterSource source) => source switch
    {
        AssetObjCharacters { Asset: >= VramAssetId.GunshipLiftoffFirstTiles and <= VramAssetId.GunshipLiftoffFirstTiles + 4 } liftoff =>
            new(source, GunshipFirstCharacter + (liftoff.Asset - VramAssetId.GunshipLiftoffFirstTiles) * Characters(GunshipLiftoffTransferDefinitions.ByteCount),
                Characters(GunshipLiftoffTransferDefinitions.ByteCount)),
        AssetObjCharacters { Asset: VramAssetId.EscapeTimerFirstTiles } => Timer(source, EscapeTimerTilePage.First),
        AssetObjCharacters { Asset: VramAssetId.EscapeTimerSecondTiles } => Timer(source, EscapeTimerTilePage.Second),
        TorizoArtworkSwapCharacters swap =>
            new(source, TorizoFirstCharacters[swap.Entry], Characters(TorizoArtworkSwapDefinitions.Entries[swap.Entry].ByteCount)),
        CrocomireSkeletonChunkCharacters chunk => new(source, SkeletonFirstCharacter(chunk.Entry),
            Characters(SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireSkeletonTransferDefinitions.ChunkByteCount)),
        EnemyTileCharacters { Enemy: EnemyDefinitionId.Evir } => new(source, EvirFirstCharacter,
            Characters(SuperMetroid.Core.Bosses.Draygon.DraygonIntroPresentationDefinitions.EvirTilesByteCount)),
        CeresDoorCharacters => new(source, CeresDoorFirstCharacter, Characters(SuperMetroid.Core.Areas.Ceres.CeresDoorVisualRomData.TileByteCount)),
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
    private static SpriteCharacterPlacement Timer(ObjCharacterSource source, EscapeTimerTilePage page) =>
        new(source, EscapeTimerTilePages.FirstCharacterOf(page), Characters(EscapeTimerTilePages.ByteCountOf(page)));

    /// <summary>The whole sprite characters <paramref name="byteCount"/> bytes of the cartridge's copy hold.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters.</exception>
    private static int Characters(int byteCount) => byteCount % SpriteCharacters.CharacterByteCount == 0
        ? byteCount / SpriteCharacters.CharacterByteCount
        : throw new InvalidDataException($"{byteCount} bytes are not whole sprite characters.");

    /// <summary>The character skeleton chunk <paramref name="chunk"/> begins at: rows of the first region, then of the second.</summary>
    private static int SkeletonFirstCharacter(int chunk) => chunk < SkeletonFirstRegionRows
        ? SkeletonFirstRegionCharacter + chunk * SkeletonCharactersPerRow
        : SkeletonSecondRegionCharacter + (chunk - SkeletonFirstRegionRows) * SkeletonCharactersPerRow;
}
