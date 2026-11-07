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

    /// <summary>What each step of <paramref name="sequence"/> copies, resolved against <paramref name="installed"/>.</summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks a step's characters.</exception>
    internal static IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> Steps(EncounterArtworkSequence sequence, IInstalledArtworkContent installed) =>
        [.. Enumerable.Range(0, StepCount(sequence)).Select(step => Step(sequence, step, installed))];

    /// <summary>The steps of <paramref name="sequence"/>.</summary>
    /// <exception cref="InvalidDataException">The sequence is not one the game plays.</exception>
    internal static int StepCount(EncounterArtworkSequence sequence) => sequence switch
    {
        EncounterArtworkSequence.DraygonEvirs or EncounterArtworkSequence.CeresDoor => 1,
        EncounterArtworkSequence.GunshipLiftoff => GunshipLiftoffTransferDefinitions.Frames.Length,
        EncounterArtworkSequence.CrocomireSkeleton => CrocomireSkeletonTransferDefinitions.FrameCount,
        EncounterArtworkSequence.BombTorizo => TorizoArtworkSwapDefinitions.BombSequence.Count,
        EncounterArtworkSequence.GoldenTorizo => TorizoArtworkSwapDefinitions.GoldenSequence.Count,
        EncounterArtworkSequence.CeresEscapePages => CeresEscapePageDefinitions.ObjectPageCount,
        EncounterArtworkSequence.MotherBrainPages => MotherBrainSpritePageDefinitions.Sequence.Count,
        _ => throw new InvalidDataException($"Undefined {nameof(EncounterArtworkSequence)} {sequence}."),
    };

    /// <summary>What step <paramref name="step"/> of <paramref name="sequence"/> copies, resolved against <paramref name="installed"/>.</summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks the step's characters, or the sequence is the corpse rot.</exception>
    internal static IReadOnlyList<EncounterArtworkUpload> Step(EncounterArtworkSequence sequence, int step, IInstalledArtworkContent installed)
    {
        Ensure.NotNull(installed);
        Ensure.BetweenInclusive(0, StepCount(sequence) - 1, step);
        return sequence switch
        {
            EncounterArtworkSequence.DraygonEvirs => [new(EncounterCharacterPlacementDefinitions.EvirFirstCharacter,
                Whole(Sprites(installed).EnemyTiles.TilesOf(EnemyDefinitionId.Evir), EncounterCharacterPlacementDefinitions.EvirByteCount, "the Evir tiles"))],
            EncounterArtworkSequence.GunshipLiftoff => [new(EncounterCharacterPlacementDefinitions.GunshipFirstCharacter
                    + step * (GunshipLiftoffCharacterDefinitions.ByteCount / SpriteCharacterTableLayout.CharacterByteCount),
                Whole(installed.Resolve(GunshipLiftoffTransferDefinitions.Frames[step].Asset), GunshipLiftoffCharacterDefinitions.ByteCount,
                    $"{GunshipLiftoffTransferDefinitions.Frames[step].Asset}"))],
            EncounterArtworkSequence.CrocomireSkeleton => [new(SkeletonFirstCharacter(step),
                Whole((Bosses(installed).CrocomireSkeleton ?? throw new InvalidDataException(
                    "Installed Crocomire artwork is missing its skeleton characters.")).Chunk(step), CrocomireSkeletonCharacterDefinitions.ChunkByteCount,
                    $"Crocomire skeleton chunk {step}"))],
            EncounterArtworkSequence.BombTorizo => [Torizo(TorizoArtworkSwapDefinitions.BombSequence[step], installed)],
            EncounterArtworkSequence.GoldenTorizo => [Torizo(TorizoArtworkSwapDefinitions.GoldenSequence[step], installed)],
            EncounterArtworkSequence.CeresEscapePages => [step < EscapeTextArtworkDefinitions.TimerPageCount
                ? Timer(Enum.GetValues<EscapeTimerTilePage>()[step], installed)
                : Warning(step - EscapeTextArtworkDefinitions.TimerPageCount, installed)],
            EncounterArtworkSequence.CeresDoor => [new(EncounterCharacterPlacementDefinitions.CeresDoorFirstCharacter,
                Whole((Areas(installed).CeresDoor ?? throw new InvalidDataException("Installed area artwork has no Ceres door visuals.")).Characters,
                    CeresDoorVisualFormat.CharacterBytes, "the Ceres door characters"))],
            EncounterArtworkSequence.MotherBrainPages => [MotherBrainPage(MotherBrainSpritePageDefinitions.Sequence[step], installed)],
            _ => throw new InvalidDataException($"Undefined {nameof(EncounterArtworkSequence)} {sequence}."),
        };
    }

    /// <summary>Torizo artwork swap <paramref name="swap"/>, where the cartridge copies it.</summary>
    private static EncounterArtworkUpload Torizo(TorizoArtworkSwap swap, IInstalledArtworkContent installed)
    {
        TorizoSwapArtwork artwork = Bosses(installed).TorizoSwaps ?? throw new InvalidDataException("Torizo artwork swaps require installed artwork.");
        TorizoArtworkSwapDefinition definition = TorizoArtworkSwapPageDefinitions.Entries[swap.Ordinal];
        return new EncounterArtworkUpload(EncounterCharacterPlacementDefinitions.TorizoDestinations[swap.Ordinal].FirstCharacter,
            Whole(artwork.Characters(definition.Page, definition.PageByte, definition.ByteCount), definition.ByteCount, $"Torizo artwork swap {swap.Ordinal}"));
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
                MotherBrainSpecialSpriteSheetDefinition sheet = MotherBrainSpecialSpriteSheetDefinitions.All.Single(definition => definition.Sheet == page.Sheet);
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
