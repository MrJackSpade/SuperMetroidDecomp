using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
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
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.AssetExtraction.Areas.Tourian;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What each tileset's rooms show over their characters after loading (#1392, #1466), each as a value of the appearance
/// input gameplay names it by, so its block sheet pictures every block for each value the inputs it depends on can take:
/// the animated tiles their room effects run, the Tourian statues' frames, Kraid's room backdrop and, in the Ceres Ridley
/// arena, its encounters' looks on the sprites' artwork; and the collectibles its rooms place, each pictured as its kind
/// alone (#1513, #1538), never under the room's other blocks.
/// </summary>
internal static class RoomBlockUploads
{
    /// <summary>The tilesets some retail room state loads, each once in room order: those a block sheet is composed for.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets() =>
        [.. RetailRooms.All.SelectMany(RoomStateLists.GetStates)
            .Select(state => RoomTilesets.Get(state.GraphicsSet)).Distinct()];

    /// <summary>
    /// The tilesets of the retail room states that show a turning floor (<see cref="RoomTurningFloors"/>), each once in room
    /// order: those a turning floor is composed for, and the only ones a room shows one in (#1536, #1649). A tileset no such
    /// state loads installs no floor.
    /// </summary>
    internal static IReadOnlyList<TilesetDefinition> FloorTilesets() =>
        [.. RetailRooms.All.SelectMany(RoomStateLists.GetStates).Where(RoomTurningFloors.ShowsOne)
            .Select(state => RoomTilesets.Get(state.GraphicsSet)).Distinct()];

    /// <summary>The installed name of every background image some retail room state shows, each once (#1536), as <see cref="BackgroundNamesOf"/> names them.</summary>
    internal static IReadOnlyList<string> BackgroundNames() => BackgroundNamesOf(RetailRooms.All.SelectMany(RoomStateLists.GetStates));

    /// <summary>
    /// The installed name of every background image one of <paramref name="states"/> shows, each once (#1536): in the
    /// state's tileset, the ordinary picture of a library background the player sees as its own picture and the picture of
    /// each door that shows its own (#1649), and its scrolling sky's rows. These are the only images a room selects: it shows
    /// its background's picture for the door it was entered through when that door is one of the background's picture
    /// doors, otherwise the ordinary one.
    /// </summary>
    /// <exception cref="InvalidDataException">A state's background is seen otherwise where nothing else draws what is seen.</exception>
    internal static IReadOnlyList<string> BackgroundNamesOf(IEnumerable<RoomStateSettings> states)
    {
        var names = new List<string>();
        foreach (RoomStateSettings state in Ensure.NotNull(states))
        {
            TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
            if (PicturedLibraryBackgrounds.TryProgramOf(state, out LibraryBackgroundProgram? library))
            {
                DoorDefinition?[] doors = [.. DoorVariants(library)];
                names.AddRange(doors.Select(door => RoomBackgroundImage.NameOf(library.Background, door, tileset)));
            }
            if (ScrollingSkyState.IsScrollingSkyRoomMain(state.MainBehavior))
                names.Add(RoomBackgroundImage.SkyNameOf(ScrollingSkyState.VariantOf(state.MainBehavior), tileset));
        }
        return [.. names.Distinct(StringComparer.Ordinal)];
    }

    /// <summary>
    /// The room effects some retail room state of <paramref name="tileset"/> runs that show on the effect overlay, each
    /// once (#1536): the effects whose liquid or atmosphere the tileset's effect overlay picture sheet pictures, and the only
    /// ones its rooms show there.
    /// </summary>
    internal static IEnumerable<RoomFxType> OverlayEffectsOf(TilesetDefinition tileset) =>
        RetailRooms.All.SelectMany(RoomStateLists.GetStates).Where(state => RoomTilesets.Get(state.GraphicsSet) == tileset)
            .SelectMany(EffectsIn).Distinct();

    /// <summary>The rooms of each tileset and what they show over their characters, in room order.</summary>
    /// <exception cref="InvalidDataException">A tileset is loaded both inside and outside Ceres.</exception>
    internal static IReadOnlyDictionary<TilesetDefinition, TilesetUploads> ByTileset(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var tilesets = new Dictionary<TilesetDefinition, TilesetUploads>();
        foreach (RoomDefinition room in RetailRooms.All)
        {
            bool ceres = room.AreaIndex == AreaId.Ceres;
            foreach (RoomStateSettings state in RoomStateLists.GetStates(room))
            {
                TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
                if (!tilesets.TryGetValue(tileset, out TilesetUploads? uploads))
                    tilesets.Add(tileset, uploads = new TilesetUploads(ceres, inputs));
                else if (uploads.Ceres != ceres)
                    throw new InvalidDataException($"Tileset {tileset} is loaded both inside and outside Ceres.");
                foreach (AppearanceOption option in Of(inputs, state))
                    uploads.Add(option);
                foreach (InWorldCollectibleKind kind in CollectiblesPlacedIn(state))
                    uploads.Place(kind);
                foreach (AnimatedCharacterPlacement frame in AnimatedFrames(inputs, state).Where(frame => frame.Set == BgCharacterSet.Tiles))
                    uploads.AddAnimated(frame);
                if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Crocomire))
                {
                    foreach (AnimatedCharacterPlacement frame in CrocomireMeltBandFrames(inputs))
                        uploads.AddAnimated(frame);
                }
                RoomBlockArtwork blockArtwork = RoomScope.BlockArtworkOf(new SelectedRoom(room, state));
                uploads.ShowBlocksOver(blockArtwork);
                BgCharacterUpload[] loadedObjects = [];
                AppearanceOption[] encounterLooks = [];
                if (blockArtwork == RoomBlockArtwork.Sprites)
                {
                    loadedObjects = [.. ObjectPageUploads(inputs, state)];
                    encounterLooks = [.. EncounterLookOptions(inputs, state)];
                    uploads.AddObjectPage(loadedObjects, encounterLooks);
                }
                // Only a library background the player sees as its own picture is pictured (#1649).
                if (PicturedLibraryBackgrounds.TryProgramOf(state, out LibraryBackgroundProgram? library))
                {
                    foreach (DoorDefinition? door in DoorVariants(library))
                        uploads.AddBackground(Replay(inputs, room, tileset, library, door, StacksBackgroundPages(state)));
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
                if (ScrollingSkyState.IsScrollingSkyRoomMain(state.MainBehavior))
                {
                    // The sky's rows show over what the room's load binds.
                    ScrollingSkyChunkTable variant = ScrollingSkyState.VariantOf(state.MainBehavior);
                    uploads.AddBackground(new RoomBackgroundMap(RoomBackgroundImage.SkyNameOf(variant, tileset),
                        BackgroundTileWords.ToWords(inputs.Skies.Tilemap(variant).Tiles), Loaded(inputs, tileset)));
                }
            }
        }
        return tilesets;
    }

    /// <summary>
    /// The collectibles that show their own picture which <paramref name="state"/>'s population places, each once: the only
    /// collectibles whose pictures a room of its tileset shows (#1513, #1538).
    /// </summary>
    private static HashSet<InWorldCollectibleKind> CollectiblesPlacedIn(RoomStateSettings state)
    {
        var placed = new HashSet<InWorldCollectibleKind>();
        foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.Span)
        {
            if (CollectibleObjects.TryIdentifyPermanentCollectible(placement.Kind, out InWorldCollectibleKind kind, out _)
                && CollectibleObjects.ShowsOwnPicture(kind))
                placed.Add(kind);
        }
        return placed;
    }

    /// <summary>What one state of a room shows over its tile characters after it loads, collectibles and animations aside: Kraid's backdrop.</summary>
    private static IEnumerable<AppearanceOption> Of(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        if (state.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Kraid))
        {
            yield return new AppearanceOption(AppearanceInputSpellings.KraidBackdrop, RoomAppearanceValues.Shown,
                new BgCharacterUpload(RoomBgArtwork.KraidBackdrop, BgCharacterSet.Tiles, RoomTileCharacters.KraidBackdropFirst,
                    (inputs.Bosses.KraidBackground ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters."))
                        .RoomBackgroundTiles.Characters));
        }
    }

    /// <summary>
    /// The frames one state of a room's animations paint over the block characters (#1406): its room effects' animated
    /// tiles and the Tourian statues' pictures.
    /// </summary>
    private static IEnumerable<AnimatedCharacterPlacement> AnimatedFrames(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        foreach (RoomEffectDefinition record in state.Fx?.Effects ?? [])
        {
            foreach (BlockAnimation animatedObject in record.BlockAnimations)
            {
                foreach (AnimatedCharacterPlacement frame in AnimatedTiles(inputs, animatedObject))
                    yield return frame;
            }
        }
        if (state.SetupBehavior == RoomSetupBehavior.RunStatueUnlockingAnimations)
        {
            foreach (TourianStatueAnimationDefinition statue in TourianStatueAnimatedTilePrograms.All)
            {
                int destination = AnimatedTileVramDestinations.WordOf(TourianStatueAnimatedTilePrograms.ObjectOf(statue.Statue));
                // Phantoon's and Ridley's statues paint characters outside the room's tile characters, which no block reads.
                if (!BgCharacterWords.AmongTiles(destination))
                    continue;
                foreach (TourianStatueBlockPicture picture in RoomFxBlockAnimationPictures.TourianStatuePictures)
                {
                    yield return AnimatedCharacterPlacement.At(statue.Picture(picture), destination,
                        inputs.AnimatedTiles.TourianStatuePicture(statue.Statue, picture));
                }
            }
        }
    }

    /// <summary>
    /// What Crocomire's melt image shows over his characters (#1392, #1406, #1652): each band of each melt image before its
    /// dissolve erases a column, which his body pictures the image over once. Each step of the dissolve hides columns of
    /// the image as Core shows it, so no step is pictured.
    /// </summary>
    private static IEnumerable<AnimatedCharacterPlacement> CrocomireMeltBandFrames(RoomBlockSheetInputs inputs)
    {
        CrocomireMeltingArtwork artwork = inputs.Bosses.CrocomireMelting
            ?? throw new InvalidDataException("Installed boss artwork has no Crocomire melting graphics.");
        foreach (CrocomireMeltingIllustration illustration in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            byte[] image = artwork.Characters(illustration).ToArray();
            for (int band = 0; band < CrocomireMelting.BandCount(illustration); band++)
            {
                yield return AnimatedCharacterPlacement.Of(CrocomireMelting.MeltBandFrame(new CrocomireMeltBandPicture(illustration, 0), band),
                    BgCharacterSet.Tiles, CrocomireMeltDissolve.BandFirstCharacter(band), CrocomireMeltDissolve.BandCharacters(image, band));
            }
        }
    }

    /// <summary>No door, for the background's ordinary picture, and each door that shows its own picture of it.</summary>
    private static IEnumerable<DoorDefinition?> DoorVariants(LibraryBackgroundProgram program) => [null, .. program.Background.PictureDoors];

    /// <summary>
    /// Replays <paramref name="program"/> as a room of <paramref name="tileset"/> entered through <paramref name="door"/>
    /// runs it: one period of the BG2 pages it leaves (<see cref="RoomBackgroundImageGeometry.OnePeriod"/>, #1651), the
    /// first page alone where both hold one tilemap, over the characters the room loads. Two different pages are laid out
    /// as the room shows them (#1617): one above the other where <paramref name="pagesStacked"/>, otherwise side by side.
    /// </summary>
    private static RoomBackgroundMap Replay(RoomBlockSheetInputs inputs, RoomDefinition room, TilesetDefinition tileset,
        LibraryBackgroundProgram program, DoorDefinition? door, bool pagesStacked)
    {
        BgArtworkBindings content = Loaded(inputs, tileset);
        var vram = new SnesVram();
        // A door no transfer names stands for every door the program does not tell apart.
        DoorDefinition entry = door ?? new DoorDefinition("no library background door", room,
            new DoorOrientation(DoorDirection.Right, DoorClosingBehavior.None), default, default, DoorSamusSpeed.None, DoorSetupDefinitions.None);
        LibraryBackgroundReplay.Execute(vram, program,
            entry, new LibraryBackgroundArtwork
            {
                TilemapArt = inputs.BackgroundTilemaps,
                SkyArt = inputs.Skies,
                HudArt = inputs.Hud,
                CharacterArt = inputs.Characters,
            });
        ushort[] words = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.Words).Select(word => vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word))];
        RoomBackgroundPeriod period = RoomBackgroundImageGeometry.AsShown(RoomBackgroundImageGeometry.OnePeriod(words), pagesStacked);
        return new RoomBackgroundMap(RoomBackgroundImage.NameOf(program.Background, door, tileset), period.Words, content) { WidthTiles = period.WidthTiles };
    }

    /// <summary>
    /// Whether a room in <paramref name="state"/> shows its background's two pages one above the other (#1617): the Tourian
    /// entrance's statues descend through them; every other room's layer lays them side by side.
    /// </summary>
    private static bool StacksBackgroundPages(RoomStateSettings state) =>
        (state.Fx?.Effects ?? []).Any(record => record.Type == RoomFxType.TourianEntranceStatue);

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
        IReadOnlyList<GraphicsSetMemberCharacters> members = GraphicsSetCharacterLayout.Of(RoomEnemyGraphicsSetLists.MembersOf(RoomEnemyGraphicsSetLists.Of(state))).Members;
        for (int member = 0; member < members.Count; member++)
        {
            ReadOnlyMemory<byte> tiles = members[member].Member.TilesIn(inputs.Objects.Enemies);
            yield return new BgCharacterUpload(new BgArtworkIdentity($"{members[member].Member.Name} set tiles"), BgCharacterSet.Tiles,
                EncounterCharacterPlacementDefinitions.EnemyTileAreaFirstCharacter + members[member].StagingOffset / SpriteCharacterTableLayout.CharacterByteCount, tiles);
        }
    }

    /// <summary>
    /// What <paramref name="state"/>'s encounters copy over the OBJ character table after its room loads, look by look of
    /// the encounters gameplay shows the looks of, each shown while its encounter shows its look (#1466, #1538): every copy
    /// of the looks up to it, in order, so the latest copy over a character shows there. Only the looks the state's room
    /// shows count (#1648): Ceres Ridley's self-destruct sequence copies his escape timer and warning ($A6:C4CB), which his
    /// arena shows so; the Ceres doors' own look only the elevator shaft's overlay door copies ($A6:F739), and his arena
    /// places none.
    /// </summary>
    private static IEnumerable<AppearanceOption> EncounterLookOptions(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        foreach (ComposedEncounter encounter in EncounterLookUploads.Of(RoomEnemyGraphicsSetLists.MembersOf(RoomEnemyGraphicsSetLists.Of(state))))
        {
            if (EncounterLookUploads.ShownAs(encounter) is not EncounterArtwork shownAs || !EncounterLookUploads.ShownIn(encounter, state))
                continue;
            EncounterLookSteps steps = EncounterLookUploads.Steps(encounter, inputs.Objects);
            IReadOnlyList<string> looks = EncounterLookUploads.Looks(encounter);
            for (int look = 1; look < looks.Count; look++)
            {
                var appearance = new EncounterLookAppearance(new EncounterLook(shownAs, looks[look]));
                EncounterArtworkUpload[] copies = [.. steps.Looks.Take(look + 1).SelectMany(earlier => earlier)];
                for (int copy = 0; copy < copies.Length; copy++)
                    yield return new AppearanceOption(appearance, RoomAppearanceValues.Shown,
                        new BgCharacterUpload(new BgArtworkIdentity($"{encounter} look {looks[look]} copy {copy}"), BgCharacterSet.Tiles,
                            copies[copy].FirstCharacter, copies[copy].Characters));
            }
        }
    }

    /// <summary>The HUD's characters as gameplay setup uploads them, before any room loads.</summary>
    internal static BgCharacterUpload HudCharacters(RoomBlockSheetInputs inputs) =>
        new(BgArtworkIdentity.Asset(InstalledArtworkId.StandardHudTiles), BgCharacterSet.Layer3, 0, Ensure.NotNull(inputs).Hud.Bytes[..HudTileAtlasFormat.CharacterByteCount]);

    /// <summary>The room effects <paramref name="state"/>'s rooms show on the effect overlay.</summary>
    private static IEnumerable<RoomFxType> EffectsIn(RoomStateSettings state) =>
        (state.Fx?.Effects ?? []).Select(record => record.Type)
            .Where(RoomFxOverlayState.ShowsOnOverlay).Distinct();

    /// <summary>Each frame a room effect of <paramref name="type"/> animates over BG3's characters.</summary>
    internal static IEnumerable<AnimatedCharacterPlacement> EffectCharacters(RoomBlockSheetInputs inputs, RoomFxType type)
    {
        BlockAnimation animatedObject = RoomFxBlockAnimationsState.ObjectOf(type);
        if (animatedObject == BlockAnimation.None)
            yield break;
        RoomFxBlockAnimationLoop mechanics = RoomFxBlockAnimationLoops.Require(animatedObject);
        for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            yield return Animated(inputs, mechanics.Frames[frame].Frame, mechanics.Image(frame),
                mechanics.PictureByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
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
    /// What a boss's body is shown over in <paramref name="state"/> (#1466): what the room loads (its library background
    /// writes the layer's map, never its characters), what the room binds after loading, and the frames its animations paint
    /// over its tile characters, Crocomire's melt bands among them in his room.
    /// </summary>
    internal static BossRoomCharacters ShownUnderBoss(RoomBlockSheetInputs inputs, RoomStateSettings state)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(state);
        TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
        BgArtworkBindings content = Loaded(inputs, tileset);
        AnimatedCharacterPlacement[] animated = [.. AnimatedFrames(inputs, state).Where(frame => frame.Set == BgCharacterSet.Tiles),
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
    private static IEnumerable<AnimatedCharacterPlacement> AnimatedTiles(RoomBlockSheetInputs inputs, BlockAnimation animatedObject)
    {
        if (WreckedShipTreadmills.TryResolve(animatedObject, out WreckedShipTreadmillLoop? treadmill))
        {
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return Animated(inputs, BlockAnimationFrames.Frame(animatedObject, frame), treadmill.Image(frame),
                    RoomFxBlockAnimationLoops.TreadmillFrameByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
            yield break;
        }
        if (animatedObject is not (BlockAnimation.HorizontalSpikes or BlockAnimation.MaridiaSandCeiling or BlockAnimation.MaridiaSandFalling))
            yield break;
        RoomFxBlockAnimationLoop mechanics = RoomFxBlockAnimationLoops.Require(animatedObject);
        for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            yield return Animated(inputs, mechanics.Frames[frame].Frame, mechanics.Image(frame),
                mechanics.PictureByteCount, AnimatedTileVramDestinations.WordOf(animatedObject));
    }

    /// <summary>Animation frame <paramref name="frame"/>, pictured from its installed characters.</summary>
    private static AnimatedCharacterPlacement Animated(RoomBlockSheetInputs inputs, ArtworkFrame frame,
        AnimatedTileImage image, int byteCount, int firstWord) =>
        AnimatedCharacterPlacement.At(frame, firstWord, Frame(inputs, image, byteCount));

    /// <summary>The installed bytes of an animated tile picture.</summary>
    private static ReadOnlyMemory<byte> Frame(RoomBlockSheetInputs inputs, AnimatedTileImage image, int byteCount) =>
        inputs.AnimatedTiles.TryResolve(image, byteCount, out ReadOnlyMemory<byte> frame) ? frame
            : throw new InvalidDataException($"Installed animated tiles hold no {byteCount}-byte picture {image}.");

    /// <summary>What one tileset's rooms show over their characters.</summary>
    /// <param name="ceres">Whether the tileset's rooms are Ceres rooms, which load no common room elements.</param>
    /// <param name="inputs">The installed artwork the uploads are drawn from.</param>
    internal sealed class TilesetUploads(bool ceres, RoomBlockSheetInputs inputs)
    {
        /// <summary>What the rooms show over their tile characters after loading, collectibles aside, each once.</summary>
        private readonly List<AppearanceOption> options = [];

        /// <summary>The collectibles showing their own picture that the rooms place.</summary>
        private readonly HashSet<InWorldCollectibleKind> placedCollectibles = [];

        /// <summary>The library backgrounds the tileset's rooms load, by name.</summary>
        private readonly Dictionary<string, RoomBackgroundMap> backgrounds = new(StringComparer.Ordinal);

        /// <summary>What the rooms show over BG3's characters after loading: their effects' frames, each once.</summary>
        private readonly List<AppearanceOption> bg3Options = [];

        /// <summary>The library backgrounds the tileset's rooms load.</summary>
        internal IEnumerable<RoomBackgroundMap> Backgrounds => backgrounds.Values;

        /// <summary>Adds a library background unless one of its name is known, which must picture the same.</summary>
        /// <exception cref="InvalidDataException">A background of the name is known and pictures something else.</exception>
        internal void AddBackground(RoomBackgroundMap background)
        {
            if (!backgrounds.TryAdd(background.Name, background) && (backgrounds[background.Name].WidthTiles != background.WidthTiles
                || !backgrounds[background.Name].Words.SequenceEqual(background.Words)))
                throw new InvalidDataException($"Rooms show library background {background.Name} in two ways; it has one picture.");
        }

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
        /// Whether some room of the tileset shows its blocks over the tileset's own artwork. A tileset every room of which
        /// points its blocks at the sprites' artwork, as Ceres Ridley's arena's setup does before its first frame ($8F:C97B),
        /// never shows them over the tileset's, so they have no picture over it (#1648).
        /// </summary>
        internal bool ShowsTilesetArtwork { get; private set; }

        /// <summary>Records that a room of the tileset shows its blocks over <paramref name="artwork"/>.</summary>
        /// <exception cref="InvalidDataException">The artwork is undefined.</exception>
        internal void ShowBlocksOver(RoomBlockArtwork artwork)
        {
            switch (artwork)
            {
                case RoomBlockArtwork.Tileset:
                    ShowsTilesetArtwork = true;
                    return;
                case RoomBlockArtwork.Sprites:
                    // What such a room shows there is recorded with its object page (AddObjectPage).
                    return;
                default:
                    throw new InvalidDataException($"Undefined block artwork {artwork}.");
            }
        }

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

        /// <summary>
        /// The graphics of each collectible showing its own picture that the rooms place, in kind order (#1453, #1538): its
        /// kind's pictures are composed from them alone, never under the room's other blocks (#1513), and an edit of the
        /// installed room block sheet restyles them.
        /// </summary>
        internal IReadOnlyList<RoomObjectDynamicCollectibleGraphic> Collectibles =>
            [.. placedCollectibles.Order().Select(DynamicCollectibleGraphics.Get)];

        /// <summary>Records that a room of the tileset places collectible <paramref name="kind"/>.</summary>
        /// <exception cref="InvalidDataException">The tileset's rooms are Ceres rooms, which place no collectibles.</exception>
        internal void Place(InWorldCollectibleKind kind)
        {
            if (Ceres)
                throw new InvalidDataException($"A Ceres room places collectible {kind}, but Ceres rooms place no collectibles.");
            placedCollectibles.Add(kind);
        }

        /// <summary>Adds an option unless the same value of the same input already shows the same artwork at the same place.</summary>
        internal void Add(AppearanceOption option) => AddOnce(options, option);

        /// <summary>Adds <paramref name="option"/> to <paramref name="known"/> unless the same value of the same input already shows there.</summary>
        private static void AddOnce(List<AppearanceOption> known, AppearanceOption option)
        {
            if (!known.Any(other => other.Input == option.Input && other.Value == option.Value && other.Upload.Identity == option.Upload.Identity
                && other.Upload.Set == option.Upload.Set && other.Upload.FirstCharacter == option.Upload.FirstCharacter))
                known.Add(option);
        }

        /// <summary>Adds a frame an animation of the tileset's rooms paints over the BG1 and BG2 characters (#1406).</summary>
        internal void AddAnimated(AnimatedCharacterPlacement frame) => Add(Ensure.NotNull(frame).Option);
    }

    /// <summary>
    /// One period of the BG2 pages a library background leaves (#1651), or the sky's rows, under its installed name, and what
    /// then shows at each character.
    /// </summary>
    /// <param name="Name">The background's installed name.</param>
    /// <param name="Words">
    /// The tilemap words of one period of what the background shows: a library background's one page where its two pages
    /// hold one tilemap, otherwise its two pages as the room shows them (#1617); or the sky's rows.
    /// </param>
    /// <param name="Content">The bindings the room's load leaves.</param>
    internal sealed record RoomBackgroundMap(string Name, ushort[] Words, BgArtworkBindings Content)
    {
        /// <summary>Tiles across one row of <see cref="Words"/>: a page, or one period across of a page that repeats (#1675).</summary>
        internal int WidthTiles { get; init; } = RoomBackgroundImageGeometry.PageTiles;
    }

    /// <summary>What an OBJ character table holds at one place: the artwork and the character it starts at.</summary>
    /// <param name="Identity">The artwork.</param>
    /// <param name="FirstCharacter">The character it starts at.</param>
    private readonly record struct PagePlacement(BgArtworkIdentity Identity, int FirstCharacter)
    {
        /// <summary>Where <paramref name="placement"/> places its artwork.</summary>
        internal static PagePlacement Of(BgCharacterUpload placement) => new(placement.Identity, placement.FirstCharacter);
    }
}
