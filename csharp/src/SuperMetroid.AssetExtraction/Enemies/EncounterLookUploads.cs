using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Escape.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The authored looks each encounter shows as it plays out, and what each copies over the cartridge's sprite characters
/// (#1392, #1406, #1463, #1538), which only the import knows. A look shows every copy of the looks before it and its own,
/// in order, over the characters its room's load leaves: the import composes each frame's finished picture of a look from
/// them, and gameplay names only the look.
/// </summary>
internal static class EncounterLookUploads
{
    /// <summary>The encounter each enemy whose graphics set loads it plays.</summary>
    private static readonly Dictionary<EnemyDefinitionId, ComposedEncounter> ByEncounter = new()
    {
        [EnemyDefinitionId.DraygonBody] = ComposedEncounter.DraygonEvirs,
        [EnemyDefinitionId.ShipTop] = ComposedEncounter.Gunship,
        [EnemyDefinitionId.Crocomire] = ComposedEncounter.Crocomire,
        [EnemyDefinitionId.BombTorizo] = ComposedEncounter.BombTorizo,
        [EnemyDefinitionId.GoldenTorizo] = ComposedEncounter.GoldenTorizo,
        [EnemyDefinitionId.RidleyCeres] = ComposedEncounter.CeresEscape,
        [EnemyDefinitionId.CeresDoor] = ComposedEncounter.CeresDoor,
        [EnemyDefinitionId.MotherBrainHead] = ComposedEncounter.MotherBrain,
    };

    /// <summary>The name of Draygon's look once her opening has readied the Evirs she releases.</summary>
    internal const string DraygonEvirsLook = "Evirs";

    /// <summary>The name of Mother Brain's look once her rise has copied her legs.</summary>
    internal const string MotherBrainLegsLook = "Legs";

    /// <summary>The name of Mother Brain's look once her rise has copied her attacks back over her legs.</summary>
    internal const string MotherBrainAttackLook = "Attack";

    /// <summary>The name of Mother Brain's look once the finish-off has copied the Baby Metroid.</summary>
    internal const string MotherBrainBabyLook = "Baby";

    /// <summary>The name of Mother Brain's look once the Baby's cutscene has copied her attacks again.</summary>
    internal const string MotherBrainAttackAgainLook = "AttackAgain";

    /// <summary>The name of Mother Brain's look once her death has copied her corpse.</summary>
    internal const string MotherBrainCorpseLook = "Corpse";

    /// <summary>The name of Mother Brain's look once her death has copied the escape timer.</summary>
    internal const string MotherBrainEscapeTimerLook = "EscapeTimer";

    /// <summary>The name of Mother Brain's look once her death has copied the escape message.</summary>
    internal const string MotherBrainEscapeTextLook = "EscapeText";

    /// <summary>The name of Mother Brain's look once her death has copied the exploded escape door.</summary>
    internal const string MotherBrainExplodedDoorLook = "ExplodedDoor";

    /// <summary>
    /// Mother Brain's looks after her room's load, in the order she shows them, each with the sheet whose pages it copies:
    /// her legs and the attack pages they covered as she rises, the Baby, her attack pages again as the Baby's cutscene
    /// ends, her corpse, the escape timer and message, and the exploded escape door.
    /// </summary>
    private static readonly MotherBrainLook[] MotherBrainLooks =
    [
        new(MotherBrainLegsLook, MotherBrainSpriteSheet.Legs),
        new(MotherBrainAttackLook, MotherBrainSpriteSheet.Attack),
        new(MotherBrainBabyLook, MotherBrainSpriteSheet.BabyMetroid),
        new(MotherBrainAttackAgainLook, MotherBrainSpriteSheet.Attack),
        new(MotherBrainCorpseLook, MotherBrainSpriteSheet.Corpse),
        new(MotherBrainEscapeTimerLook, MotherBrainSpriteSheet.EscapeTimer),
        new(MotherBrainEscapeTextLook, MotherBrainSpriteSheet.EscapeText),
        new(MotherBrainExplodedDoorLook, MotherBrainSpriteSheet.ExplodedDoor),
    ];

    /// <summary>The encounters the members of graphics set <paramref name="set"/> play, in encounter order.</summary>
    internal static IReadOnlyList<ComposedEncounter> Of(RoomEnemyGraphicsSetMembers set) =>
        [.. Ensure.NotNull(set).Records.ToArray().Select(record => record.DefinitionId).OfType<EnemyDefinitionId>().Where(ByEncounter.ContainsKey)
            .Select(enemy => ByEncounter[enemy]).Distinct().Order()];

    /// <summary>
    /// Whether a room of <paramref name="state"/> shows <paramref name="encounter"/>'s looks after its load (#1648): the state
    /// places an enemy that plays the encounter and copies its looks. Every such enemy does but a Ceres door other than the
    /// elevator shaft's pre-explosion overlay door, whose initialization alone copies the doors' own look ($A6:F739); a
    /// room whose state shows no look of an encounter keeps its load there.
    /// </summary>
    internal static bool ShownIn(ComposedEncounter encounter, RoomStateSettings state)
    {
        foreach (RoomEnemyPlacement record in Ensure.NotNull(state).EnemyPopulation.Placements.Span)
        {
            if (ByEncounter.TryGetValue(record.DefinitionId, out ComposedEncounter played) && played == encounter && CopiesLooks(record))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Whether <paramref name="placement"/> copies its encounter's looks: every placement but a Ceres door other than the
    /// pre-explosion overlay door (enemy parameter 2, whose initialization alone runs $A6:F739's copy).
    /// </summary>
    private static bool CopiesLooks(RoomEnemyPlacement placement) =>
        placement.DefinitionId != EnemyDefinitionId.CeresDoor
        || placement.SpawnSettingsOf<CeresDoorSpawnSettings>().Variant == CeresDoorVariant.RotatingElevatorPreExplosionOverlay;

    /// <summary>
    /// The encounter gameplay shows <paramref name="encounter"/>'s looks as, or none for an encounter whose looks gameplay
    /// never shows: each of its frames is drawn in one look only, which is its one finished picture.
    /// </summary>
    /// <exception cref="InvalidDataException">The encounter is not one the import composes.</exception>
    internal static EncounterArtwork? ShownAs(ComposedEncounter encounter) => encounter switch
    {
        ComposedEncounter.DraygonEvirs or ComposedEncounter.MotherBrain => null,
        ComposedEncounter.Gunship => EncounterArtwork.Gunship,
        ComposedEncounter.Crocomire => EncounterArtwork.Crocomire,
        ComposedEncounter.BombTorizo => EncounterArtwork.BombTorizo,
        ComposedEncounter.GoldenTorizo => EncounterArtwork.GoldenTorizo,
        ComposedEncounter.CeresEscape => EncounterArtwork.CeresEscape,
        ComposedEncounter.CeresDoor => EncounterArtwork.CeresDoor,
        _ => throw new InvalidDataException($"Undefined {nameof(ComposedEncounter)} {encounter}."),
    };

    /// <summary>The names of <paramref name="encounter"/>'s looks in the order it shows them, its room's load first.</summary>
    /// <exception cref="InvalidDataException">The encounter is not one the import composes.</exception>
    internal static IReadOnlyList<string> Looks(ComposedEncounter encounter) => encounter switch
    {
        ComposedEncounter.DraygonEvirs => [EncounterLook.LoadedName, DraygonEvirsLook],
        ComposedEncounter.Gunship => [EncounterLook.LoadedName, GunshipTakeoffDefinitions.TakeoffLook.Name],
        ComposedEncounter.Crocomire => [EncounterLook.LoadedName, CrocomireSkeletonLookDefinitions.SkeletonLook.Name],
        ComposedEncounter.BombTorizo => [EncounterLook.LoadedName,
            .. TorizoArtworkSwapDefinitions.BombSequence.Select(swap => swap.Look(EncounterArtwork.BombTorizo).Name)],
        ComposedEncounter.GoldenTorizo => [EncounterLook.LoadedName,
            .. TorizoArtworkSwapDefinitions.GoldenSequence.Select(swap => swap.Look(EncounterArtwork.GoldenTorizo).Name)],
        ComposedEncounter.CeresEscape => [EncounterLook.LoadedName, CeresEscapeReadyingDefinitions.WarningLook.Name],
        ComposedEncounter.CeresDoor => [EncounterLook.LoadedName, CeresDoorLookDefinitions.OverlayLook.Name],
        ComposedEncounter.MotherBrain => [EncounterLook.LoadedName, .. MotherBrainLooks.Select(look => look.Name)],
        _ => throw new InvalidDataException($"Undefined {nameof(ComposedEncounter)} {encounter}."),
    };

    /// <summary>The place among <paramref name="encounter"/>'s looks of the one named <paramref name="look"/>.</summary>
    /// <exception cref="InvalidDataException">The encounter has no such look.</exception>
    internal static int IndexOf(ComposedEncounter encounter, string look)
    {
        int index = Looks(encounter).ToList().IndexOf(look);
        return index >= 0 ? index : throw new InvalidDataException($"{encounter} has no look {look}.");
    }

    /// <summary>What each of <paramref name="encounter"/>'s looks copies, resolved against <paramref name="installed"/>.</summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks a look's characters.</exception>
    internal static EncounterLookSteps Steps(ComposedEncounter encounter, IInstalledArtworkContent installed) =>
        new(encounter, [.. Enumerable.Range(0, Looks(encounter).Count).Select(look => Uploads(encounter, look, installed))]);

    /// <summary>
    /// What look <paramref name="look"/> of <paramref name="encounter"/> copies, in the order its copies land, resolved
    /// against <paramref name="installed"/>: nothing for the room's load.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks the look's characters, or the encounter has no such look.</exception>
    internal static IReadOnlyList<EncounterArtworkUpload> Uploads(ComposedEncounter encounter, int look, IInstalledArtworkContent installed)
    {
        Ensure.NotNull(installed);
        Ensure.BetweenInclusive(0, Looks(encounter).Count - 1, look);
        if (look == 0)
            return [];
        return encounter switch
        {
            ComposedEncounter.DraygonEvirs => [new(EncounterCharacterPlacementDefinitions.EvirFirstCharacter,
                Whole(Sprites(installed).EnemyTiles.TilesOf(EnemyDefinitionId.Evir), EncounterCharacterPlacementDefinitions.EvirByteCount, "the Evir tiles"))],
            ComposedEncounter.Gunship => [.. Enumerable.Range(0, GunshipLiftoffTransferDefinitions.Frames.Length).Select(page =>
                new EncounterArtworkUpload(EncounterCharacterPlacementDefinitions.GunshipFirstCharacter
                        + page * (GunshipLiftoffCharacterDefinitions.ByteCount / SpriteCharacterTableLayout.CharacterByteCount),
                    Whole(installed.Resolve(GunshipLiftoffTransferDefinitions.Frames[page].Asset), GunshipLiftoffCharacterDefinitions.ByteCount,
                        $"{GunshipLiftoffTransferDefinitions.Frames[page].Asset}")))],
            ComposedEncounter.Crocomire => [.. Enumerable.Range(0, CrocomireSkeletonCharacterDefinitions.ChunkCount).Select(chunk =>
                new EncounterArtworkUpload(SkeletonFirstCharacter(chunk),
                    Whole((Bosses(installed).CrocomireSkeleton ?? throw new InvalidDataException(
                        "Installed Crocomire artwork is missing its skeleton characters.")).Chunk(chunk), CrocomireSkeletonCharacterDefinitions.ChunkByteCount,
                        $"Crocomire skeleton chunk {chunk}")))],
            ComposedEncounter.BombTorizo => [Torizo(TorizoArtworkSwapDefinitions.BombSequence[look - 1], installed)],
            ComposedEncounter.GoldenTorizo => [Torizo(TorizoArtworkSwapDefinitions.GoldenSequence[look - 1], installed)],
            ComposedEncounter.CeresEscape => [.. Enum.GetValues<EscapeTimerTilePage>().Select(page => Timer(page, installed)),
                .. Enumerable.Range(0, CeresWarningTextPageDefinitions.PageCount).Select(page => Warning(page, installed))],
            ComposedEncounter.CeresDoor => [new(EncounterCharacterPlacementDefinitions.CeresDoorFirstCharacter,
                Whole((Areas(installed).CeresDoor ?? throw new InvalidDataException("Installed area artwork has no Ceres door visuals.")).Characters,
                    CeresDoorVisualFormat.CharacterBytes, "the Ceres door characters"))],
            ComposedEncounter.MotherBrain => MotherBrainPages(MotherBrainLooks[look - 1].Sheet, installed),
            _ => throw new InvalidDataException($"Undefined {nameof(ComposedEncounter)} {encounter}."),
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

    /// <summary>Every page of Mother Brain's <paramref name="sheet"/>, in order, each where the cartridge copies it.</summary>
    /// <exception cref="InvalidDataException">The installed artwork lacks the sheet.</exception>
    private static IReadOnlyList<EncounterArtworkUpload> MotherBrainPages(MotherBrainSpriteSheet sheet, IInstalledArtworkContent installed)
    {
        switch (sheet)
        {
            case MotherBrainSpriteSheet.Corpse:
                RoomCharacterAtlas corpse = Bosses(installed).MotherBrainCorpse ?? throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-corpse-tiles.png.");
                return [.. Enumerable.Range(0, MotherBrainCorpseArtworkDefinitions.RowCount).Select(page => new EncounterArtworkUpload(
                    MotherBrainCorpseSheet.PageFirstCharacter(page), corpse.Characters.Slice(MotherBrainCorpseSheet.PageOffset(page), MotherBrainCorpseSheet.PageByteCount)))];
            case MotherBrainSpriteSheet.EscapeTimer:
                return [.. Enum.GetValues<EscapeTimerTilePage>().Select(page => Timer(page, installed))];
            case MotherBrainSpriteSheet.EscapeText:
                RoomCharacterAtlas text = Bosses(installed).MotherBrainEscapeText ?? throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.");
                return [.. Enumerable.Range(0, EscapeTextSheetFormat.PageCount).Select(page => new EncounterArtworkUpload(
                    EscapeTextPages.PageFirstCharacter(page), text.Characters.Slice(EscapeTextPages.PageOffset(page), EscapeTextPages.PageByteCount(page))))];
            case MotherBrainSpriteSheet.Legs:
            case MotherBrainSpriteSheet.BabyMetroid:
            case MotherBrainSpriteSheet.Attack:
            case MotherBrainSpriteSheet.ExplodedDoor:
                MotherBrainSpecialSpriteSheetDefinition definition = MotherBrainSpecialSpriteSheetDefinitions.All.Single(special => special.Sheet == sheet);
                MotherBrainSpecialSpriteArtworkCatalog artwork = Bosses(installed).MotherBrainSpecialSprites ??
                    throw new InvalidDataException($"Installed enemy artwork is missing {definition.FileName}.");
                return [.. Enumerable.Range(0, definition.PageCount).Select(page => new EncounterArtworkUpload(
                    MotherBrainSpecialSprites.PageFirstCharacter(definition, page), artwork.Get(definition.Sheet).Characters.Slice(
                        page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount, MotherBrainSpecialSpriteSheetDefinition.PageByteCount)))];
            default:
                throw new InvalidDataException($"Undefined {nameof(MotherBrainSpriteSheet)} {sheet}.");
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

    /// <summary>One of Mother Brain's looks after her room's load.</summary>
    /// <param name="Name">The look's name.</param>
    /// <param name="Sheet">The sheet whose pages the look copies.</param>
    private readonly record struct MotherBrainLook(string Name, MotherBrainSpriteSheet Sheet);

    /// <summary>The installed area artwork <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed area artwork.</exception>
    private static AreaSourceArtwork Areas(IInstalledArtworkContent installed) =>
        (installed as IInstalledAreaSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed area artwork to compose an encounter's artwork from.")).AreaTiles;
}
