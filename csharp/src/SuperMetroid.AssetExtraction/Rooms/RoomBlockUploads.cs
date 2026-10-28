using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What each tileset's rooms bind over the background characters after loading (#1392), so its block sheet pictures
/// every block over each artwork that can show under it: the animated tiles their room effects run, the Tourian statues'
/// frames, the collectibles' graphics in each slot with the block definitions they write, the characters their library
/// backgrounds transfer, and Kraid's room backdrop.
/// </summary>
internal static class RoomBlockUploads
{
    /// <summary>The tilesets some retail room state loads, each once in room order: those a block sheet is composed for.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets() =>
        [.. RoomHeaderDefinitions.All.SelectMany(RoomStateSelectionDefinitions.GetStates)
            .Select(state => RoomTilesetDefinitions.Get(state.GraphicsSet)).Distinct()];

    /// <summary>The rooms of each tileset and what they bind, in room order.</summary>
    /// <exception cref="InvalidDataException">
    /// A tileset is loaded both inside and outside Ceres, or a library background transfers over the block characters
    /// artwork that is not installed as characters.
    /// </exception>
    internal static IReadOnlyDictionary<TilesetDefinition, TilesetUploads> ByTileset(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var tilesets = new Dictionary<TilesetDefinition, TilesetUploads>();
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            bool ceres = room.AreaIndex == AreaId.Ceres;
            foreach (RoomStateSettings state in RoomStateSelectionDefinitions.GetStates(room))
            {
                TilesetDefinition tileset = RoomTilesetDefinitions.Get(state.GraphicsSet);
                if (!tilesets.TryGetValue(tileset, out TilesetUploads? uploads))
                    tilesets.Add(tileset, uploads = new TilesetUploads(ceres, inputs));
                else if (uploads.Ceres != ceres)
                    throw new InvalidDataException($"Tileset {tileset} is loaded both inside and outside Ceres.");
                foreach (BgCharacterUpload placement in Of(inputs, state).Where(OverTiles))
                    uploads.Add(placement);
                foreach (AnimatedCharacterPlacement frame in AnimatedFrames(inputs, room, state).Where(frame => frame.Set == BgCharacterSet.Tiles))
                    uploads.AddAnimated(frame);
                if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Crocomire))
                {
                    foreach (AnimatedCharacterPlacement frame in CrocomireMeltBandFrames(inputs))
                        uploads.AddAnimated(frame);
                }
                foreach (BgCharacterUpload placement in LibraryPlacements(inputs, state))
                    uploads.Add(placement);
                RoomBackgroundCharacters characters = RoomScope.CharactersOf(new SelectedRoom(room, state));
                BgCharacterUpload[] loadedObjects = [], encounterObjects = [];
                if (characters == RoomBackgroundCharacters.ObjectCharacterPage)
                {
                    loadedObjects = [.. ObjectPageUploads(inputs, state)];
                    encounterObjects = [.. EncounterObjectUploads(inputs, state)];
                    uploads.AddObjectPage(loadedObjects, encounterObjects);
                }
                if (state.Background is { } program)
                {
                    foreach (DoorDefinition? door in DoorVariants(program))
                        uploads.AddBackground(Replay(inputs, room, tileset, program, door));
                }
                // BG3 shows the HUD over what the room's load and its library background bind, from the copy of the HUD's
                // characters Kraid's library background makes where it makes one, and its effects' pages over their characters.
                RoomBackgroundMap? replayed = state.Background is { } bg3Program ? Replay(inputs, room, tileset, bg3Program, DoorVariants(bg3Program).First()) : null;
                uploads.AddBg3(replayed?.Content ?? Loaded(inputs, tileset),
                    replayed?.SelectsKraidHudCharacters == true ? BgCharacterSet.KraidLayer3 : BgCharacterSet.Layer3,
                    EffectsIn(state), [.. Bg3LibraryUploads(inputs, state), .. Bg3BossUploads(inputs, state)]);
                TileWordWriter[] writers = [.. WritersIn(state)];
                if (writers.Length > 0 && characters == RoomBackgroundCharacters.ObjectCharacterPage)
                {
                    // Words written over a room whose backgrounds read the OBJ character table show over what its load and
                    // its encounters upload there.
                    BgCharacterUpload[] written = [.. loadedObjects, .. encounterObjects];
                    var content = new BgArtworkBindings();
                    foreach (BgCharacterUpload placement in written)
                        placement.BindTo(content);
                    uploads.AddWriters(writers, content, characters, written);
                }
                else if (writers.Length > 0)
                {
                    // Words written over the room show over what its load and its library background bind.
                    uploads.AddWriters(writers, state.Background is { } wordsProgram
                        ? Replay(inputs, room, tileset, wordsProgram, DoorVariants(wordsProgram).First()).Content : Loaded(inputs, tileset),
                        characters, []);
                }
                if (ScrollingSkyState.IsScrollingSkyRoomMain(state.MainCallback))
                {
                    // The sky's rows show over what the room's load and its library background bind.
                    ScrollingSkyChunkTable variant = ScrollingSkyState.VariantOf(state.MainCallback);
                    BgArtworkBindings content = state.Background is { } skyProgram
                        ? Replay(inputs, room, tileset, skyProgram, null).Content : Loaded(inputs, tileset);
                    uploads.AddBackground(new RoomBackgroundMap(RoomBackgroundImage.SkyNameOf(variant, tileset),
                        BackgroundTileWords.ToWords(inputs.Skies.Tilemap(variant).Tiles), content));
                }
            }
        }
        foreach ((TilesetDefinition _, TilesetUploads uploads) in tilesets.Where(entry => !entry.Value.Ceres))
            uploads.AddCollectibles(Collectibles(inputs));
        return tilesets;
    }

    /// <summary>What one state of a room binds after it loads, collectibles and animations aside: Kraid's backdrop.</summary>
    private static IEnumerable<BgCharacterUpload> Of(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
        {
            yield return new BgCharacterUpload(KraidBehavior.KraidRoomBackgroundIdentity, BgCharacterSet.Tiles, KraidBackgroundDefinitions.RoomBackgroundFirstCharacter,
                (inputs.Bosses.KraidBackground ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters."))
                    .RoomBackgroundTiles.Characters);
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
                foreach (AnimatedCharacterPlacement frame in AnimatedTiles(inputs, AreaAnimatedTileObjectDefinitions.Read(room.AreaIndex, bit)))
                    yield return frame;
            }
        }
        if (state.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations)
        {
            foreach (TourianStatueAnimatedTileProgramDefinition statue in TourianStatueAnimatedTileMechanicsDefinitions.All)
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

    /// <summary>
    /// The characters one state's library background binds as its room loads, from the transfers the cartridge copies among
    /// the block characters: the HUD's, which Kraid's list copies for layer 3 to read while his body covers the standard
    /// ones.
    /// </summary>
    /// <exception cref="InvalidDataException">It copies other artwork among the block characters.</exception>
    private static IEnumerable<BgCharacterUpload> LibraryPlacements(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        foreach (LibraryBackgroundInstruction transfer in state.Background?.Instructions ?? [])
        {
            if (!transfer.Transfers || !BgCharacterWords.AmongTiles(LibraryBackgroundOperands.DestinationWord(transfer.Destination)))
                continue;
            if (transfer.Command != LibraryBackgroundCommand.TransferForKraid || transfer.Source != LibraryBackgroundSource.HudCharacters
                || transfer.ByteCount != HudTileAtlasFormat.CharacterByteCount)
                throw new InvalidDataException(
                    $"{state.Name}'s library background transfers {transfer.Source} over the block characters, which no installed character artwork names.");
            yield return new BgCharacterUpload(RoomBgArtwork.Library(transfer.Source), BgCharacterSet.KraidLayer3, 0,
                inputs.Hud.Bytes[..HudTileAtlasFormat.CharacterByteCount]);
        }
    }

    /// <summary>The doors <paramref name="program"/> transfers for, each a variant of its background, and no door.</summary>
    private static IEnumerable<DoorDefinition?> DoorVariants(LibraryBackgroundProgram program) =>
        [null, .. program.Instructions.Where(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor)
            .Select(instruction => instruction.Door).Distinct()];

    /// <summary>
    /// Replays <paramref name="program"/> as a room of <paramref name="tileset"/> entered through <paramref name="door"/>
    /// runs it, over the characters the room loads: the BG2 pages it leaves and what shows at each character then.
    /// </summary>
    private static RoomBackgroundMap Replay(RoomBlockSheetInputs inputs, RoomHeaderDefinition room, TilesetDefinition tileset,
        LibraryBackgroundProgram program, DoorDefinition? door)
    {
        BgArtworkBindings content = Loaded(inputs, tileset);
        var vram = new SnesVram();
        // A door no transfer names stands for every door the program does not tell apart.
        DoorDefinition entry = door ?? new DoorDefinition("no library background door", room,
            new DoorOrientation(DoorDirection.Right, DoorClosingBehavior.None), default, default, DoorSamusSpeed.None, DoorSetupCode.None);
        LibraryBackgroundExecutionResult result = LibraryBackgroundReplay.Execute(vram, program,
            entry, new LibraryBackgroundArtwork
            {
                TilemapArt = inputs.BackgroundTilemaps,
                SkyArt = inputs.Skies,
                HudArt = inputs.Hud,
                CharacterArt = inputs.Characters,
                Backgrounds = content,
            });
        ushort[] words = [.. Enumerable.Range(0, RoomBackgroundImage.Words).Select(word => vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word))];
        return new RoomBackgroundMap(RoomBackgroundImage.NameOf(program, door, tileset), words, content, result.SelectsKraidHudCharacters);
    }

    /// <summary>What each enemy writes over its room's BG1 or BG2: a boss's body, and Mother Brain's escape warning.</summary>
    private static readonly Dictionary<EnemyDefinitionId, TileWordWriter[]> EnemyWriters = new()
    {
        [EnemyDefinitionId.Kraid] = [TileWordWriter.Kraid],
        [EnemyDefinitionId.PhantoonBody] = [TileWordWriter.Phantoon],
        [EnemyDefinitionId.DraygonBody] = [TileWordWriter.Draygon],
        [EnemyDefinitionId.Crocomire] = [TileWordWriter.Crocomire],
        [EnemyDefinitionId.MotherBrainBody] = [TileWordWriter.MotherBrain, TileWordWriter.ZebesEscapeText],
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
            yield return new BgCharacterUpload(new BgArtworkIdentity($"{new GraphicsSetTileCharacters(entry.DefinitionId)}"), BgCharacterSet.Tiles,
                EnemyGraphicsSet.EnemyTileAreaFirstCharacter + entry.StagingOffset / SpriteArtworkPlacements.CharacterByteCount, tiles);
        }
    }

    /// <summary>
    /// What <paramref name="state"/>'s encounters upload over the OBJ character table after its room loads, in the order
    /// they make them, each named as the table records it: Ceres Ridley's getaway its escape timer and its warning.
    /// </summary>
    private static IEnumerable<BgCharacterUpload> EncounterObjectUploads(RoomBlockSheetInputs inputs, RoomStateSettings state) =>
        SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog.Of(state.EnemyGraphicsSet).Select(upload => new BgCharacterUpload(
            new BgArtworkIdentity($"{upload.Source}"), BgCharacterSet.Tiles, upload.FirstCharacter,
            SpriteArtworkBytes.Of(upload.Source, inputs.Objects)));

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
        RoomFxAnimatedTileObjectDefinition mechanics = RoomFxAnimatedTileMechanicsDefinitions.Require(animatedObject);
        for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            yield return Animated(inputs, animatedObject, frame, mechanics.Image(mechanics.Frames[frame]),
                mechanics.FrameByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
    }

    /// <summary>
    /// The characters <paramref name="state"/>'s library background transfers over BG3's. A tilemap it stages in work RAM
    /// or a sky's rows lands there as words, not characters, as Kraid's BG2 map does once his library moves the HUD's
    /// characters to his base.
    /// </summary>
    /// <exception cref="InvalidDataException">A transfer over BG3's characters names no installed character artwork.</exception>
    private static IEnumerable<BgCharacterUpload> Bg3LibraryUploads(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        foreach (LibraryBackgroundInstruction transfer in state.Background?.Instructions ?? [])
        {
            if (!transfer.Transfers)
                continue;
            int firstWord = LibraryBackgroundOperands.DestinationWord(transfer.Destination);
            if (firstWord + transfer.ByteCount / sizeof(ushort) <= GameplayVramLayout.HudCharacters.Offset
                || firstWord >= GameplayVramLayout.HudCharacters.Offset + GameplayVramLayout.HudCharacters.Count
                || transfer.Source is LibraryBackgroundSource.StagedTilemap or LibraryBackgroundSource.StagedEnemyTilemap
                    or LibraryBackgroundSource.SkyPage)
                continue;
            ReadOnlyMemory<byte> characters = transfer.Source == LibraryBackgroundSource.HudCharacters
                ? inputs.Hud.Bytes[..transfer.ByteCount]
                : throw new InvalidDataException(
                    $"{state.Name}'s library background transfers {transfer.Source} over BG3's characters, which no installed character artwork names.");
            yield return BgCharacterWords.Place(RoomBgArtwork.Library(transfer.Source), firstWord, characters);
        }
    }

    /// <summary>What <paramref name="state"/>'s bosses bind over BG3's characters: the HUD's, which Kraid's death restores.</summary>
    private static IEnumerable<BgCharacterUpload> Bg3BossUploads(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        if (!state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
            yield break;
        for (int quarter = 0; quarter < HudTileAtlasFormat.RestoreQuarterCount; quarter++)
            yield return BgCharacterUpload.Of(KraidHudRestoration.Placement(quarter), inputs.Hud.KraidRestoreQuarter(quarter));
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
    /// The common room elements' and the tileset's characters a room of <paramref name="tileset"/> binds as it loads, in
    /// the order Core binds them, with the bytes the installed artwork holds for each.
    /// </summary>
    internal static IReadOnlyList<BgCharacterUpload> LoadUploads(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        ReadOnlyMemory<byte> cre = inputs.Characters.Cre.Characters;
        ReadOnlyMemory<byte> characters = inputs.Characters.Get(tileset.Characters).Characters;
        return [.. LoadedRoomAssets.CharacterPlacements(tileset, BgCharacterSet.Tiles.CharactersIn(cre.Length), BgCharacterSet.Tiles.CharactersIn(characters.Length))
            .Select(placement => BgCharacterUpload.Of(placement, placement.Identity == RoomBgArtwork.CommonRoomElements ? cre : characters))];
    }

    /// <summary>Each frame an animated-tile object the port runs shows; none for an object it does not animate.</summary>
    private static IEnumerable<AnimatedCharacterPlacement> AnimatedTiles(RoomBlockSheetInputs inputs, AnimatedTileObject animatedObject)
    {
        if (WreckedShipTreadmillMechanicsDefinitions.TryResolve(animatedObject, out WreckedShipTreadmillObjectDefinition? treadmill))
        {
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return Animated(inputs, animatedObject, frame, treadmill.Image(frame), WreckedShipTreadmillMechanicsDefinitions.FrameByteCount,
                    AnimatedTileVramDestinations.WordOf(animatedObject));
            yield break;
        }
        if (animatedObject is not (AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling))
            yield break;
        RoomFxAnimatedTileObjectDefinition mechanics = RoomFxAnimatedTileMechanicsDefinitions.Require(animatedObject);
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

    /// <summary>Each collectible's graphics in each slot, with the block definitions it writes there.</summary>
    private static IEnumerable<CollectibleSlotCharacters> Collectibles(RoomBlockSheetInputs inputs)
    {
        foreach (InWorldCollectibleKind kind in RoomPlmDynamicCollectibleGraphicsDefinitions.Kinds)
        {
            RoomPlmDynamicCollectibleGraphic graphic = inputs.Collectibles.Resolve(kind);
            for (int slot = 0; slot < CollectiblePlms.GraphicsSlots; slot++)
                yield return new CollectibleSlotCharacters(
                    BgCharacterUpload.Of(CollectiblePlms.SlotCharacters(RoomBgArtwork.Collectible(graphic.Kind), slot,
                        BgCharacterSet.Tiles.CharactersIn(graphic.Tiles.Length)), graphic.Tiles),
                    CollectiblePlms.SlotDefinitions(slot, graphic.PaletteOffsets.Span));
        }
    }

    /// <summary>Whether <paramref name="placement"/> binds among the characters a block's tile words name.</summary>
    private static bool OverTiles(BgCharacterUpload placement) => placement.Set == BgCharacterSet.Tiles;

    /// <summary>What one tileset's rooms bind.</summary>
    /// <param name="ceres">Whether the tileset's rooms are Ceres rooms, which load no common room elements.</param>
    /// <param name="inputs">The installed artwork the uploads are drawn from.</param>
    internal sealed class TilesetUploads(bool ceres, RoomBlockSheetInputs inputs)
    {
        /// <summary>The placements by identity and place, each once.</summary>
        private readonly List<BgCharacterUpload> uploads = [];

        /// <summary>The collectibles' placements.</summary>
        private readonly List<CollectibleSlotCharacters> collectibles = [];

        /// <summary>The library backgrounds the tileset's rooms load, by name.</summary>
        private readonly Dictionary<string, RoomBackgroundMap> backgrounds = new(StringComparer.Ordinal);

        /// <summary>The library backgrounds the tileset's rooms load.</summary>
        internal IEnumerable<RoomBackgroundMap> Backgrounds => backgrounds.Values;

        /// <summary>Adds a library background unless one of its name is known.</summary>
        internal void AddBackground(RoomBackgroundMap background) => backgrounds.TryAdd(background.Name, background);

        /// <summary>What writes tile words over the tileset's rooms.</summary>
        internal HashSet<TileWordWriter> Writers { get; } = [];

        /// <summary>
        /// What a room whose blocks read the OBJ character table shows there: the uploads its load makes, and those with its
        /// encounters' uploads after, when it makes any; none when none of the tileset's rooms read it.
        /// </summary>
        internal IReadOnlyList<IReadOnlyList<BgCharacterUpload>> ObjectPages { get; private set; } = [];

        /// <summary>
        /// Records what a room whose blocks read the OBJ character table uploads there: <paramref name="loaded"/> as it
        /// loads, then <paramref name="encounters"/>.
        /// </summary>
        /// <exception cref="InvalidDataException">Another such room of the tileset uploads otherwise.</exception>
        internal void AddObjectPage(IReadOnlyList<BgCharacterUpload> loaded, IReadOnlyList<BgCharacterUpload> encounters)
        {
            IReadOnlyList<BgCharacterUpload>[] pages = encounters.Count == 0 ? [loaded] : [loaded, [.. loaded, .. encounters]];
            if (ObjectPages.Count > 0 && !ObjectPages.SelectMany(page => page).Select(PagePlacement.Of)
                .SequenceEqual(pages.SelectMany(page => page).Select(PagePlacement.Of)))
                throw new InvalidDataException("Two rooms of one tileset read different OBJ character tables.");
            if (ObjectPages.Count == 0)
                ObjectPages = pages;
        }

        /// <summary>What the first room shows BG3's words over, or null before a room is recorded.</summary>
        internal BgArtworkBindings? Bg3Content { get; private set; }

        /// <summary>The characters BG3 reads in the first room, where <see cref="Bg3Content"/> is read.</summary>
        internal BgCharacterSet Bg3ContentSet { get; private set; }

        /// <summary>The room effects whose BG3 pages the tileset's rooms show.</summary>
        internal HashSet<RoomFxType> FxTypes { get; } = [];

        /// <summary>
        /// The characters BG3 reads in the tileset's rooms, the standard ones always among them: a room that reads a copy
        /// returns BG3 to them once its boss restores them, as Kraid's death does.
        /// </summary>
        internal HashSet<BgCharacterSet> Bg3CharacterSets { get; } = [BgCharacterSet.Layer3];

        /// <summary>What the tileset's rooms bind over BG3's characters after loading: their effects' frames and library transfers.</summary>
        internal List<BgCharacterUpload> Bg3Uploads { get; } = [];

        /// <summary>
        /// Records a room showing BG3 over <paramref name="content"/>'s <paramref name="set"/>, with
        /// <paramref name="effects"/>' pages and <paramref name="library"/>'s transfers over BG3's characters.
        /// </summary>
        internal void AddBg3(BgArtworkBindings content, BgCharacterSet set, IEnumerable<RoomFxType> effects,
            IEnumerable<BgCharacterUpload> library)
        {
            if (Bg3Content is null)
            {
                Bg3Content = content;
                Bg3ContentSet = set;
            }
            Bg3CharacterSets.Add(set);
            foreach (RoomFxType effect in effects)
            {
                if (!FxTypes.Add(effect))
                    continue;
                foreach (AnimatedCharacterPlacement frame in EffectCharacters(Inputs, effect))
                    AddBg3Animated(frame);
            }
            foreach (BgCharacterUpload placement in library)
                AddOnce(Bg3Uploads, placement);
        }

        /// <summary>What the first room with writers shows their words over, or null without writers.</summary>
        internal BgArtworkBindings? WordContent { get; private set; }

        /// <summary>The characters the writers' words name.</summary>
        internal RoomBackgroundCharacters WordCharacters { get; private set; }

        /// <summary>The placements <see cref="WordContent"/> binds beyond the room's load, whose characters the words show.</summary>
        internal IReadOnlyList<BgCharacterUpload> WordUploads { get; private set; } = [];

        /// <summary>
        /// Records that a room's <paramref name="writers"/> write words naming <paramref name="characters"/> over
        /// <paramref name="content"/>, which binds <paramref name="uploads"/> beyond the room's load.
        /// </summary>
        /// <exception cref="InvalidDataException">Another room of the tileset writes words naming other characters.</exception>
        internal void AddWriters(IEnumerable<TileWordWriter> writers, BgArtworkBindings content, RoomBackgroundCharacters characters,
            IReadOnlyList<BgCharacterUpload> uploads)
        {
            if (WordContent is not null && WordCharacters != characters)
                throw new InvalidDataException("Two rooms of one tileset write words naming different characters.");
            Writers.UnionWith(writers);
            if (WordContent is null)
            {
                WordContent = content;
                WordCharacters = characters;
                WordUploads = uploads;
            }
        }

        /// <summary>Whether the tileset's rooms are Ceres rooms.</summary>
        internal bool Ceres { get; } = ceres;

        /// <summary>The installed artwork the uploads are drawn from.</summary>
        private RoomBlockSheetInputs Inputs { get; } = inputs;

        /// <summary>The placements, each once.</summary>
        internal IReadOnlyList<BgCharacterUpload> Uploads => uploads;

        /// <summary>The collectibles' placements.</summary>
        internal IReadOnlyList<CollectibleSlotCharacters> Collectibles => collectibles;

        /// <summary>Adds a placement unless the same artwork is already bound at the same place.</summary>
        internal void Add(BgCharacterUpload placement) => AddOnce(uploads, placement);

        /// <summary>Adds <paramref name="placement"/> to <paramref name="placements"/> unless the same artwork is already there at the same place.</summary>
        private static void AddOnce(List<BgCharacterUpload> placements, BgCharacterUpload placement)
        {
            if (!placements.Any(known => known.Identity == placement.Identity && known.Set == placement.Set && known.FirstCharacter == placement.FirstCharacter))
                placements.Add(placement);
        }

        /// <summary>Adds the collectibles' placements.</summary>
        internal void AddCollectibles(IEnumerable<CollectibleSlotCharacters> slots) => collectibles.AddRange(slots);

        /// <summary>The runs of the tileset's BG1 and BG2 characters its rooms' animations paint (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> BlockRuns { get; } = [];

        /// <summary>The runs of the OBJ character page its rooms' animations paint, for rooms whose blocks read it (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> ObjectPageRuns { get; } = [];

        /// <summary>The runs of BG3's characters its rooms' animations paint, from the HUD's character base (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> Bg3Runs { get; } = [];

        /// <summary>
        /// Adds a frame an animation of the tileset's rooms paints over BG3's characters: its BG3 words are pictured over it,
        /// and the animation is declared to paint its characters (#1406).
        /// </summary>
        private void AddBg3Animated(AnimatedCharacterPlacement frame)
        {
            AddOnce(Bg3Uploads, Ensure.NotNull(frame).Placement);
            Bg3Runs.Add(frame.Run);
        }

        /// <summary>
        /// Adds a frame an animation of the tileset's rooms paints over the BG1 and BG2 characters: its blocks and words are
        /// pictured over it, and the animation is declared to paint its characters (#1406).
        /// </summary>
        internal void AddAnimated(AnimatedCharacterPlacement frame)
        {
            Add(Ensure.NotNull(frame).Placement);
            BlockRuns.Add(frame.Run);
        }
    }

    /// <summary>The BG2 pages a library background leaves, under its installed name, and what then shows at each character.</summary>
    /// <param name="Name">The background's installed name.</param>
    /// <param name="Words">The pages' tilemap words.</param>
    /// <param name="Content">The bindings the room's load and the background leave.</param>
    /// <param name="SelectsKraidHudCharacters">Whether the background moved the HUD's characters to Kraid's base.</param>
    internal sealed record RoomBackgroundMap(string Name, ushort[] Words, BgArtworkBindings Content, bool SelectsKraidHudCharacters = false);

    /// <summary>A collectible's graphics bound in one slot and the block definitions it writes there.</summary>
    /// <param name="Characters">The graphics bound.</param>
    /// <param name="Tiles">The tiles written, four per definition.</param>
    internal sealed record CollectibleSlotCharacters(BgCharacterUpload Characters, BackgroundTile[] Tiles);

    /// <summary>What an OBJ character table holds at one place: the artwork and the character it starts at.</summary>
    /// <param name="Identity">The artwork.</param>
    /// <param name="FirstCharacter">The character it starts at.</param>
    private readonly record struct PagePlacement(BgArtworkIdentity Identity, int FirstCharacter)
    {
        /// <summary>Where <paramref name="placement"/> places its artwork.</summary>
        internal static PagePlacement Of(BgCharacterUpload placement) => new(placement.Identity, placement.FirstCharacter);
    }
}
