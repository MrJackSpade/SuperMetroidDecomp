using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Pre-assembles and pre-colours every boss body frame (#1466): each frame a boss draws on its body layer (Kraid's body
/// pages and head, Phantoon's, Draygon's, Crocomire's and Mother Brain's poses and parts, Crocomire's melt images) as an
/// image painted over the characters the boss's room shows, with the cells it covers. A frame whose cells show an
/// animated piece of artwork is split: its still cells form one part, and the cells of each animation another, pictured
/// again over every frame of that animation. The tilemap words the cartridge writes are read here, once, and nowhere
/// else.
/// </summary>
internal static class BossBodySheetComposer
{
    /// <summary>Part pictures laid out across the sheet before a column grows downward.</summary>
    private const int SheetColumns = 8;

    /// <summary>The enemy whose rooms each boss's body is drawn in.</summary>
    private static readonly Dictionary<BossBodyOwner, EnemyDefinitionId> Enemies = new()
    {
        [BossBodyOwner.Kraid] = EnemyDefinitionId.Kraid,
        [BossBodyOwner.Phantoon] = EnemyDefinitionId.PhantoonBody,
        [BossBodyOwner.Draygon] = EnemyDefinitionId.DraygonBody,
        [BossBodyOwner.Crocomire] = EnemyDefinitionId.Crocomire,
        [BossBodyOwner.MotherBrain] = EnemyDefinitionId.MotherBrainBody,
    };

    /// <summary>Composes every boss's body sheet from the installed stock artwork.</summary>
    /// <exception cref="InvalidDataException">
    /// A boss's artwork is missing, no room shows the boss, or a frame reads a character its room rebinds after loading.
    /// </exception>
    internal static IReadOnlyList<ComposedBossBodySheet> Compose(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        return [.. Enum.GetValues<BossBodyOwner>().Select(owner => Compose(inputs, owner))];
    }

    /// <summary>
    /// Composes <paramref name="owner"/>'s sheet over the first room state, in room order, that shows the boss: the
    /// characters every frame reads there, as the tile word sheets were composed before (#1392).
    /// </summary>
    private static ComposedBossBodySheet Compose(RoomBlockSheetInputs inputs, BossBodyOwner owner)
    {
        EnemyDefinitionId enemy = Enemies[owner];
        BossRoomCharacters? characters = null;
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            RoomStateSettings? state = RoomStateSelection.GetStates(room)
                .FirstOrDefault(candidate => candidate.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == enemy));
            if (state is not null)
            {
                characters = RoomBlockUploads.ShownUnderBoss(inputs, room, state);
                break;
            }
        }
        if (characters is null)
            throw new InvalidDataException($"No room state shows {owner}.");
        var painter = new BossBodyPainter(owner, characters, RoomStaticPaletteImage.Colours(inputs.Palettes.Get(characters.Tileset.Palette)));
        foreach (BossBodyFrameSource frame in Frames(inputs.Bosses, owner))
            painter.Add(frame);
        return painter.Compose(BlankOf(owner), SheetColumns);
    }

    /// <summary>The installed sheet name of <paramref name="owner"/>'s body sheet.</summary>
    internal static string SheetNameOf(BossBodyOwner owner) => $"BossBody.{owner}";

    /// <summary>The tile <paramref name="owner"/> clears its body layer with.</summary>
    internal static BackgroundTile BlankOf(BossBodyOwner owner) => owner switch
    {
        BossBodyOwner.Kraid => KraidBackgroundDefinitions.BlankTile,
        BossBodyOwner.Phantoon or BossBodyOwner.Draygon or BossBodyOwner.Crocomire or BossBodyOwner.MotherBrain => BossBodySourceDefinitions.BlankTile,
        _ => throw new InvalidDataException($"Undefined boss body owner {owner}."),
    };

    /// <summary>Every frame <paramref name="owner"/> draws, at the layer cells it writes.</summary>
    /// <exception cref="InvalidDataException">The boss's installed artwork is missing.</exception>
    internal static IEnumerable<BossBodyFrameSource> Frames(BossArtworkCatalog bosses, BossBodyOwner owner)
    {
        switch (owner)
        {
            case BossBodyOwner.Kraid:
                return KraidFrames(bosses.KraidBackground ?? throw Missing(owner));
            case BossBodyOwner.Phantoon:
                return EnemyFrames((bosses.PhantoonBg2Frames ?? throw Missing(owner)).Writes);
            case BossBodyOwner.Draygon:
                return EnemyFrames((bosses.DraygonBg2Frames ?? throw Missing(owner)).Writes);
            case BossBodyOwner.Crocomire:
                CrocomireMeltingArtwork melting = bosses.CrocomireMelting ?? throw Missing(owner);
                return [.. EnemyFrames((bosses.CrocomireBg2Frames ?? throw Missing(owner)).Writes),
                    .. Enum.GetValues<CrocomireMeltingIllustration>().Select(image => BossBodyFrameSource.Written(
                        CrocomireMeltingFrameNames.Of(image), BossBodySourceDefinitions.CrocomireMeltFirstCell, melting.Tilemap(image)))];
            case BossBodyOwner.MotherBrain:
                return EnemyFrames((bosses.MotherBrainBodyBg2Frames ?? throw Missing(owner)).Writes);
            default:
                throw new InvalidDataException($"Undefined boss body owner {owner}.");
        }
    }

    /// <summary>
    /// Kraid's frames: his upper and lower pages, with and without the priority his growth sets, each on its page of his
    /// layer, and his head at each mouth stage over the upper page.
    /// </summary>
    private static IEnumerable<BossBodyFrameSource> KraidFrames(KraidBackgroundArtwork art)
    {
        BackgroundTile[] working = KraidWorkingMap.Compose(art);
        BackgroundTile[] upper = working[..KraidBackgroundDefinitions.PageCells];
        BackgroundTile[] lower = working[KraidBackgroundDefinitions.LowerPageFirstCell..(KraidBackgroundDefinitions.LowerPageFirstCell + KraidBackgroundDefinitions.PageCells)];
        int lowerCell = KraidBodyLayout.LowerPageTopRow * BossBodyDefinitions.ColumnCount;
        foreach (KraidBodyPage page in new[] { KraidBodyPage.Body, KraidBodyPage.PriorityBody })
        {
            bool inFront = page == KraidBodyPage.PriorityBody;
            yield return BossBodyFrameSource.Written(KraidBodyFrameNames.Of(page, KraidBodyHalf.Upper)
                ?? throw new InvalidDataException("Kraid's upper body page has no frame."), 0, InFront(upper, inFront));
            yield return BossBodyFrameSource.Written(KraidBodyFrameNames.Of(page, KraidBodyHalf.Lower)
                ?? throw new InvalidDataException("Kraid's lower body page has no frame."), lowerCell, InFront(lower, inFront));
        }
        foreach (KraidHeadFrame head in KraidHeadFrame.ByMouthStage)
            yield return BossBodyFrameSource.Written(KraidBodyFrameNames.Head(head.MouthStage), 0, art.HeadTiles(head));

        // Kraid's page with every word's priority set, as his growth sets it, when the page is shown in front.
        static BackgroundTile[] InFront(BackgroundTile[] page, bool inFront) =>
            inFront ? [.. page.Select(tile => tile with { HighPriority = true })] : page;
    }

    /// <summary>Each of a catalog's frames, named for its enemy frame, at the cells its writes reach, the last write to a cell winning.</summary>
    private static IEnumerable<BossBodyFrameSource> EnemyFrames(EnemyBg2FrameCatalog catalog) =>
        catalog.Frames.OrderBy(frame => frame.Key.ToString(), StringComparer.Ordinal).Select(frame =>
        {
            var cells = new SortedDictionary<int, BackgroundTile>();
            foreach (EnemyBg2TilemapWrite write in frame.Value)
            {
                ReadOnlySpan<BackgroundTile> tiles = write.Tiles.Span;
                for (int index = 0; index < tiles.Length; index++)
                    cells[write.FirstCell + index] = tiles[index];
            }
            return new BossBodyFrameSource(BossBodyFrameNames.Of(frame.Key), cells);
        });

    /// <summary>The failure for a boss whose installed artwork is missing.</summary>
    private static InvalidDataException Missing(BossBodyOwner owner) => new($"The installed boss artwork has no {owner} body.");
}
