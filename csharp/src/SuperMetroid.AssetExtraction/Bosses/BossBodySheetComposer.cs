using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
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
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Pre-assembles and pre-colours every boss body frame (#1466, #1515, #1540): each frame a boss shows on its body (Kraid's
/// body, the head it rises with and his head's poses, Phantoon's, Draygon's, Crocomire's and Mother Brain's poses and
/// parts, Crocomire's melt images) as one whole picture painted over the characters the boss's room shows, placed where it
/// lies on the body's area. A frame the cartridge writes only in part over another is pictured whole over it. The cells of
/// each animation a frame shows are pictured again over every frame of that animation. The tilemap words the cartridge
/// writes are read here, once, and nowhere else.
/// </summary>
internal static partial class BossBodySheetComposer
{
    /// <summary>The boss bodies in the order their sheets are composed and listed (#2567).</summary>
    private static readonly BossBodyOwner[] OwnerOrder = [.. Ensure.EveryMemberOnce<BossBodyOwner>(
    [
        BossBodyOwner.Kraid,
        BossBodyOwner.Phantoon,
        BossBodyOwner.Draygon,
        BossBodyOwner.Crocomire,
        BossBodyOwner.MotherBrain,
    ])];

    /// <summary>Crocomire's melting illustrations in the order his body sheet holds their melt states (#2567).</summary>
    private static readonly IReadOnlyList<CrocomireMeltingIllustration> MeltingOrder = Ensure.EveryMemberOnce<CrocomireMeltingIllustration>(
    [
        CrocomireMeltingIllustration.Flesh,
        CrocomireMeltingIllustration.BareSkull,
    ]);

    /// <summary>The first row of the bottom-left screen of Kraid's layer, where his lower body is written.</summary>
    internal const int LowerPageTopRow = 64;

    /// <summary>
    /// The bosses whose body layer gameplay also shades from the room and the menus' colours, with those shades: Mother
    /// Brain's body layer; every other body has only the shades of its own colours.
    /// </summary>
    private static readonly Dictionary<BossBodyOwner, Func<RoomBlockSheetInputs, IReadOnlyList<SheetShade>>> LayerShades =
        new Dictionary<BossBodyOwner, Func<RoomBlockSheetInputs, IReadOnlyList<SheetShade>>>
        {
            [BossBodyOwner.MotherBrain] = inputs => MotherBrainSheetShades.BodyLayer(inputs.Maps, inputs.Bosses),
        };

    /// <summary>The enemy whose rooms each boss's body is drawn in.</summary>
    private static readonly Dictionary<BossBodyOwner, EnemyDefinitionId> Enemies = new()
    {
        [BossBodyOwner.Kraid] = EnemyDefinitionId.Kraid,
        [BossBodyOwner.Phantoon] = EnemyDefinitionId.PhantoonBody,
        [BossBodyOwner.Draygon] = EnemyDefinitionId.DraygonBody,
        [BossBodyOwner.Crocomire] = EnemyDefinitionId.Crocomire,
        [BossBodyOwner.MotherBrain] = EnemyDefinitionId.MotherBrainBody,
    };

    /// <summary>Whether <paramref name="enemy"/> is a boss whose body a body sheet pictures, drawn on its room's background.</summary>
    internal static bool DrawsBodyOf(EnemyDefinitionId enemy) => Enemies.ContainsValue(enemy);

    /// <summary>Composes every boss's body sheet from the installed stock artwork.</summary>
    /// <exception cref="InvalidDataException">
    /// A boss's artwork is missing, no room shows the boss, or a frame reads a character its room rebinds after loading.
    /// </exception>
    internal static IReadOnlyList<ComposedBossBodySheet> Compose(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        // Each boss's sheet composes on its own (#1530), into its owner's place, so the order stays the owners'.
        BossBodyOwner[] owners = OwnerOrder;
        var sheets = new ComposedBossBodySheet[owners.Length];
        try
        {
            Parallel.For(0, owners.Length, index => sheets[index] = Compose(inputs, owners[index]));
        }
        catch (AggregateException failure) when (failure.InnerExceptions.Count == 1)
        {
            // One boss's failure surfaces as itself, with its own stack, as this method documents; several stay together.
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure.InnerExceptions[0]).Throw();
        }
        return sheets;
    }

    /// <summary>The first room state, in room order, whose population places <paramref name="owner"/>.</summary>
    /// <exception cref="InvalidDataException">No room state shows the boss.</exception>
    private static RoomStateSettings FirstStateShowing(BossBodyOwner owner)
    {
        EnemyDefinitionId enemy = Enemies[owner];
        foreach (RoomDefinition room in RetailRooms.All)
        {
            foreach (RoomStateSettings state in RoomStateLists.GetStates(room))
            {
                if (state.EnemyPopulation.Placements.Span.Any(record => record.DefinitionId == enemy))
                    return state;
            }
        }
        throw new InvalidDataException($"No room state shows {owner}.");
    }

    /// <summary>
    /// Composes <paramref name="owner"/>'s sheet over the first room state, in room order, that shows the boss: the
    /// characters every frame reads there, as the tile word sheets were composed before (#1392).
    /// </summary>
    internal static ComposedBossBodySheet Compose(RoomBlockSheetInputs inputs, BossBodyOwner owner)
    {
        BossRoomCharacters characters = RoomBlockUploads.ShownUnderBoss(inputs, FirstStateShowing(owner));
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(characters.Tileset.Palette));
        var painter = new BossBodyPainter(owner, characters, colours);
        foreach (BossBodyFrameSource frame in Frames(inputs.Bosses, owner))
            painter.Add(frame);
        // The cleared layer's blank shows nothing, which the painter confirms.
        ComposedBossBodySheet composed = painter.Compose(BlankOf(owner));
        // The body is pre-coloured in the shades the boss's colour state names (#1512) and a door's dark shades (#3036).
        return composed with { Sheet = ShadedSheets.With(composed.Sheet,
            [.. BossSheetShades.Body(owner, inputs.Bosses, composed.Sheet.Inks, composed.Sheet.Manifest.Palette, colours),
                .. LayerShades.TryGetValue(owner, out Func<RoomBlockSheetInputs, IReadOnlyList<SheetShade>>? layer) ? layer(inputs) : [],
                .. DoorFadeBackgroundShades.Of()]) };
    }

    /// <summary>The installed sheet name of <paramref name="owner"/>'s body sheet.</summary>
    internal static string SheetNameOf(BossBodyOwner owner) => $"BossBody.{BossBodyOwnerKeys.Installed.Of(owner)}";

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
                return KraidFrames(bosses.KraidBackground);
            case BossBodyOwner.Phantoon:
                return EnemyFrames(bosses.PhantoonBg2Frames.Writes);
            case BossBodyOwner.Draygon:
                return EnemyFrames(bosses.DraygonBg2Frames.Writes);
            case BossBodyOwner.Crocomire:
                CrocomireMeltingArtwork melting = bosses.CrocomireMelting;
                return [.. EnemyFrames(bosses.CrocomireBg2Frames.Writes),
                    .. MeltingOrder.SelectMany(image => CrocomireMeltStates(melting, image))];
            case BossBodyOwner.MotherBrain:
                return EnemyFrames(bosses.MotherBrainBodyBg2Frames.Writes);
            default:
                throw new InvalidDataException($"Undefined boss body owner {owner}.");
        }
    }

    /// <summary>
    /// Crocomire's melt image <paramref name="image"/> as his body shows it, once (#1652, #2478): the image before its
    /// dissolve erases a column, every band of its characters unerased, with how the dissolve erases each of its pixels.
    /// Core hides the pixels each band's steps have erased as it shows the image, so no step is pictured.
    /// </summary>
    private static List<BossBodyFrameSource> CrocomireMeltStates(CrocomireMeltingArtwork melting, CrocomireMeltingIllustration image)
    {
        var selected = new Dictionary<ArtworkAnimation, ArtworkFrame>();
        for (int band = 0; band < CrocomireMeltingDefinitions.MeltImageBandCount; band++)
            selected.Add(CrocomireMeltBands.Animation(band), CrocomireMeltBands.Undissolved(image, band));
        BossBodyFrameSource source = BossBodyFrameSource.Written(CrocomireMeltingFrameNames.Of(image),
            BossBodySourceDefinitions.CrocomireMeltFirstCell, melting.Tilemap(image));
        return [source with { Selected = selected, Dissolve = DissolveOf(source) }];
    }

    /// <summary>
    /// How the dissolve erases a melt image's picture, written as <paramref name="source"/> writes it
    /// (#1652, #2478). A cell showing a character of one of the bands the dissolve erases shows that band, and each of its
    /// pixels is erased after the first step whose column lies in the character's group of eight pixel columns and whose
    /// place among each eight steps is the pixel's column in the character
    /// (<see cref="CrocomireMeltDissolveDefinitions.ColumnOrder"/>), down every row of the character. Every other pixel is
    /// kept: a cell showing another character, or a character of the band the dissolve never erases.
    /// </summary>
    private static PictureDissolve DissolveOf(BossBodyFrameSource source)
    {
        const int cell = RoomTileCanvas.CharacterPixels;
        Position16<BackgroundPixels> corner = BossBodyPainter.CornerOf(source.Cells);
        Extent16<ImagePixels> size = BossBodyPainter.PictureSizeOf(source.Cells);
        var pixels = new DissolvingPixel?[size.X * size.Y];
        int bands = Math.Min(CrocomireMeltingDefinitions.MeltImageBandCount, CrocomireMeltDissolveDefinitions.ErasedBands);
        foreach ((int layerCell, BackgroundTile tile) in source.Cells)
        {
            int band = tile.Tile / CrocomireMeltDissolve.BandRowCharacters, group = tile.Tile % CrocomireMeltDissolve.BandRowCharacters;
            if (band >= bands || group >= CrocomireMeltDissolve.BandCharacterCount)
                continue;
            Position16<BackgroundPixels> at = BossBodyPainter.PixelOf(layerCell);
            int left = at.X - corner.X, top = at.Y - corner.Y;
            for (int x = 0; x < cell; x++)
            {
                if (ErasingStep(group, tile.FlipHorizontally ? cell - 1 - x : x) is not { } step)
                    continue;
                for (int y = 0; y < cell; y++)
                    pixels[((top + y) * size.X) + left + x] = new DissolvingPixel(band, step + 1);
            }
        }
        return new PictureDissolve(size, [.. pixels]);
    }

    /// <summary>
    /// The first step of the dissolve that erases pixel column <paramref name="column"/> of a character of group
    /// <paramref name="group"/>, counted from zero, or null when no step does.
    /// </summary>
    private static int? ErasingStep(int group, int column)
    {
        ReadOnlySpan<int> order = CrocomireMeltDissolveDefinitions.ColumnOrder;
        for (int step = 0; step < order.Length; step++)
        {
            if (order[step] / CrocomireMeltDissolveDefinitions.GroupColumns == group && step % CrocomireMeltDissolveDefinitions.GroupColumns == column)
                return step;
        }
        return null;
    }

    /// <summary>
    /// Kraid's frames (#1540): his body below his head, its upper half on the top-left screen of his layer and its lower
    /// half on the bottom-left, and the head his body rises with on the upper half's top rows, each drawn behind the sprites
    /// as he rises; and his head at each mouth stage on those same rows. His growth draws his body and the head it rises with
    /// in front of the sprites, the same pictures at another depth (#1634), so neither is pictured again. Each mouth stage is
    /// written over every one of those rows, so his head shows there alone and is a frame beside his body, never over it.
    /// </summary>
    private static IEnumerable<BossBodyFrameSource> KraidFrames(KraidBackgroundArtwork art)
    {
        BackgroundTile[] working = KraidWorkingMap.Compose(art);
        BackgroundTile[] upper = working[..KraidWorkingMapDefinitions.PageCells];
        BackgroundTile[] lower = working[KraidWorkingMapDefinitions.LowerPageFirstCell..(KraidWorkingMapDefinitions.LowerPageFirstCell + KraidWorkingMapDefinitions.PageCells)];
        int lowerCell = LowerPageTopRow * BossBodySourceDefinitions.LayerColumns;
        const int headCells = KraidWorkingMapDefinitions.HeadImageCells;
        yield return BossBodyFrameSource.Written(KraidBodyFrameNames.RestingHead, 0, upper[..headCells]);
        BossBodyFrameSource body = BossBodyFrameSource.Written(KraidBodyFrameNames.Body, headCells, upper[headCells..]);
        for (int index = 0; index < lower.Length; index++)
            body.Cells[lowerCell + index] = lower[index];
        yield return body;
        // Each installed head pose holds exactly the head's rows (KraidHeadTilemapAtlas), so it shows there alone.
        foreach (KraidHeadFrame head in KraidHeadTilemapFormat.HeadsByMouthStage)
            yield return BossBodyFrameSource.Written(head.FrameName, 0, art.HeadTiles(head));
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
        // Each frame takes its installed sheet key (#2283), the key the enemy sheets give it.
        return written.Keys.OrderBy(EnemyFrameSheetKeys.Of, StringComparer.Ordinal)
            .Select(frame => new BossBodyFrameSource(EnemyFrameSheetKeys.Of(frame), Whole(frame))
            {
                PictureShift = new(MotherBrainBodyBg2FrameDefinitions.FramesShiftedRight.GetValueOrDefault(frame),
                    BossBodySourceDefinitions.FramesLowered.GetValueOrDefault(frame)),
            });

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

}
