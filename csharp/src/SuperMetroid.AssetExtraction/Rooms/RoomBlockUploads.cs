using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What each tileset's rooms show over their characters after loading (#1392, #1466), each as a value of the appearance
/// input gameplay names it by, so its block sheet pictures every block for each value the inputs it depends on can take:
/// the animated tiles their room effects run, the Tourian statues' frames, the collectibles in each slot, Kraid's room
/// backdrop and, in the Ceres Ridley arena, its encounters' looks on the sprites' artwork.
/// </summary>
internal static class RoomBlockUploads
{
    /// <summary>The tilesets some retail room state loads, each once in room order: those a block sheet is composed for.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets() =>
        [.. RoomHeaderDefinitions.All.SelectMany(RoomStateSelection.GetStates)
            .Select(state => RoomTilesets.Get(state.GraphicsSet)).Distinct()];

    /// <summary>The rooms of each tileset and what they show over their characters, in room order.</summary>
    /// <exception cref="InvalidDataException">A tileset is loaded both inside and outside Ceres.</exception>
    internal static IReadOnlyDictionary<TilesetDefinition, TilesetUploads> ByTileset(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var tilesets = new Dictionary<TilesetDefinition, TilesetUploads>();
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            bool ceres = room.AreaIndex == AreaId.Ceres;
            foreach (RoomStateSettings state in RoomStateSelection.GetStates(room))
            {
                TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
                if (!tilesets.TryGetValue(tileset, out TilesetUploads? uploads))
                    tilesets.Add(tileset, uploads = new TilesetUploads(ceres, inputs));
                else if (uploads.Ceres != ceres)
                    throw new InvalidDataException($"Tileset {tileset} is loaded both inside and outside Ceres.");
                foreach (AppearanceOption option in Of(inputs, state))
                    uploads.Add(option);
                foreach (AnimatedCharacterPlacement frame in AnimatedFrames(inputs, room, state).Where(frame => frame.Set == BgCharacterSet.Tiles))
                    uploads.AddAnimated(frame);
                if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Crocomire))
                {
                    foreach (AnimatedCharacterPlacement frame in CrocomireMeltBandFrames(inputs))
                        uploads.AddAnimated(frame);
                }
                LibraryBackgroundProgram? library = state.Background is { } background ? LibraryBackgroundPrograms.Of(background) : null;
                RoomBlockArtwork blockArtwork = RoomScope.BlockArtworkOf(new SelectedRoom(room, state));
                BgCharacterUpload[] loadedObjects = [];
                AppearanceOption[] encounterLooks = [];
                if (blockArtwork == RoomBlockArtwork.Sprites)
                {
                    loadedObjects = [.. ObjectPageUploads(inputs, state)];
                    encounterLooks = [.. EncounterLookOptions(inputs, state)];
                    uploads.AddObjectPage(loadedObjects, encounterLooks);
                }
                if (library is not null)
                {
                    foreach (DoorDefinition? door in DoorVariants(library))
                        uploads.AddBackground(Replay(inputs, room, tileset, library, door));
                }
                // BG3 shows the HUD over the layer-3 characters the room's load binds, and its effects' pages over their
                // frames.
                uploads.AddBg3(Loaded(inputs, tileset), EffectsIn(state));
                TileWordWriter[] writers = [.. WritersIn(state)];
                if (writers.Length > 0 && blockArtwork == RoomBlockArtwork.Sprites)
                {
                    // Words written over a room whose backgrounds show the sprites' artwork show over what its load and its
                    // encounters place there.
                    BgCharacterUpload[] written = [.. loadedObjects, .. encounterLooks.Select(option => option.Upload)];
                    var content = new BgArtworkBindings();
                    foreach (BgCharacterUpload placement in written)
                        placement.BindTo(content);
                    uploads.AddWriters(writers, content, blockArtwork, written);
                }
                else if (writers.Length > 0)
                {
                    // Words written over the room show over what its load binds.
                    uploads.AddWriters(writers, Loaded(inputs, tileset), blockArtwork, []);
                }
                if (ScrollingSkyState.IsScrollingSkyRoomMain(state.MainCallback))
                {
                    // The sky's rows show over what the room's load binds.
                    ScrollingSkyChunkTable variant = ScrollingSkyState.VariantOf(state.MainCallback);
                    uploads.AddBackground(new RoomBackgroundMap(RoomBackgroundImage.SkyNameOf(variant, tileset),
                        BackgroundTileWords.ToWords(inputs.Skies.Tilemap(variant).Tiles), Loaded(inputs, tileset)));
                }
            }
        }
        foreach ((TilesetDefinition _, TilesetUploads uploads) in tilesets.Where(entry => !entry.Value.Ceres))
            uploads.AddCollectibles(Collectibles());
        return tilesets;
    }

    /// <summary>What one state of a room shows over its tile characters after it loads, collectibles and animations aside: Kraid's backdrop.</summary>
    private static IEnumerable<AppearanceOption> Of(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
        {
            yield return new AppearanceOption(KraidBackdropAppearance.Instance, RoomAppearanceValues.Shown,
                new BgCharacterUpload(RoomBgArtwork.KraidBackdrop, BgCharacterSet.Tiles, RoomTileCharacters.KraidBackdropFirst,
                    (inputs.Bosses.KraidBackground ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters."))
                        .RoomBackgroundTiles.Characters));
        }
    }

    /// <summary>
    /// The frames one state of a room's animations paint over the block characters (#1406): its room effects' animated
    /// tiles and the Tourian statues' pictures.
    /// </summary>
    private static IEnumerable<AnimatedCharacterPlacement> AnimatedFrames(RoomBlockSheetInputs inputs, RoomHeaderDefinition room,
        RoomStateSettings state)
    {
        foreach (RoomFxRecordDefinition record in state.Fx?.Records ?? [])
        {
            for (int bit = 0; bit < AreaAnimatedTileObjectDefinitions.ObjectsPerArea; bit++)
            {
                if (!record.AnimatedTileSlots.Includes(bit))
                    continue;
                foreach (AnimatedCharacterPlacement frame in AnimatedTiles(inputs, AreaAnimatedTileObjects.Read(room.AreaIndex, bit)))
                    yield return frame;
            }
        }
        if (state.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations)
        {
            foreach (TourianStatueAnimatedTileProgramDefinition statue in TourianStatueAnimatedTiles.All)
            {
                // Phantoon's and Ridley's statues paint characters outside the room's tile characters, which no block reads.
                if (!BgCharacterWords.AmongTiles(AnimatedTileVramDestinations.WordOf(statue.AnimatedObject)))
                    continue;
                foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
                {
                    yield return AnimatedCharacterPlacement.At(AnimatedTileFrames.Picture(statue.AnimatedObject, picture), AnimatedTileVramDestinations.WordOf(statue.AnimatedObject),
                        inputs.AnimatedTiles.TourianStatuePicture(statue.AnimatedObject, picture, statue.FrameByteCount));
                }
            }
        }
    }

    /// <summary>
    /// What Crocomire's melt shows over his characters (#1392, #1406): each band of each melt image after every number of
    /// columns erased, as the frame of the band's animation the runtime selects for it.
    /// </summary>
    private static IEnumerable<AnimatedCharacterPlacement> CrocomireMeltBandFrames(RoomBlockSheetInputs inputs)
    {
        CrocomireMeltingArtwork artwork = inputs.Bosses.CrocomireMelting
            ?? throw new InvalidDataException("Installed boss artwork has no Crocomire melting graphics.");
        foreach (CrocomireMeltingIllustration illustration in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int step = 0;
            foreach (byte[] image in CrocomireMeltDissolve.Steps(artwork, illustration))
            {
                for (int band = 0; band < CrocomireMelting.BandCount(illustration); band++)
                {
                    yield return new AnimatedCharacterPlacement(CrocomireMelting.MeltBandFrame(new CrocomireMeltBandPicture(illustration, step), band),
                        BgCharacterSet.Tiles, CrocomireMeltDissolve.BandFirstCharacter(band), CrocomireMeltDissolve.BandCharacters(image, band));
                }
                step++;
            }
        }
    }

    /// <summary>No door, for the background's ordinary picture, and each door that shows its own picture of it.</summary>
    private static IEnumerable<DoorDefinition?> DoorVariants(LibraryBackgroundProgram program) => [null, .. program.Background.PictureDoors];

    /// <summary>
    /// Replays <paramref name="program"/> as a room of <paramref name="tileset"/> entered through <paramref name="door"/>
    /// runs it: the BG2 pages it leaves, over the characters the room loads.
    /// </summary>
    private static RoomBackgroundMap Replay(RoomBlockSheetInputs inputs, RoomHeaderDefinition room, TilesetDefinition tileset,
        LibraryBackgroundProgram program, DoorDefinition? door)
    {
        BgArtworkBindings content = Loaded(inputs, tileset);
        var vram = new SnesVram();
        // A door no transfer names stands for every door the program does not tell apart.
        DoorDefinition entry = door ?? new DoorDefinition("no library background door", room,
            new DoorOrientation(DoorDirection.Right, DoorClosingBehavior.None), default, default, DoorSamusSpeed.None, DoorSetupCode.None);
        LibraryBackgroundReplay.Execute(vram, program,
            entry, new LibraryBackgroundArtwork
            {
                TilemapArt = inputs.BackgroundTilemaps,
                SkyArt = inputs.Skies,
                HudArt = inputs.Hud,
                CharacterArt = inputs.Characters,
            });
        ushort[] words = [.. Enumerable.Range(0, RoomBackgroundImage.Words).Select(word => vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word))];
        return new RoomBackgroundMap(RoomBackgroundImage.NameOf(program.Background, door, tileset), words, content);
    }

    /// <summary>What each enemy writes over its room's BG1 or BG2: the escape warnings Mother Brain and Ceres Ridley type.</summary>
    private static readonly Dictionary<EnemyDefinitionId, TileWordWriter[]> EnemyWriters = new()
    {
        [EnemyDefinitionId.MotherBrainBody] = [TileWordWriter.ZebesEscapeText],
        [EnemyDefinitionId.RidleyCeres] = [TileWordWriter.CeresEscapeText],
    };

    /// <summary>What writes tile words over <paramref name="state"/>'s BG1 or BG2.</summary>
    private static IEnumerable<TileWordWriter> WritersIn(RoomStateSettings state) =>
        state.EnemyPopulation.Records.ToArray().SelectMany(record => EnemyWriters.TryGetValue(record.DefinitionId, out TileWordWriter[]? writers)
            ? writers : []).Distinct();

    /// <summary>
    /// What <paramref name="state"/>'s room uploads over the OBJ character table as it loads, which the Ceres Ridley arena's
    /// blocks show: each enemy's tiles its graphics set loads, named as the table records them.
    /// </summary>
    /// <exception cref="InvalidDataException">An enemy's tiles are not installed.</exception>
    private static IEnumerable<BgCharacterUpload> ObjectPageUploads(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in EnemyGraphicsSet.LayOut(CompiledRoomEnemyDefinitions.Instance, state.EnemyGraphicsSet))
        {
            ReadOnlyMemory<byte> tiles = inputs.Objects.Enemies.TilesOf(entry.DefinitionId);
            SpriteArtworkPlacement placed = EncounterCharacterPlacements.Placed(new GraphicsSetTileCharacters(entry));
            yield return new BgCharacterUpload(new BgArtworkIdentity($"{placed.Source}"), BgCharacterSet.Tiles, placed.FirstCharacter, tiles);
        }
    }

    /// <summary>
    /// What <paramref name="state"/>'s encounters show over the OBJ character table after its room loads, in the order they
    /// show them, each as its look shown (#1466): Ceres Ridley's getaway its escape timer and its warning.
    /// </summary>
    private static IEnumerable<AppearanceOption> EncounterLookOptions(RoomBlockSheetInputs inputs, RoomStateSettings state) =>
        EncounterCharacterPlacements.Catalog.Of(state.EnemyGraphicsSet).Select(upload => new AppearanceOption(
            new EncounterLookAppearance(upload.Source), RoomAppearanceValues.Shown,
            new BgCharacterUpload(new BgArtworkIdentity($"{upload.Source}"), BgCharacterSet.Tiles, upload.FirstCharacter,
                SpriteArtworkBytes.Of(upload.Source, inputs.Objects))));

    /// <summary>The HUD's characters as gameplay setup uploads them, before any room loads.</summary>
    internal static BgCharacterUpload HudCharacters(RoomBlockSheetInputs inputs) =>
        new(BgArtworkIdentity.Asset(InstalledArtworkId.StandardHudTiles), BgCharacterSet.Layer3, 0, Ensure.NotNull(inputs).Hud.Bytes[..HudTileAtlasFormat.CharacterByteCount]);

    /// <summary>The room effects whose BG3 pages <paramref name="state"/>'s rooms show, and the characters each animates on BG3.</summary>
    private static IEnumerable<RoomFxType> EffectsIn(RoomStateSettings state) =>
        (state.Fx?.Records ?? []).Select(record => record.Type)
            .Where(RoomLayer3FxState.ShowsPage).Distinct();

    /// <summary>Each frame a room effect of <paramref name="type"/> animates over BG3's characters.</summary>
    internal static IEnumerable<AnimatedCharacterPlacement> EffectCharacters(RoomBlockSheetInputs inputs, RoomFxType type)
    {
        AnimatedTileObject animatedObject = RoomFxAnimatedTilesState.ObjectOf(type);
        if (animatedObject == AnimatedTileObject.None)
            yield break;
        RoomFxAnimatedTileLoop mechanics = RoomFxAnimatedTiles.Require(animatedObject);
        for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            yield return Animated(inputs, animatedObject, frame, mechanics.Image(mechanics.Frames[frame]),
                mechanics.FrameByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
    }

    /// <summary>Bindings holding the HUD's characters and what a room of <paramref name="tileset"/> binds as it loads.</summary>
    internal static BgArtworkBindings Loaded(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        var content = new BgArtworkBindings();
        HudCharacters(inputs).BindTo(content);
        foreach (BgCharacterUpload placement in LoadUploads(inputs, tileset))
            placement.BindTo(content);
        return content;
    }

    /// <summary>
    /// What a boss's body is shown over in <paramref name="state"/> of <paramref name="room"/> (#1466): what the room loads
    /// and its library background leaves, what the room binds after loading, and the frames its animations paint over its
    /// tile characters, Crocomire's melt bands among them in his room.
    /// </summary>
    internal static BossRoomCharacters ShownUnderBoss(RoomBlockSheetInputs inputs, RoomHeaderDefinition room, RoomStateSettings state)
    {
        Ensure.NotNull(inputs);
        TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
        LibraryBackgroundProgram? library = state.Background is { } background ? LibraryBackgroundPrograms.Of(background) : null;
        BgArtworkBindings content = library is not null
            ? Replay(inputs, room, tileset, library, DoorVariants(library).First()).Content : Loaded(inputs, tileset);
        AnimatedCharacterPlacement[] animated = [.. AnimatedFrames(inputs, room, state).Where(frame => frame.Set == BgCharacterSet.Tiles),
            .. state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Crocomire)
                ? CrocomireMeltBandFrames(inputs) : []];
        return new BossRoomCharacters(tileset, content,
            [.. LoadUploads(inputs, tileset), HudCharacters(inputs)], [.. Of(inputs, state).Select(option => option.Upload)], animated);
    }

    /// <summary>
    /// The common room elements' and the tileset's characters a room of <paramref name="tileset"/> binds as it loads, in
    /// the order the cartridge copies them (#1350), with the bytes the installed artwork holds for each.
    /// </summary>
    internal static IReadOnlyList<BgCharacterUpload> LoadUploads(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        // Preserve $82:E783's transfer order, not merely its two destinations. Ordinary area art ends before the common
        // room elements' characters, but Ceres tileset $11 holds all 1024 tile characters. The tileset must therefore bind
        // over the common room elements; reversing these produces the vertical-stripe corruption seen when CRE replaces
        // Ceres tiles.
        return
        [
            new BgCharacterUpload(RoomBgArtwork.CommonRoomElements, BgCharacterSet.Tiles, RoomTileCharacters.CommonRoomElementsFirst,
                inputs.Characters.Cre.Characters),
            new BgCharacterUpload(RoomBgArtwork.Tileset(tileset.Characters), BgCharacterSet.Tiles, RoomTileCharacters.TilesetFirst,
                inputs.Characters.Get(tileset.Characters).Characters),
        ];
    }

    /// <summary>Each frame an animated-tile object the port runs shows; none for an object it does not animate.</summary>
    private static IEnumerable<AnimatedCharacterPlacement> AnimatedTiles(RoomBlockSheetInputs inputs, AnimatedTileObject animatedObject)
    {
        if (WreckedShipTreadmills.TryResolve(animatedObject, out WreckedShipTreadmillLoop? treadmill))
        {
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return Animated(inputs, animatedObject, frame, treadmill.Image(frame), WreckedShipTreadmillMechanicsDefinitions.FrameByteCount,
                    AnimatedTileVramDestinations.WordOf(animatedObject));
            yield break;
        }
        if (animatedObject is not (AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling))
            yield break;
        RoomFxAnimatedTileLoop mechanics = RoomFxAnimatedTiles.Require(animatedObject);
        for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            yield return Animated(inputs, animatedObject, frame, mechanics.Image(mechanics.Frames[frame]),
                mechanics.FrameByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
    }

    /// <summary>Frame <paramref name="frame"/> of <paramref name="animatedObject"/>'s animation, pictured from its installed characters.</summary>
    private static AnimatedCharacterPlacement Animated(RoomBlockSheetInputs inputs, AnimatedTileObject animatedObject, int frame,
        AnimatedTileImage image, int byteCount, int firstWord) =>
        AnimatedCharacterPlacement.At(AnimatedTileFrames.Frame(animatedObject, frame), firstWord, Frame(inputs, image, byteCount));

    /// <summary>The installed bytes of an animated tile picture.</summary>
    private static ReadOnlyMemory<byte> Frame(RoomBlockSheetInputs inputs, AnimatedTileImage image, int byteCount) =>
        inputs.AnimatedTiles.TryResolve(image, byteCount, out ReadOnlyMemory<byte> frame) ? frame
            : throw new InvalidDataException($"Installed animated tiles hold no {byteCount}-byte picture {image}.");

    /// <summary>
    /// Each collectible's cartridge graphics in each slot, with the tiles of the blocks it shows there (#1453): the stock
    /// pictures of the blocks named for the collectible in the slot, which the room block sheet's override restyles.
    /// </summary>
    private static IEnumerable<CollectibleSlotCharacters> Collectibles()
    {
        foreach (InWorldCollectibleKind kind in RoomPlmDynamicCollectibleGraphicsDefinitions.Kinds)
        {
            RoomPlmDynamicCollectibleGraphic graphic = DynamicCollectibleGraphics.Get(kind);
            for (int slot = 0; slot < CollectiblePlms.GraphicsSlots; slot++)
                yield return new CollectibleSlotCharacters(new AppearanceOption(new CollectibleSlotAppearance(slot), CollectibleSlotAppearance.ValueOf(graphic.Kind),
                        CollectibleSlotBlocks.SlotUpload(graphic.Kind, slot, graphic.Tiles)),
                    CollectibleSlotBlocks.FirstBlockOf(slot), CollectibleSlotBlocks.Tiles(slot, graphic.PaletteOffsets.Span));
        }
    }

    /// <summary>What one tileset's rooms show over their characters.</summary>
    /// <param name="ceres">Whether the tileset's rooms are Ceres rooms, which load no common room elements.</param>
    /// <param name="inputs">The installed artwork the uploads are drawn from.</param>
    internal sealed class TilesetUploads(bool ceres, RoomBlockSheetInputs inputs)
    {
        /// <summary>What the rooms show over their tile characters after loading, collectibles aside, each once.</summary>
        private readonly List<AppearanceOption> options = [];

        /// <summary>The collectibles in each slot.</summary>
        private readonly List<CollectibleSlotCharacters> collectibles = [];

        /// <summary>The library backgrounds the tileset's rooms load, by name.</summary>
        private readonly Dictionary<string, RoomBackgroundMap> backgrounds = new(StringComparer.Ordinal);

        /// <summary>What the rooms show over BG3's characters after loading: their effects' frames, each once.</summary>
        private readonly List<AppearanceOption> bg3Options = [];

        /// <summary>The library backgrounds the tileset's rooms load.</summary>
        internal IEnumerable<RoomBackgroundMap> Backgrounds => backgrounds.Values;

        /// <summary>Adds a library background unless one of its name is known.</summary>
        internal void AddBackground(RoomBackgroundMap background) => backgrounds.TryAdd(background.Name, background);

        /// <summary>What writes tile words over the tileset's rooms.</summary>
        internal HashSet<TileWordWriter> Writers { get; } = [];

        /// <summary>
        /// What a room whose blocks show the sprites' artwork has there as it loads, or null when none of the tileset's rooms
        /// shows it.
        /// </summary>
        internal IReadOnlyList<BgCharacterUpload>? ObjectPageLoad { get; private set; }

        /// <summary>The looks such a room's encounters show there after it loads, in the order they show them.</summary>
        internal IReadOnlyList<AppearanceOption> ObjectPageLooks { get; private set; } = [];

        /// <summary>
        /// Records what a room whose blocks show the sprites' artwork has there: <paramref name="loaded"/> as it loads, and
        /// the looks <paramref name="looks"/> its encounters show after.
        /// </summary>
        /// <exception cref="InvalidDataException">Another such room of the tileset has otherwise.</exception>
        internal void AddObjectPage(IReadOnlyList<BgCharacterUpload> loaded, IReadOnlyList<AppearanceOption> looks)
        {
            if (ObjectPageLoad is { } known && (!known.Select(PagePlacement.Of).SequenceEqual(loaded.Select(PagePlacement.Of))
                || !ObjectPageLooks.Select(option => PagePlacement.Of(option.Upload)).SequenceEqual(looks.Select(option => PagePlacement.Of(option.Upload)))))
                throw new InvalidDataException("Two rooms of one tileset show different sprites' artwork.");
            ObjectPageLoad ??= loaded;
            if (ObjectPageLooks.Count == 0)
                ObjectPageLooks = looks;
        }

        /// <summary>What the first room shows BG3's words over, or null before a room is recorded.</summary>
        internal BgArtworkBindings? Bg3Content { get; private set; }

        /// <summary>The room effects whose BG3 pages the tileset's rooms show.</summary>
        internal HashSet<RoomFxType> FxTypes { get; } = [];

        /// <summary>What the tileset's rooms show over BG3's characters after loading: their effects' frames.</summary>
        internal IReadOnlyList<AppearanceOption> Bg3Options => bg3Options;

        /// <summary>Records a room showing BG3 over <paramref name="content"/>'s layer-3 characters, with <paramref name="effects"/>' pages.</summary>
        internal void AddBg3(BgArtworkBindings content, IEnumerable<RoomFxType> effects)
        {
            Bg3Content ??= content;
            foreach (RoomFxType effect in effects)
            {
                if (!FxTypes.Add(effect))
                    continue;
                foreach (AnimatedCharacterPlacement frame in EffectCharacters(Inputs, effect))
                    AddOnce(bg3Options, frame.Option);
            }
        }

        /// <summary>What the first room with writers shows their words over, or null without writers.</summary>
        internal BgArtworkBindings? WordContent { get; private set; }

        /// <summary>The artwork the rooms whose writers write words show their blocks over, which the words show over too.</summary>
        internal RoomBlockArtwork WordArtwork { get; private set; }

        /// <summary>The placements <see cref="WordContent"/> binds beyond the room's load, whose characters the words show.</summary>
        internal IReadOnlyList<BgCharacterUpload> WordUploads { get; private set; } = [];

        /// <summary>
        /// Records that a room's <paramref name="writers"/> write words over its blocks' <paramref name="artwork"/>, shown over
        /// <paramref name="content"/>, which binds <paramref name="uploads"/> beyond the room's load.
        /// </summary>
        /// <exception cref="InvalidDataException">Another room of the tileset writes words over other artwork.</exception>
        internal void AddWriters(IEnumerable<TileWordWriter> writers, BgArtworkBindings content, RoomBlockArtwork artwork,
            IReadOnlyList<BgCharacterUpload> uploads)
        {
            if (WordContent is not null && WordArtwork != artwork)
                throw new InvalidDataException("Two rooms of one tileset write words over different artwork.");
            Writers.UnionWith(writers);
            if (WordContent is null)
            {
                WordContent = content;
                WordArtwork = artwork;
                WordUploads = uploads;
            }
        }

        /// <summary>Whether the tileset's rooms are Ceres rooms.</summary>
        internal bool Ceres { get; } = ceres;

        /// <summary>The installed artwork the uploads are drawn from.</summary>
        private RoomBlockSheetInputs Inputs { get; } = inputs;

        /// <summary>What the rooms show over their tile characters after loading, collectibles aside: Kraid's backdrop and animations' frames.</summary>
        internal IReadOnlyList<AppearanceOption> Options => options;

        /// <summary>The artwork <see cref="Options"/> shows, each once.</summary>
        internal IEnumerable<BgCharacterUpload> Uploads => options.Select(option => option.Upload);

        /// <summary>The collectibles in each slot.</summary>
        internal IReadOnlyList<CollectibleSlotCharacters> Collectibles => collectibles;

        /// <summary>Everything the rooms show over their tile characters after loading, collectibles among it.</summary>
        internal IReadOnlyList<AppearanceOption> AllOptions => [.. options, .. collectibles.Select(slot => slot.Option)];

        /// <summary>Adds an option unless the same value of the same input already shows the same artwork at the same place.</summary>
        internal void Add(AppearanceOption option) => AddOnce(options, option);

        /// <summary>Adds <paramref name="option"/> to <paramref name="known"/> unless the same value of the same input already shows there.</summary>
        private static void AddOnce(List<AppearanceOption> known, AppearanceOption option)
        {
            if (!known.Any(other => other.Input == option.Input && other.Value == option.Value && other.Upload.Identity == option.Upload.Identity
                && other.Upload.Set == option.Upload.Set && other.Upload.FirstCharacter == option.Upload.FirstCharacter))
                known.Add(option);
        }

        /// <summary>Adds the collectibles in each slot.</summary>
        internal void AddCollectibles(IEnumerable<CollectibleSlotCharacters> slots) => collectibles.AddRange(slots);

        /// <summary>Adds a frame an animation of the tileset's rooms paints over the BG1 and BG2 characters (#1406).</summary>
        internal void AddAnimated(AnimatedCharacterPlacement frame) => Add(Ensure.NotNull(frame).Option);
    }

    /// <summary>The BG2 pages a library background leaves, under its installed name, and what then shows at each character.</summary>
    /// <param name="Name">The background's installed name.</param>
    /// <param name="Words">The pages' tilemap words.</param>
    /// <param name="Content">The bindings the room's load leaves.</param>
    internal sealed record RoomBackgroundMap(string Name, ushort[] Words, BgArtworkBindings Content);

    /// <summary>A collectible shown in one slot and the blocks it shows there.</summary>
    /// <param name="Option">The collectible in the slot, as the value of the slot's input, and its graphics.</param>
    /// <param name="FirstBlock">The first of the slot's blocks.</param>
    /// <param name="Tiles">The tiles of the slot's blocks, four per block.</param>
    internal sealed record CollectibleSlotCharacters(AppearanceOption Option, int FirstBlock, BackgroundTile[] Tiles);

    /// <summary>What an OBJ character table holds at one place: the artwork and the character it starts at.</summary>
    /// <param name="Identity">The artwork.</param>
    /// <param name="FirstCharacter">The character it starts at.</param>
    private readonly record struct PagePlacement(BgArtworkIdentity Identity, int FirstCharacter)
    {
        /// <summary>Where <paramref name="placement"/> places its artwork.</summary>
        internal static PagePlacement Of(BgCharacterUpload placement) => new(placement.Identity, placement.FirstCharacter);
    }
}
