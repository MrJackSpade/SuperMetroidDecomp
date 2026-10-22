using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Artwork;
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
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
                    tilesets.Add(tileset, uploads = new TilesetUploads(ceres));
                else if (uploads.Ceres != ceres)
                    throw new InvalidDataException($"Tileset {tileset} is loaded both inside and outside Ceres.");
                foreach (BgCharacterUpload upload in Of(inputs, room, state).Where(Overlaps))
                    uploads.Add(upload);
                foreach (BgCharacterUpload upload in LibraryUploads(inputs, state))
                    uploads.AddLibrary(upload);
                if (state.Background is { } program)
                {
                    foreach (CartridgeDoorHeader? door in DoorVariants(program))
                        uploads.AddBackground(Replay(inputs, room, tileset, program, door));
                }
                TileWordWriter[] writers = [.. WritersIn(state)];
                if (writers.Length > 0)
                {
                    // Words written over the room show over what its load and its library background bind.
                    uploads.AddWriters(writers, state.Background is { } wordsProgram
                        ? Replay(inputs, room, tileset, wordsProgram, DoorVariants(wordsProgram).First()).Content : Loaded(inputs, tileset));
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
                    .RoomBackgroundTiles.Transfer);
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
                ? inputs.Characters.Get(RoomCharacterSet.TourianStatueGhost).Transfer
                : transfer.SourceAddress == HudTileAtlasFormat.SourceAddress && transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount
                    ? inputs.Hud.Transfer[..HudTileAtlasFormat.CharacterByteCount]
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
        LibraryBackgroundLoader.ExecuteProgram(new LibraryBackgroundStaging(new Bg2TilemapStaging(), new EnemyStagingSurfaces()), vram, program,
            entry, new LibraryBackgroundArtwork
            {
                TilemapArt = inputs.BackgroundTilemaps,
                SkyArt = inputs.Skies,
                HudArt = inputs.Hud,
                CharacterArt = inputs.Characters,
                Backgrounds = content,
            });
        ushort[] words = [.. Enumerable.Range(0, RoomBackgroundImage.Words).Select(word => vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word))];
        return new RoomBackgroundMap(RoomBackgroundImage.NameOf(program, door, tileset), words, content);
    }

    /// <summary>What each enemy writes over its room's BG1 or BG2: a boss's body, and Mother Brain's escape warning.</summary>
    private static readonly Dictionary<EnemyDefinitionId, TileWordWriter[]> EnemyWriters = new()
    {
        [EnemyDefinitionId.Kraid] = [TileWordWriter.Kraid],
        [EnemyDefinitionId.PhantoonBody] = [TileWordWriter.Phantoon],
        [EnemyDefinitionId.DraygonBody] = [TileWordWriter.Draygon],
        [EnemyDefinitionId.Crocomire] = [TileWordWriter.Crocomire],
        [EnemyDefinitionId.MotherBrainBody] = [TileWordWriter.MotherBrain, TileWordWriter.ZebesEscapeText],
    };

    /// <summary>What writes tile words over <paramref name="state"/>'s BG1 or BG2.</summary>
    private static IEnumerable<TileWordWriter> WritersIn(CartridgeRoomState state) =>
        state.EnemyPopulation.Records.ToArray().SelectMany(record => EnemyWriters.TryGetValue(record.DefinitionId, out TileWordWriter[]? writers)
            ? writers : []).Distinct();

    /// <summary>Bindings over a fresh VRAM holding what a room of <paramref name="tileset"/> binds as it loads.</summary>
    private static BgArtworkBindings Loaded(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        var content = new BgArtworkBindings(new SnesVram());
        foreach (BgCharacterUpload upload in CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Transfer,
            inputs.Characters.Get(tileset.Characters).Transfer))
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
    internal sealed class TilesetUploads(bool ceres)
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

        /// <summary>What the first room with writers shows their words over, or null without writers.</summary>
        internal BgArtworkBindings? WordContent { get; private set; }

        /// <summary>Records that a room's <paramref name="writers"/> write over <paramref name="content"/>.</summary>
        internal void AddWriters(IEnumerable<TileWordWriter> writers, BgArtworkBindings content)
        {
            Writers.UnionWith(writers);
            WordContent ??= content;
        }

        /// <summary>Whether the tileset's rooms are Ceres rooms.</summary>
        internal bool Ceres { get; } = ceres;

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
    }

    /// <summary>The BG2 pages a library background leaves, under its installed name, and what then shows at each character.</summary>
    /// <param name="Name">The background's installed name.</param>
    /// <param name="Words">The pages' tilemap words.</param>
    /// <param name="Content">The bindings the room's load and the background leave.</param>
    internal sealed record RoomBackgroundMap(string Name, ushort[] Words, BgArtworkBindings Content);

    /// <summary>A collectible's graphics bound in one slot and the block definitions it writes there.</summary>
    /// <param name="Upload">The graphics bound.</param>
    /// <param name="Words">The tile words written, four per definition.</param>
    internal sealed record CollectibleSlotUpload(BgCharacterUpload Upload, ushort[] Words);
}
