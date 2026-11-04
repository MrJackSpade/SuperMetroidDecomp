using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Escape.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// What each step of an encounter's artwork sequence copies over the cartridge's sprite characters (#1392, #1406, #1463,
/// #1513), which only the import knows: the encounter names the step it has reached, and the import composes each frame's
/// picture of that step by copying every step up to it, in order, over the characters its room's load leaves.
/// </summary>
internal static class EncounterArtworkUploads
{
    /// <summary>The artwork sequence each encounter plays, by the enemy whose graphics set loads it.</summary>
    private static readonly Dictionary<EnemyDefinitionId, EncounterArtworkSequence> ByEncounter = new()
    {
        [EnemyDefinitionId.DraygonBody] = EncounterArtworkSequence.DraygonEvirs,
        [EnemyDefinitionId.ShipTop] = EncounterArtworkSequence.GunshipLiftoff,
        [EnemyDefinitionId.Crocomire] = EncounterArtworkSequence.CrocomireSkeleton,
        [EnemyDefinitionId.BombTorizo] = EncounterArtworkSequence.BombTorizo,
        [EnemyDefinitionId.GoldenTorizo] = EncounterArtworkSequence.GoldenTorizo,
        [EnemyDefinitionId.RidleyCeres] = EncounterArtworkSequence.CeresEscapePages,
        [EnemyDefinitionId.CeresDoor] = EncounterArtworkSequence.CeresDoor,
        [EnemyDefinitionId.MotherBrainHead] = EncounterArtworkSequence.MotherBrainPages,
    };

    /// <summary>The artwork sequences the encounters of graphics set <paramref name="set"/>'s members play, in sequence order.</summary>
    internal static IReadOnlyList<EncounterArtworkSequence> Of(RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.ToArray().Select(record => record.DefinitionId).Where(ByEncounter.ContainsKey)
            .Select(enemy => ByEncounter[enemy]).Distinct().Order()];

    /// <summary>
    /// What each step of <paramref name="sequence"/> copies, resolved against <paramref name="installed"/>. Mother Brain's
    /// corpse rot is its corpse's simulation's, not installed artwork's.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks a step's characters, or the sequence is the corpse rot.</exception>
    internal static IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> Steps(EncounterArtworkSequence sequence, IInstalledArtworkContent installed)
    {
        Ensure.NotNull(installed);
        return sequence switch
        {
            EncounterArtworkSequence.DraygonEvirs => [[new(EncounterCharacterPlacementDefinitions.EvirFirstCharacter,
                Whole(Sprites(installed).EnemyTiles.TilesOf(EnemyDefinitionId.Evir), EncounterCharacterPlacementDefinitions.EvirByteCount, "the Evir tiles"))]],
            EncounterArtworkSequence.GunshipLiftoff => [.. GunshipLiftoffTransferDefinitions.Frames.Select((page, index) => (IReadOnlyList<EncounterArtworkUpload>)[
                new EncounterArtworkUpload(EncounterCharacterPlacementDefinitions.GunshipFirstCharacter
                    + index * (GunshipLiftoffCharacterDefinitions.ByteCount / SpriteCharacterTableLayout.CharacterByteCount),
                    Whole(installed.Resolve(page.Asset), GunshipLiftoffCharacterDefinitions.ByteCount, $"{page.Asset}"))])],
            EncounterArtworkSequence.CrocomireSkeleton => [.. Enumerable.Range(0, CrocomireSkeletonTransferDefinitions.FrameCount)
                .Select(chunk => (IReadOnlyList<EncounterArtworkUpload>)[new EncounterArtworkUpload(SkeletonFirstCharacter(chunk),
                    Whole((Bosses(installed).CrocomireSkeleton ?? throw new InvalidDataException(
                        "Installed Crocomire artwork is missing its skeleton characters.")).Chunk(chunk), CrocomireSkeletonCharacterDefinitions.ChunkByteCount,
                        $"Crocomire skeleton chunk {chunk}"))])],
            EncounterArtworkSequence.BombTorizo => Torizo(TorizoArtworkSwapDefinitions.BombSequence, installed),
            EncounterArtworkSequence.GoldenTorizo => Torizo(TorizoArtworkSwapDefinitions.GoldenSequence, installed),
            EncounterArtworkSequence.CeresEscapePages => [
                .. Enum.GetValues<EscapeTimerTilePage>().Select(page => (IReadOnlyList<EncounterArtworkUpload>)[Timer(page, installed)]),
                .. Enumerable.Range(0, CeresEscapePageDefinitions.WarningPageCount).Select(page => (IReadOnlyList<EncounterArtworkUpload>)[Warning(page, installed)]),
            ],
            EncounterArtworkSequence.CeresDoor => [[new(EncounterCharacterPlacementDefinitions.CeresDoorFirstCharacter,
                Whole((Areas(installed).CeresDoor ?? throw new InvalidDataException("Installed area artwork has no Ceres door visuals.")).Characters,
                    CeresDoorVisualFormat.CharacterBytes, "the Ceres door characters"))]],
            EncounterArtworkSequence.MotherBrainPages => [.. MotherBrainSpritePageDefinitions.Sequence
                .Select(page => (IReadOnlyList<EncounterArtworkUpload>)[MotherBrainPage(page, installed)])],
            EncounterArtworkSequence.MotherBrainCorpseRot => throw new InvalidDataException(
                "Mother Brain's corpse rot is pictured from her corpse's simulation, not from installed artwork."),
            _ => throw new InvalidDataException($"Undefined {nameof(EncounterArtworkSequence)} {sequence}."),
        };
    }

    /// <summary>The steps of a Torizo whose programs show <paramref name="swaps"/> in order.</summary>
    private static IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> Torizo(IReadOnlyList<TorizoArtworkSwap> swaps, IInstalledArtworkContent installed)
    {
        TorizoSwapArtwork artwork = Bosses(installed).TorizoSwaps ?? throw new InvalidDataException("Torizo artwork swaps require installed artwork.");
        return [.. swaps.Select(swap =>
        {
            TorizoArtworkSwapDefinition definition = TorizoArtworkSwapPageDefinitions.Entries[swap.Ordinal];
            return (IReadOnlyList<EncounterArtworkUpload>)[new EncounterArtworkUpload(
                EncounterCharacterPlacementDefinitions.TorizoDestinations[swap.Ordinal].FirstCharacter,
                Whole(artwork.Characters(definition.Page, definition.PageByte, definition.ByteCount), definition.ByteCount, $"Torizo artwork swap {swap.Ordinal}"))];
        })];
    }

    /// <summary>Escape timer page <paramref name="page"/>, where the cartridge copies it.</summary>
    private static EncounterArtworkUpload Timer(EscapeTimerTilePage page, IInstalledArtworkContent installed) =>
        new(EscapeTimerTilePageTransfers.FirstCharacterOf(page),
            Whole(installed.Resolve(EscapeTimerTilePages.AssetOf(page)), EscapeTimerTilePageTransfers.ByteCountOf(page), $"escape timer page {page}"));

    /// <summary>Ceres warning page <paramref name="page"/>, where the cartridge copies it.</summary>
    private static EncounterArtworkUpload Warning(int page, IInstalledArtworkContent installed) =>
        new(CeresWarningGlyphDefinitions.WarningFirstCharacter + CeresWarningTextPages.ByteOffset(page) / SpriteCharacterTableLayout.CharacterByteCount,
            (Areas(installed).CeresEscapeTiles ?? throw new InvalidDataException("Installed Ceres escape has no editable character pages."))
                .TryResolve(CeresWarningTextPageDefinitions.InstalledPage, CeresWarningTextPages.ByteOffset(page), CeresWarningTextPages.ByteCount(page),
                    out ReadOnlyMemory<byte> characters)
                ? characters
                : throw new InvalidDataException($"Installed Ceres escape warning text has no characters for warning page {page}."));

    /// <summary>Mother Brain's page <paramref name="page"/>, where the cartridge copies it.</summary>
    private static EncounterArtworkUpload MotherBrainPage(MotherBrainSpritePage page, IInstalledArtworkContent installed)
    {
        switch (page.Sheet)
        {
            case MotherBrainSpritePageSheet.Corpse:
                return new(MotherBrainCorpseSheet.PageFirstCharacter(page.Page), (Bosses(installed).MotherBrainCorpse ?? throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-corpse-tiles.png.")).Characters.Slice(
                    MotherBrainCorpseSheet.PageOffset(page.Page), MotherBrainCorpseSheet.PageByteCount));
            case MotherBrainSpritePageSheet.EscapeTimer:
                return Timer(Enum.GetValues<EscapeTimerTilePage>()[page.Page], installed);
            case MotherBrainSpritePageSheet.EscapeText:
                return new(EscapeTextPages.PageFirstCharacter(page.Page), (Bosses(installed).MotherBrainEscapeText ?? throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.")).Characters.Slice(
                    EscapeTextPages.PageOffset(page.Page), EscapeTextPages.PageByteCount(page.Page)));
            case MotherBrainSpritePageSheet.Legs:
            case MotherBrainSpritePageSheet.BabyMetroid:
            case MotherBrainSpritePageSheet.Attack:
            case MotherBrainSpritePageSheet.ExplodedDoor:
                MotherBrainSpecialSpriteSheetDefinition sheet = MotherBrainSpecialSpriteArtworkDefinitions.All.Single(definition => definition.Sheet == page.Sheet);
                MotherBrainSpecialSpriteArtworkCatalog artwork = Bosses(installed).MotherBrainSpecialSprites ??
                    throw new InvalidDataException($"Installed enemy artwork is missing {sheet.FileName}.");
                return new(MotherBrainSpecialSprites.PageFirstCharacter(sheet, page.Page), artwork.Get(sheet.Sheet).Characters.Slice(
                    page.Page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount, MotherBrainSpecialSpriteSheetDefinition.PageByteCount));
            default:
                throw new InvalidDataException($"Undefined {nameof(MotherBrainSpritePageSheet)} {page.Sheet}.");
        }
    }

    /// <summary>The first <paramref name="byteCount"/> bytes of <paramref name="characters"/>, which the cartridge copies.</summary>
    /// <exception cref="InvalidDataException">The installed characters are fewer.</exception>
    private static ReadOnlyMemory<byte> Whole(ReadOnlyMemory<byte> characters, int byteCount, string what) =>
        characters.Length >= byteCount ? characters[..byteCount]
            : throw new InvalidDataException($"Installed {what} hold {characters.Length} bytes; the cartridge copies {byteCount}.");

    /// <summary>The character skeleton chunk <paramref name="chunk"/> begins at: rows of the first region, then of the second.</summary>
    private static int SkeletonFirstCharacter(int chunk) => chunk < EncounterCharacterPlacementDefinitions.SkeletonFirstRegionRows
        ? EncounterCharacterPlacementDefinitions.SkeletonFirstRegionCharacter + chunk * EncounterCharacterPlacementDefinitions.SkeletonCharactersPerRow
        : EncounterCharacterPlacementDefinitions.SkeletonSecondRegionCharacter + (chunk - EncounterCharacterPlacementDefinitions.SkeletonFirstRegionRows) * EncounterCharacterPlacementDefinitions.SkeletonCharactersPerRow;

    /// <summary>The installed enemy tiles <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed gameplay artwork.</exception>
    private static IInstalledSpriteCharacterArtwork Sprites(IInstalledArtworkContent installed) =>
        installed as IInstalledSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed gameplay artwork to compose an encounter's artwork from.");

    /// <summary>The installed boss artwork <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed boss artwork.</exception>
    private static BossSourceArtwork Bosses(IInstalledArtworkContent installed) =>
        (installed as IInstalledBossSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed boss artwork to compose an encounter's artwork from.")).BossTiles;

    /// <summary>The installed area artwork <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed area artwork.</exception>
    private static AreaSourceArtwork Areas(IInstalledArtworkContent installed) =>
        (installed as IInstalledAreaSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed area artwork to compose an encounter's artwork from.")).AreaTiles;
}
