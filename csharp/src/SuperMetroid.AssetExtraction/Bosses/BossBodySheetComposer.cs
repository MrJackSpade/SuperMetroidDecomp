using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Pre-assembles and pre-colours every boss body frame (#1466, #1515): each frame a boss shows on its body (Kraid's whole
/// body and head, Phantoon's, Draygon's, Crocomire's and Mother Brain's poses and parts, Crocomire's melt images) as one
/// whole picture painted over the characters the boss's room shows, placed where it lies on the body's area, with the
/// rectangles it covers. A frame the cartridge writes only in part over another is pictured whole over it. The cells of
/// each animation a frame shows are pictured again over every frame of that animation. The tilemap words the cartridge
/// writes are read here, once, and nowhere else.
/// </summary>
internal static class BossBodySheetComposer
{
    /// <summary>The first row of the bottom-left screen of Kraid's layer, where his lower body is written.</summary>
    internal const int LowerPageTopRow = 64;

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
        // Each boss's sheet composes on its own (#1530), into its owner's place, so the order stays the owners'.
        BossBodyOwner[] owners = Enum.GetValues<BossBodyOwner>();
        var sheets = new ComposedBossBodySheet[owners.Length];
        Parallel.For(0, owners.Length, index => sheets[index] = Compose(inputs, owners[index]));
        return sheets;
    }

    /// <summary>
    /// Composes <paramref name="owner"/>'s sheet over the first room state, in room order, that shows the boss: the
    /// characters every frame reads there, as the tile word sheets were composed before (#1392).
    /// </summary>
    private static ComposedBossBodySheet Compose(RoomBlockSheetInputs inputs, BossBodyOwner owner)
    {
        EnemyDefinitionId enemy = Enemies[owner];
        BossRoomCharacters? characters = null;
        foreach (RoomHeaderDefinition room in RetailRoomHeaders.All)
        {
            RoomStateSettings? state = RoomStateLists.GetStates(room)
                .FirstOrDefault(candidate => candidate.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == enemy));
            if (state is not null)
            {
                characters = RoomBlockUploads.ShownUnderBoss(inputs, room, state);
                break;
            }
        }
        if (characters is null)
            throw new InvalidDataException($"No room state shows {owner}.");
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(characters.Tileset.Palette));
        var painter = new BossBodyPainter(owner, characters, colours);
        foreach (BossBodyFrameSource frame in Frames(inputs.Bosses, owner))
            painter.Add(frame);
        // The cleared layer's blank shows nothing, which the painter confirms.
        ComposedBossBodySheet composed = painter.Compose(BlankOf(owner));
        // The body is pre-coloured in the shades the boss's colour state names (#1512).
        return composed with { Sheet = ShadedSheets.With(composed.Sheet,
            [.. BossSheetShades.Body(owner, inputs.Bosses, composed.Sheet.PaletteIndices, colours),
                .. owner == BossBodyOwner.MotherBrain ? MotherBrainSheetShades.Background(inputs.Maps, inputs.Bosses) : []]) };
    }

    /// <summary>The installed sheet name of <paramref name="owner"/>'s body sheet.</summary>
    internal static string SheetNameOf(BossBodyOwner owner) => $"BossBody.{owner}";

    /// <summary>The tile <paramref name="owner"/> clears its body layer with.</summary>
    internal static BackgroundTile BlankOf(BossBodyOwner owner) => owner switch
    {
        BossBodyOwner.Kraid => KraidWorkingMapDefinitions.BlankTile,
        BossBodyOwner.Phantoon or BossBodyOwner.Draygon or BossBodyOwner.Crocomire or BossBodyOwner.MotherBrain => BossBodySourceDefinitions.BlankTile,
        _ => throw new InvalidDataException($"Undefined boss body owner {owner}."),
    };

    /// <summary>Every frame <paramref name="owner"/> shows, whole, at the layer cells it covers.</summary>
    /// <exception cref="InvalidDataException">The boss's installed artwork is missing.</exception>
    internal static IEnumerable<BossBodyFrameSource> Frames(BossSourceArtwork bosses, BossBodyOwner owner)
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
                    .. Enum.GetValues<CrocomireMeltingIllustration>().SelectMany(image => CrocomireMeltStates(melting, image))];
            case BossBodyOwner.MotherBrain:
                return EnemyFrames((bosses.MotherBrainBodyBg2Frames ?? throw Missing(owner)).Writes);
            default:
                throw new InvalidDataException($"Undefined boss body owner {owner}.");
        }
    }

    /// <summary>
    /// Each state of Crocomire's melt image <paramref name="image"/> as his body shows it, whole (#1513): the image before
    /// its dissolve erases a column and after each erase, every band of its characters at the dissolve step the band was
    /// last shown at (<see cref="CrocomireMelting.BandStepAfter"/>). The updates in which the image's bands are first shown,
    /// one an update, show the unerased image whole; his body frames are replaced by the image before any band is shown, so
    /// no body frame is pictured under a band.
    /// </summary>
    private static List<BossBodyFrameSource> CrocomireMeltStates(CrocomireMeltingArtwork melting, CrocomireMeltingIllustration image)
    {
        int bands = CrocomireMelting.BandCount(image);
        var states = new List<BossBodyFrameSource>();
        for (int erased = 0; erased <= CrocomireMeltingDefinitions.ColumnCount; erased++)
        {
            var selected = new Dictionary<ArtworkAnimation, ArtworkFrame>();
            for (int band = 0; band < bands; band++)
            {
                selected.Add(CrocomireMelting.MeltBandAnimation(band), CrocomireMelting.MeltBandFrame(
                    new CrocomireMeltBandPicture(image, CrocomireMelting.BandStepAfter(image, band, erased)), band));
            }
            states.Add(BossBodyFrameSource.Written(CrocomireMeltingFrameNames.Of(image, erased), BossBodySourceDefinitions.CrocomireMeltFirstCell,
                melting.Tilemap(image)) with { Selected = selected });
        }
        return states;
    }

    /// <summary>
    /// Kraid's frames: his whole body, its upper half on the top-left screen of his layer and its lower half on the
    /// bottom-left, with and without the priority his growth sets, and his head at each mouth stage over its top rows.
    /// </summary>
    private static IEnumerable<BossBodyFrameSource> KraidFrames(KraidBackgroundArtwork art)
    {
        BackgroundTile[] working = KraidWorkingMap.Compose(art);
        BackgroundTile[] upper = working[..KraidWorkingMapDefinitions.PageCells];
        BackgroundTile[] lower = working[KraidWorkingMapDefinitions.LowerPageFirstCell..(KraidWorkingMapDefinitions.LowerPageFirstCell + KraidWorkingMapDefinitions.PageCells)];
        int lowerCell = LowerPageTopRow * BossBodySourceDefinitions.LayerColumns;
        foreach (KraidBodyAppearance appearance in new[] { KraidBodyAppearance.BehindSprites, KraidBodyAppearance.InFrontOfSprites })
        {
            bool inFront = appearance == KraidBodyAppearance.InFrontOfSprites;
            BossBodyFrameSource body = BossBodyFrameSource.Written(KraidBodyFrameNames.Of(appearance)
                ?? throw new InvalidDataException("Kraid's body has no frame."), 0, InFront(upper, inFront));
            BackgroundTile[] lowerShown = InFront(lower, inFront);
            for (int index = 0; index < lowerShown.Length; index++)
                body.Cells[lowerCell + index] = lowerShown[index];
            yield return body;
        }
        foreach (KraidHeadFrame head in KraidHeadTilemapFormat.HeadsByMouthStage)
            yield return BossBodyFrameSource.Written(KraidBodyFrameNames.Head(head.MouthStage), 0, art.HeadTiles(head));

        // Kraid's page with every word's priority set, as his growth sets it, when the page is shown in front.
        static BackgroundTile[] InFront(BackgroundTile[] page, bool inFront) =>
            inFront ? [.. page.Select(tile => tile with { HighPriority = true })] : page;
    }

    /// <summary>
    /// Each of a catalog's frames, named for its enemy frame, whole: at the cells its writes reach, the last write to a
    /// cell winning, written over the whole frame it is drawn over (<see cref="BossBodySourceDefinitions.FramesDrawnOver"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">A frame is drawn over one the catalog does not hold.</exception>
    private static IEnumerable<BossBodyFrameSource> EnemyFrames(EnemyBg2FrameCatalog catalog)
    {
        var written = new Dictionary<EnemyFrameId, SortedDictionary<int, BackgroundTile>>();
        foreach (KeyValuePair<EnemyFrameId, EnemyBg2TilemapWrite[]> frame in catalog.Frames)
        {
            var cells = new SortedDictionary<int, BackgroundTile>();
            foreach (EnemyBg2TilemapWrite write in frame.Value)
            {
                ReadOnlySpan<BackgroundTile> tiles = write.Tiles.Span;
                for (int index = 0; index < tiles.Length; index++)
                    cells[write.FirstCell + index] = tiles[index];
            }
            written.Add(frame.Key, cells);
        }
        return written.Keys.OrderBy(frame => frame.ToString(), StringComparer.Ordinal)
            .Select(frame => new BossBodyFrameSource(BossBodyFrameNames.Of(frame), Whole(frame)));

        // The frame's cells written over the whole frame it is drawn over, with the part that fills its gap written over them.
        SortedDictionary<int, BackgroundTile> Whole(EnemyFrameId frame)
        {
            SortedDictionary<int, BackgroundTile> cells = Beneath(frame);
            if (BossBodySourceDefinitions.FramesPicturedWith.TryGetValue(frame, out EnemyFrameId filling))
            {
                if (!written.ContainsKey(filling))
                    throw new InvalidDataException($"Body frame {frame} is pictured with {filling}, which its boss's frames do not hold.");
                cells = new SortedDictionary<int, BackgroundTile>(cells);
                foreach ((int cell, BackgroundTile tile) in Whole(filling))
                    cells[cell] = tile;
            }
            return cells;
        }

        // The frame's cells written over the whole frame it is drawn over.
        SortedDictionary<int, BackgroundTile> Beneath(EnemyFrameId frame)
        {
            if (!BossBodySourceDefinitions.FramesDrawnOver.TryGetValue(frame, out EnemyFrameId beneath))
                return written[frame];
            if (!written.ContainsKey(beneath))
                throw new InvalidDataException($"Body frame {frame} is drawn over {beneath}, which its boss's frames do not hold.");
            var cells = new SortedDictionary<int, BackgroundTile>(Whole(beneath));
            foreach ((int cell, BackgroundTile tile) in written[frame])
                cells[cell] = tile;
            return cells;
        }
    }

    /// <summary>The failure for a boss whose installed artwork is missing.</summary>
    private static InvalidDataException Missing(BossBodyOwner owner) => new($"The installed boss artwork has no {owner} body.");
}
