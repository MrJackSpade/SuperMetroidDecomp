using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.World.Artwork;
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
    /// <summary>The VRAM words the BG1 and BG2 characters a block's tile words name occupy.</summary>
    private const int BlockCharacterWords = 1024 * RoomBlockPictureNames.CharacterWords;

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
            foreach (CartridgeRoomState state in RoomStateSelectionDefinitions.GetStates(room))
            {
                TilesetDefinition tileset = RoomTilesetDefinitions.Get(state.GraphicsSet);
                if (!tilesets.TryGetValue(tileset, out TilesetUploads? uploads))
                    tilesets.Add(tileset, uploads = new TilesetUploads(ceres, inputs));
                else if (uploads.Ceres != ceres)
                    throw new InvalidDataException($"Tileset {tileset} is loaded both inside and outside Ceres.");
                foreach (BgCharacterUpload upload in Of(inputs, room, state).Where(Overlaps))
                    uploads.Add(upload);
                if (state.EnemyPopulation.Records.Span.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Crocomire))
                {
                    foreach (AnimatedCharacterUpload frame in CrocomireMeltBandFrames(inputs))
                        uploads.AddAnimated(frame);
                }
                foreach (BgCharacterUpload upload in LibraryUploads(inputs, state))
                    uploads.AddLibrary(upload);
                RoomBackgroundCharacters characters = RoomScope.CharactersOf(new CartridgeRoomHeader(room, state));
                BgCharacterUpload[] loadedObjects = [], encounterObjects = [];
                if (characters == RoomBackgroundCharacters.ObjectCharacterPage)
                {
                    loadedObjects = [.. ObjectPageUploads(inputs, state)];
                    encounterObjects = [.. EncounterObjectUploads(inputs, state)];
                    uploads.AddObjectPage(loadedObjects, encounterObjects);
                }
                if (state.Background is { } program)
                {
                    foreach (CartridgeDoorHeader? door in DoorVariants(program))
                        uploads.AddBackground(Replay(inputs, room, tileset, program, door));
                }
                // BG3 shows the HUD over what the room's load and its library background bind, with the HUD's characters
                // at Kraid's base where his library background moves them, and its effects' pages over their characters.
                RoomBackgroundMap? replayed = state.Background is { } bg3Program ? Replay(inputs, room, tileset, bg3Program, DoorVariants(bg3Program).First()) : null;
                uploads.AddBg3(replayed?.Content ?? Loaded(inputs, tileset),
                    replayed?.SelectsKraidHudCharacters == true ? RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord : GameplayVramLayout.HudCharacterWord,
                    EffectsIn(state), [.. Bg3LibraryUploads(inputs, state), .. Bg3BossUploads(inputs, state)]);
                TileWordWriter[] writers = [.. WritersIn(state)];
                if (writers.Length > 0 && characters == RoomBackgroundCharacters.ObjectCharacterPage)
                {
                    // Words written over a room whose backgrounds read the OBJ character table show over what its load and
                    // its encounters upload there.
                    BgCharacterUpload[] written = [.. loadedObjects, .. encounterObjects];
                    var content = new BgArtworkBindings(new SnesVram());
                    foreach (BgCharacterUpload upload in written)
                        upload.BindTo(content);
                    uploads.AddWriters(writers, content, RoomCharacterSource.CharacterBaseOf(characters), written);
                }
                else if (writers.Length > 0)
                {
                    // Words written over the room show over what its load and its library background bind.
                    uploads.AddWriters(writers, state.Background is { } wordsProgram
                        ? Replay(inputs, room, tileset, wordsProgram, DoorVariants(wordsProgram).First()).Content : Loaded(inputs, tileset),
                        RoomCharacterSource.CharacterBaseOf(characters), []);
                }
                if (ScrollingSkyState.IsScrollingSkyRoomMain(state.MainCallback))
                {
                    // The sky's rows show over what the room's load and its library background bind.
                    ScrollingSkyChunkTable variant = ScrollingSkyState.VariantOf(state.MainCallback);
                    BgArtworkBindings content = state.Background is { } skyProgram
                        ? Replay(inputs, room, tileset, skyProgram, null).Content : Loaded(inputs, tileset);
                    uploads.AddBackground(new RoomBackgroundMap(RoomBackgroundImage.SkyNameOf(variant, tileset),
                        [.. inputs.Skies.Tilemap(variant).Words], content));
                }
            }
        }
        foreach ((TilesetDefinition _, TilesetUploads uploads) in tilesets.Where(entry => !entry.Value.Ceres))
            uploads.AddCollectibles(Collectibles(inputs));
        return tilesets;
    }

    /// <summary>What one state of a room binds after it loads, collectibles aside.</summary>
    [PpuLayoutBoundary("codec: the VRAM words a room's animated tiles, library backgrounds and Kraid's backdrop bind at")]
    private static IEnumerable<BgCharacterUpload> Of(RoomBlockSheetInputs inputs, RoomHeaderDefinition room, CartridgeRoomState state)
    {
        foreach (RoomFxRecordDefinition record in state.Fx?.Records ?? [])
        {
            for (int bit = 0; bit < AreaAnimatedTileObjectDefinitions.ObjectsPerArea; bit++)
            {
                if ((record.AnimatedTileBitset & (1 << bit)) == 0)
                    continue;
                foreach (BgCharacterUpload upload in AnimatedTiles(inputs, AreaAnimatedTileObjectDefinitions.Read(room.AreaIndex, bit)))
                    yield return upload;
            }
        }
        if (state.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations)
        {
            foreach (TourianStatueAnimatedTileProgramDefinition statue in TourianStatueAnimatedTileMechanicsDefinitions.All)
            {
                foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
                {
                    var source = new TourianStatuePictureCharacters(statue.AnimatedObject, picture);
                    yield return new BgCharacterUpload(new BgArtworkIdentity(source.ToString()), statue.Destination.Word,
                        Frame(inputs, TourianStatueTileStripDefinitions.SourceAddress(statue.AnimatedObject, picture), statue.TransferByteCount));
                }
            }
        }
        if (state.EnemyPopulation.Records.Span.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
        {
            yield return new BgCharacterUpload(KraidBehavior.KraidRoomBackgroundIdentity, BossVramLayout.KraidRoomBackgroundTiles.Offset,
                (inputs.Bosses.KraidBackground ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters."))
                    .RoomBackgroundTiles.Characters);
        }
    }

    /// <summary>
    /// What Crocomire's melt shows over his characters (#1392, #1406): each band of each melt image after every number of
    /// columns erased, as the frame of the band's animation the runtime selects for it.
    /// </summary>
    private static IEnumerable<AnimatedCharacterUpload> CrocomireMeltBandFrames(RoomBlockSheetInputs inputs)
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
                    yield return new AnimatedCharacterUpload(CrocomireMelting.MeltBandFrame(new CrocomireMeltBandPicture(illustration, step), band),
                        CrocomireMeltDissolve.BandFirstWord(band), CrocomireMeltDissolve.BandCharacters(image, band));
                }
                step++;
            }
        }
    }

    /// <summary>The characters one state's library background binds over the block characters as its room loads.</summary>
    /// <exception cref="InvalidDataException">It transfers artwork that is not installed as characters over them.</exception>
    private static IEnumerable<BgCharacterUpload> LibraryUploads(RoomBlockSheetInputs inputs, CartridgeRoomState state)
    {
        foreach (LibraryBackgroundInstruction transfer in state.Background?.Instructions ?? [])
        {
            if (transfer.Command is not (LibraryBackgroundCommand.TransferToVram or LibraryBackgroundCommand.TransferToVramForKraid
                or LibraryBackgroundCommand.TransferForDoor) || transfer.Destination >= BlockCharacterWords)
                continue;
            // The library binds the Tourian statue ghost's characters and the HUD's (Kraid's room moves them under BG1).
            ReadOnlyMemory<byte> characters = transfer.SourceAddress == RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress
                ? inputs.Characters.Get(RoomCharacterSet.TourianStatueGhost).Characters
                : transfer.SourceAddress == HudTileAtlasFormat.SourceAddress && transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount
                    ? inputs.Hud.Bytes[..HudTileAtlasFormat.CharacterByteCount]
                    : throw new InvalidDataException(
                        $"{state.Name}'s library background transfers ${transfer.SourceAddress:X6} over the block characters, which no installed character artwork names.");
            yield return new BgCharacterUpload(RoomBgArtwork.Library(transfer.SourceAddress), transfer.Destination, characters);
        }
    }

    /// <summary>The doors <paramref name="program"/> transfers for, each a variant of its background, and no door.</summary>
    private static IEnumerable<CartridgeDoorHeader?> DoorVariants(LibraryBackgroundProgram program) =>
        [null, .. program.Instructions.Where(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor)
            .Select(instruction => instruction.Door).Distinct()];

    /// <summary>
    /// Replays <paramref name="program"/> as a room of <paramref name="tileset"/> entered through <paramref name="door"/>
    /// runs it, over the characters the room loads: the BG2 pages it leaves and what shows at each character then.
    /// </summary>
    [PpuLayoutBoundary("codec: the BG2 tilemap pages a library background loads")]
    private static RoomBackgroundMap Replay(RoomBlockSheetInputs inputs, RoomHeaderDefinition room, TilesetDefinition tileset,
        LibraryBackgroundProgram program, CartridgeDoorHeader? door)
    {
        BgArtworkBindings content = Loaded(inputs, tileset);
        SnesVram vram = content.Vram;
        // A door no transfer names stands for every door the program does not tell apart.
        CartridgeDoorHeader entry = door ?? new CartridgeDoorHeader("no library background door", room,
            new CartridgeDoorOrientation(DoorDirection.Right, DoorClosingBehavior.None), default, default, 0, DoorSetupCode.None);
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
    private static IEnumerable<TileWordWriter> WritersIn(CartridgeRoomState state) =>
        state.EnemyPopulation.Records.ToArray().SelectMany(record => EnemyWriters.TryGetValue(record.DefinitionId, out TileWordWriter[]? writers)
            ? writers : []).Distinct();

    /// <summary>
    /// What <paramref name="state"/>'s room uploads over the OBJ character table as it loads, which the Ceres Ridley arena's
    /// blocks show: each enemy's tiles its graphics set loads, named as the table records them.
    /// </summary>
    /// <exception cref="InvalidDataException">An enemy's tiles are not installed.</exception>
    [PpuLayoutBoundary("codec: the VRAM words a room's enemy graphics set uploads its tiles to")]
    private static IEnumerable<BgCharacterUpload> ObjectPageUploads(RoomBlockSheetInputs inputs, CartridgeRoomState state)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in EnemyGraphicsSet.LayOut(CompiledRoomEnemyDefinitions.Instance, state.EnemyGraphicsSet, new SnesVram()))
        {
            int source = entry.Definition.TileDataAddress;
            ReadOnlyMemory<byte> tiles = inputs.Objects.Enemies.TryResolve(source, entry.TileByteCount, out ReadOnlyMemory<byte> installed) ? installed
                : throw new InvalidDataException($"{state.Name}'s enemy tiles from ${source:X6} are not installed.");
            yield return new BgCharacterUpload(new BgArtworkIdentity($"{new TransferObjCharacters(source, entry.TileByteCount)}"),
                EnemyVram.EnemyTilesDma(entry.StagingOffset / 2).Word, tiles);
        }
    }

    /// <summary>
    /// What <paramref name="state"/>'s encounters upload over the OBJ character table after its room loads, in the order
    /// they make them, each named as the table records it: Ceres Ridley's getaway its escape timer and its warning.
    /// </summary>
    [PpuLayoutBoundary("codec: the VRAM words an encounter's OBJ character table uploads land on")]
    private static IEnumerable<BgCharacterUpload> EncounterObjectUploads(RoomBlockSheetInputs inputs, CartridgeRoomState state) =>
        RuntimeCharacterUploadDefinitions.Catalog.Of(state.EnemyGraphicsSet).Select(upload => new BgCharacterUpload(
            new BgArtworkIdentity($"{upload.Source}"), GameplayVramLayout.ObjectCharacters.Offset + upload.FirstByte / sizeof(ushort),
            upload.Source.Resolve(inputs.Objects)));

    /// <summary>The HUD's characters as gameplay setup uploads them, before any room loads.</summary>
    internal static BgCharacterUpload HudCharacters(RoomBlockSheetInputs inputs) =>
        new(BgArtworkIdentity.Asset(VramAssetId.StandardHudTiles), GameplayVramLayout.HudCharacterUpload.Offset, Ensure.NotNull(inputs).Hud.Bytes);

    /// <summary>The room effects whose BG3 pages <paramref name="state"/>'s rooms show, and the characters each animates on BG3.</summary>
    private static IEnumerable<RoomFxType> EffectsIn(CartridgeRoomState state) =>
        (state.Fx?.Records ?? []).Select(record => RoomFxTypes.FromCartridge(record.Type, $"room FX {state.Fx?.Name}"))
            .Where(RoomLayer3FxState.ShowsPage).Distinct();

    /// <summary>Each frame a room effect of <paramref name="type"/> animates over BG3's characters.</summary>
    internal static IEnumerable<BgCharacterUpload> EffectCharacters(RoomBlockSheetInputs inputs, RoomFxType type)
    {
        AnimatedTileObject animatedObject = RoomFxAnimatedTilesState.ObjectOf(type);
        if (animatedObject == AnimatedTileObject.None)
            yield break;
        RoomFxAnimatedTileObjectDefinition mechanics = RoomFxAnimatedTileMechanicsDefinitions.Require(animatedObject);
        foreach (RoomFxAnimatedTileFrameDefinition frame in mechanics.Frames)
            yield return Transfer(inputs, RoomFxAnimatedTileArtworkDefinitions.SourceAddress(mechanics, frame), mechanics.TransferByteCount,
                VramDmaDestination.FromNative(mechanics.EncodedVramDestination).Word);
    }

    /// <summary>
    /// The characters <paramref name="state"/>'s library background transfers over BG3's. A tilemap it stages in work RAM
    /// or a sky's rows lands there as words, not characters, as Kraid's BG2 map does once his library moves the HUD's
    /// characters to his base.
    /// </summary>
    /// <exception cref="InvalidDataException">A transfer over BG3's characters names no installed character artwork.</exception>
    [PpuLayoutBoundary("codec: the VRAM words a room's library background transfers BG3 characters to")]
    private static IEnumerable<BgCharacterUpload> Bg3LibraryUploads(RoomBlockSheetInputs inputs, CartridgeRoomState state)
    {
        foreach (LibraryBackgroundInstruction transfer in state.Background?.Instructions ?? [])
        {
            if (transfer.Command is not (LibraryBackgroundCommand.TransferToVram or LibraryBackgroundCommand.TransferToVramForKraid
                or LibraryBackgroundCommand.TransferForDoor)
                || transfer.Destination + transfer.ByteCount / sizeof(ushort) <= GameplayVramLayout.HudCharacters.Offset
                || transfer.Destination >= GameplayVramLayout.HudCharacters.Offset + GameplayVramLayout.HudCharacters.Count
                || SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(transfer.SourceAddress)) == SnesDmaSourceKind.WorkRam
                || inputs.Skies.TryResolve(transfer.SourceAddress, transfer.ByteCount, out _))
                continue;
            ReadOnlyMemory<byte> characters = transfer.SourceAddress == HudTileAtlasFormat.SourceAddress
                ? inputs.Hud.Bytes[..transfer.ByteCount]
                : throw new InvalidDataException(
                    $"{state.Name}'s library background transfers ${transfer.SourceAddress:X6} over BG3's characters, which no installed character artwork names.");
            yield return new BgCharacterUpload(RoomBgArtwork.Library(transfer.SourceAddress), transfer.Destination, characters);
        }
    }

    /// <summary>What <paramref name="state"/>'s bosses bind over BG3's characters: the HUD's, which Kraid's death restores.</summary>
    private static IEnumerable<BgCharacterUpload> Bg3BossUploads(RoomBlockSheetInputs inputs, CartridgeRoomState state)
    {
        if (!state.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
            yield break;
        for (int quarter = 0; quarter < HudTileAtlasFormat.RestoreQuarterCount; quarter++)
            yield return BossVram.KraidBg3Restore(quarter, inputs.Hud.KraidRestoreQuarter(quarter));
    }

    /// <summary>Bindings over a fresh VRAM holding the HUD's characters and what a room of <paramref name="tileset"/> binds as it loads.</summary>
    internal static BgArtworkBindings Loaded(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        var content = new BgArtworkBindings(new SnesVram());
        HudCharacters(inputs).BindTo(content);
        foreach (BgCharacterUpload upload in CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Characters,
            inputs.Characters.Get(tileset.Characters).Characters))
            upload.BindTo(content);
        return content;
    }

    /// <summary>Each frame an animated-tile object the port runs uploads; none for an object it does not animate.</summary>
    private static IEnumerable<BgCharacterUpload> AnimatedTiles(RoomBlockSheetInputs inputs, AnimatedTileObject animatedObject)
    {
        if (WreckedShipTreadmillMechanicsDefinitions.TryResolve(animatedObject, out WreckedShipTreadmillObjectDefinition? treadmill))
        {
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return Transfer(inputs, treadmill.FrameSource(frame), WreckedShipTreadmillRomData.TransferByteCount,
                    VramDmaDestination.FromNative(WreckedShipTreadmillRomData.EncodedVramDestination).Word);
            yield break;
        }
        if (animatedObject is not (AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling))
            yield break;
        RoomFxAnimatedTileObjectDefinition mechanics = RoomFxAnimatedTileMechanicsDefinitions.Require(animatedObject);
        foreach (RoomFxAnimatedTileFrameDefinition frame in mechanics.Frames)
            yield return Transfer(inputs, RoomFxAnimatedTileArtworkDefinitions.SourceAddress(mechanics, frame), mechanics.TransferByteCount,
                VramDmaDestination.FromNative(mechanics.EncodedVramDestination).Word);
    }

    /// <summary>A queued transfer of an animated tile frame, named as the queue names it when it binds.</summary>
    private static BgCharacterUpload Transfer(RoomBlockSheetInputs inputs, int sourceAddress, int byteCount, int destination) =>
        new(BgArtworkIdentity.Transfer(sourceAddress), destination, Frame(inputs, sourceAddress, byteCount));

    /// <summary>The installed bytes of an animated tile frame.</summary>
    private static ReadOnlyMemory<byte> Frame(RoomBlockSheetInputs inputs, int sourceAddress, int byteCount) =>
        inputs.AnimatedTiles.TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> frame) ? frame
            : throw new InvalidDataException($"Installed animated tiles hold no {byteCount}-byte frame from ${sourceAddress:X6}.");

    /// <summary>Each collectible's graphics in each slot, with the block definitions it writes there.</summary>
    private static IEnumerable<CollectibleSlotUpload> Collectibles(RoomBlockSheetInputs inputs)
    {
        for (int kind = RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
             kind < RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind + RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount; kind++)
        {
            RoomPlmDynamicCollectibleGraphic graphic = inputs.Collectibles.Resolve((InWorldCollectibleKind)kind);
            for (int slot = 0; slot < CollectiblePlms.GraphicsSlots; slot++)
                yield return new CollectibleSlotUpload(
                    CollectiblePlms.SlotUpload(RoomBgArtwork.Collectible(graphic.Kind), slot, graphic.Tiles),
                    CollectiblePlms.SlotDefinitions(slot, graphic.PaletteOffsets.Span));
        }
    }

    /// <summary>Whether an upload reaches the characters a block's tile words name.</summary>
    private static bool Overlaps(BgCharacterUpload upload) => upload.FirstWord < BlockCharacterWords;

    /// <summary>What one tileset's rooms bind.</summary>
    /// <param name="ceres">Whether the tileset's rooms are Ceres rooms, which load no common room elements.</param>
    /// <param name="inputs">The installed artwork the uploads are drawn from.</param>
    internal sealed class TilesetUploads(bool ceres, RoomBlockSheetInputs inputs)
    {
        /// <summary>The uploads by identity and destination, each once.</summary>
        private readonly List<BgCharacterUpload> uploads = [];

        /// <summary>The collectibles' uploads.</summary>
        private readonly List<CollectibleSlotUpload> collectibles = [];

        /// <summary>The library backgrounds the tileset's rooms load, by name.</summary>
        private readonly Dictionary<string, RoomBackgroundMap> backgrounds = new(StringComparer.Ordinal);

        /// <summary>The library backgrounds the tileset's rooms load.</summary>
        internal IEnumerable<RoomBackgroundMap> Backgrounds => backgrounds.Values;

        /// <summary>The artwork library backgrounds bind as their rooms load, which their images show.</summary>
        internal HashSet<BgArtworkIdentity> LibraryArtwork { get; } = [];

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
            if (ObjectPages.Count > 0 && !ObjectPages.SelectMany(page => page).Select(upload => (upload.Identity, upload.FirstWord))
                .SequenceEqual(pages.SelectMany(page => page).Select(upload => (upload.Identity, upload.FirstWord))))
                throw new InvalidDataException("Two rooms of one tileset read different OBJ character tables.");
            if (ObjectPages.Count == 0)
                ObjectPages = pages;
        }

        /// <summary>What the first room shows BG3's words over, or null before a room is recorded.</summary>
        internal BgArtworkBindings? Bg3Content { get; private set; }

        /// <summary>The HUD character base BG3 shows in the first room, where <see cref="Bg3Content"/> is read.</summary>
        internal int Bg3ContentBase { get; private set; }

        /// <summary>The room effects whose BG3 pages the tileset's rooms show.</summary>
        internal HashSet<RoomFxType> FxTypes { get; } = [];

        /// <summary>
        /// The HUD character bases BG3 shows in the tileset's rooms, the standard one always among them: a room that moves
        /// the HUD's characters returns BG3 to it once its boss restores them there, as Kraid's death does.
        /// </summary>
        internal HashSet<int> Bg3CharacterBases { get; } = [GameplayVramLayout.HudCharacterWord];

        /// <summary>What the tileset's rooms bind over BG3's characters after loading: their effects' frames and library transfers.</summary>
        internal List<BgCharacterUpload> Bg3Uploads { get; } = [];

        /// <summary>
        /// Records a room showing BG3 over <paramref name="content"/> at <paramref name="characterBase"/>, with
        /// <paramref name="effects"/>' pages and <paramref name="library"/>'s transfers over BG3's characters.
        /// </summary>
        internal void AddBg3(BgArtworkBindings content, int characterBase, IEnumerable<RoomFxType> effects,
            IEnumerable<BgCharacterUpload> library)
        {
            if (Bg3Content is null)
            {
                Bg3Content = content;
                Bg3ContentBase = characterBase;
            }
            Bg3CharacterBases.Add(characterBase);
            foreach (RoomFxType effect in effects)
            {
                if (FxTypes.Add(effect))
                    Bg3Uploads.AddRange(EffectCharacters(Inputs, effect));
            }
            foreach (BgCharacterUpload upload in library)
            {
                if (!Bg3Uploads.Any(known => known.Identity == upload.Identity && known.FirstWord == upload.FirstWord))
                    Bg3Uploads.Add(upload);
            }
        }

        /// <summary>What the first room with writers shows their words over, or null without writers.</summary>
        internal BgArtworkBindings? WordContent { get; private set; }

        /// <summary>The VRAM word of character zero the writers' words name characters from.</summary>
        internal int WordCharacterBase { get; private set; }

        /// <summary>The uploads <see cref="WordContent"/> binds beyond the room's load, whose characters the words show.</summary>
        internal IReadOnlyList<BgCharacterUpload> WordUploads { get; private set; } = [];

        /// <summary>
        /// Records that a room's <paramref name="writers"/> write words naming characters from
        /// <paramref name="characterBase"/> over <paramref name="content"/>, which binds <paramref name="uploads"/> beyond
        /// the room's load.
        /// </summary>
        /// <exception cref="InvalidDataException">Another room of the tileset writes words naming characters from another base.</exception>
        internal void AddWriters(IEnumerable<TileWordWriter> writers, BgArtworkBindings content, int characterBase, IReadOnlyList<BgCharacterUpload> uploads)
        {
            if (WordContent is not null && WordCharacterBase != characterBase)
                throw new InvalidDataException("Two rooms of one tileset write words naming characters from different bases.");
            Writers.UnionWith(writers);
            if (WordContent is null)
            {
                WordContent = content;
                WordCharacterBase = characterBase;
                WordUploads = uploads;
            }
        }

        /// <summary>Whether the tileset's rooms are Ceres rooms.</summary>
        internal bool Ceres { get; } = ceres;

        /// <summary>The installed artwork the uploads are drawn from.</summary>
        private RoomBlockSheetInputs Inputs { get; } = inputs;

        /// <summary>The uploads, each once.</summary>
        internal IReadOnlyList<BgCharacterUpload> Uploads => uploads;

        /// <summary>The collectibles' uploads.</summary>
        internal IReadOnlyList<CollectibleSlotUpload> Collectibles => collectibles;

        /// <summary>Adds an upload a library background binds as its room loads, which its image shows.</summary>
        internal void AddLibrary(BgCharacterUpload upload)
        {
            LibraryArtwork.Add(upload.Identity);
            Add(upload);
        }

        /// <summary>Adds an upload unless the same artwork is already bound at the same word.</summary>
        internal void Add(BgCharacterUpload upload)
        {
            if (!uploads.Any(known => known.Identity == upload.Identity && known.FirstWord == upload.FirstWord))
                uploads.Add(upload);
        }

        /// <summary>Adds the collectibles' uploads.</summary>
        internal void AddCollectibles(IEnumerable<CollectibleSlotUpload> slots) => collectibles.AddRange(slots);

        /// <summary>The runs of the tileset's BG1 and BG2 characters its rooms' animations paint (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> BlockRuns { get; } = [];

        /// <summary>The runs of the OBJ character page its rooms' animations paint, for rooms whose blocks read it (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> ObjectPageRuns { get; } = [];

        /// <summary>The runs of BG3's characters its rooms' animations paint, from the HUD's character base (#1406).</summary>
        internal HashSet<AnimatedCharacterRun> Bg3Runs { get; } = [];

        /// <summary>
        /// Adds a frame an animation of the tileset's rooms paints over the BG1 and BG2 characters: its blocks and words are
        /// pictured over it, and the animation is declared to paint its characters (#1406).
        /// </summary>
        internal void AddAnimated(AnimatedCharacterUpload frame)
        {
            Add(Ensure.NotNull(frame).Upload);
            BlockRuns.Add(frame.RunFrom(RoomGraphicsLayout.AreaCharacters.Offset, RoomBlockPictureNames.CharacterWords));
        }
    }

    /// <summary>The BG2 pages a library background leaves, under its installed name, and what then shows at each character.</summary>
    /// <param name="Name">The background's installed name.</param>
    /// <param name="Words">The pages' tilemap words.</param>
    /// <param name="Content">The bindings the room's load and the background leave.</param>
    /// <param name="SelectsKraidHudCharacters">Whether the background moved the HUD's characters to Kraid's base.</param>
    internal sealed record RoomBackgroundMap(string Name, ushort[] Words, BgArtworkBindings Content, bool SelectsKraidHudCharacters = false);

    /// <summary>A collectible's graphics bound in one slot and the block definitions it writes there.</summary>
    /// <param name="Upload">The graphics bound.</param>
    /// <param name="Words">The tile words written, four per definition.</param>
    internal sealed record CollectibleSlotUpload(BgCharacterUpload Upload, ushort[] Words);
}
