using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Audits retail room-FX selection, visible liquid layers, animation, and transition behavior across named states and doors.</summary>
internal static class RoomFxRetailInventoryVerification
{
    /// <summary>Expected number of distinct retail room headers in the FX audit.</summary>
    private const int RetailRoomFxRoomCount = 262;
    /// <summary>Expected number of named retail room states selected by the source inventory.</summary>
    private const int RetailRoomFxStateCount = 323;
    /// <summary>Expected number of physical bank-$83 entry doors exercised by the audit.</summary>
    private const int RetailRoomFxDoorCount = 597;

    /// <summary>One bus's extracted room-FX catalogs; immutable, so every FX state built on that bus shares them.</summary>
    internal sealed record RetailFxCatalogs(RoomFxPaletteBlendCatalog PaletteBlends,
        RoomFxLayer3TilemapCatalog Layer3Tilemaps, RoomFxAnimatedTileAtlas AnimatedTiles);

    /// <summary>Reuses extracted FX art for states sharing an import bus without keeping that bus alive.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SuperMetroidAddressSpace, RetailFxCatalogs>
        retailFxCatalogs = new();

    /// <summary>Creates a production layer-three FX owner bound to art extracted once from the supplied retail bus.</summary>
    internal static RoomFxSession CreateRetailFxSession(SuperMetroidAddressSpace bus)
    {
        RetailFxCatalogs catalogs = retailFxCatalogs.GetValue(bus, source => new(
            RoomFxPaletteBlendFile.Read(new MemoryStream(
                SuperMetroid.AssetExtraction.Fx.RoomFxPaletteBlendExtractor.Extract(source))),
            RoomFxLayer3TilemapCatalog.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.Fx.RoomFxLayer3TilemapExtractor.Extract(source))),
            RoomFxAnimatedTileAtlasPng.Read(new MemoryStream(
                SuperMetroid.AssetExtraction.Fx.RoomFxAnimatedTileAtlasExtractor.Extract(source)))));
        return new()
        {
            PaletteBlendColors = catalogs.PaletteBlends,
            Layer3Tilemaps = catalogs.Layer3Tilemaps,
            AnimatedTileArtwork = catalogs.AnimatedTiles,
        };
    }

    /// <summary>
    /// Exhaustively compares every named retail room state and every physical entry door
    /// with the FX record selected by the production owner. The named-state and physical-
    /// door cardinalities are fixed cartridge facts, so silently omitting an alternate
    /// boss/event state or an unlabelled door header fails the suite.
    /// </summary>
    internal static void VerifyRetailRoomFxInventory()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        RetailFxRoomState[] states = File.ReadLines(symbolPath)
            .Select(ParseRetailFxRoomState)
            .Where(state => state.HasValue)
            .Select(state => (state ?? throw new InvalidOperationException("Verification requires state.")))
            .Where(state => state.RoomPointer != RetailRoomFxDefinitions.DebugRoomHeader)
            .DistinctBy(state => state.StatePointer)
            .OrderBy(state => state.RoomPointer)
            .ThenBy(state => state.StatePointer)
            .ToArray();
        ushort[] roomPointers = states
            .Select(state => state.RoomPointer)
            .Distinct()
            .ToArray();
        DoorDefinition[] doors = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, pointer))
            .ToArray();

        AssertEqual(RetailRoomFxRoomCount, roomPointers.Length,
            "retail room-FX inventory room count");
        AssertEqual(RetailRoomFxStateCount, states.Length,
            "retail room-FX inventory state count");
        AssertEqual(RetailRoomFxDoorCount, doors.Length,
            "retail room-FX inventory physical door count");
        Suite(nameof(VerifyRetailRoomFxAnimatedTileHeaders), () => VerifyRetailRoomFxAnimatedTileHeaders(bus));
        Suite(nameof(VerifyRetailLavaSurface), () => VerifyRetailLavaSurface(bus, states));

        var doorByPointer = doors.ToDictionary(door => door.Pointer);
        ILookup<ushort, DoorDefinition> incomingDoors = doors
            .ToLookup(door => RoomHeaderDefinitions.PointerOf(door.DestinationRoom));
        var classifications = new Dictionary<RetailLiquidClassification, int>();
        var auditedStatePointers = new HashSet<ushort>();
        var knownExamples = new Dictionary<RoomIdentity, HashSet<RetailLiquidClassification>>();
        var inventoryLines = new List<string>
        {
            "room\troom_header\tstate\tentry_door\tfx_list\tselected_record\t" +
            "expected_type\tactual_type\texpected_base_y\tactual_base_y\t" +
            "target_y\tvelocity\tliquid_options\tclassification",
        };
        int auditedEntryPaths = 0;
        int doorSpecificRecords = 0;
        int tidalEntryPaths = 0;

        foreach (RetailFxRoomState namedState in states)
        {
            SelectedRoom defaultRoom = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, namedState.RoomPointer);
            SelectedRoom room = defaultRoom with
            {
                State = SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.Load(bus, namedState.StatePointer),
            };
            AssertTrue(auditedStatePointers.Add(room.State.Pointer),
                $"room-FX state $8F:{room.State.Pointer:X4} audited once");

            foreach (ushort recordDoor in EnumerateSpecificFxRecordDoors(
                         bus,
                         room.State.FxPointer))
            {
                doorSpecificRecords++;
                AssertTrue(doorByPointer.TryGetValue(recordDoor, out DoorDefinition? fxDoor),
                    $"room {room.Identity} state $8F:{room.State.Pointer:X4} FX-specific " +
                    $"door $83:{recordDoor:X4} is a physical retail door");
                AssertEqual(room.Definition, (fxDoor ?? throw new InvalidOperationException("Verification requires fxDoor.")).DestinationRoom,
                    $"FX-specific door $83:{recordDoor:X4} enters room {room.Identity}");
            }

            ushort[] entryPaths = incomingDoors[room.Pointer]
                .Select(door => door.Pointer)
                .Prepend((ushort)0)
                .Distinct()
                .Order()
                .ToArray();
            foreach (ushort doorPointer in entryPaths)
            {
                RetailFxPathAudit pathAudit = AuditRetailFxEntryPath(
                    bus,
                    room,
                    doorPointer);
                RetailLiquidClassification classification = pathAudit.Classification;
                classifications[classification] =
                    classifications.GetValueOrDefault(classification) + 1;
                auditedEntryPaths++;
                inventoryLines.Add(string.Join('\t',
                    room.Identity.ToString(),
                    $"{room.Pointer:X4}",
                    $"{room.State.Pointer:X4}",
                    doorPointer == 0 ? "default" : $"{doorPointer:X4}",
                    room.State.FxPointer == 0 ? "none" : $"{room.State.FxPointer:X4}",
                    pathAudit.SelectedRecord == 0 ? "none" : $"{pathAudit.SelectedRecord:X4}",
                    pathAudit.ExpectedType.ToString(),
                    pathAudit.ActualType.ToString(),
                    $"{pathAudit.ExpectedBaseY:X4}",
                    $"{pathAudit.ActualBaseY:X4}",
                    $"{pathAudit.TargetY:X4}",
                    $"{pathAudit.Velocity:X4}",
                    $"{pathAudit.LiquidOptions:X2}",
                    classification.ToString()));
                if (classification is RetailLiquidClassification.HiddenWater or
                        RetailLiquidClassification.VisibleWater or
                        RetailLiquidClassification.HiddenLavaOrAcid or
                        RetailLiquidClassification.VisibleLavaOrAcid)
                {
                    ushort selectedRecord = SelectFxRecordIndependently(
                        bus,
                        room.State.FxPointer,
                        doorPointer);
                    if (selectedRecord != 0 &&
                        (ReadRetailFxRecordByte(
                            bus,
                            selectedRecord,
                            RoomFxRomDataRecordTooling.LiquidOptionsOffset) &
                            (RoomFxNativeSources.SmallTideOption |
                             RoomFxNativeSources.LargeTideOption)) != 0)
                    {
                        VerifyRetailLiquidTide(bus, room, doorPointer);
                        tidalEntryPaths++;
                    }
                }

                if (RetailRoomFxDefinitions.KnownExampleRooms.Contains(room.Identity))
                {
                    if (!knownExamples.TryGetValue(room.Identity, out HashSet<RetailLiquidClassification>? set))
                    {
                        set = [];
                        knownExamples.Add(room.Identity, set);
                    }
                    set.Add(classification);
                }
            }
        }

        AssertEqual(RetailRoomFxStateCount, auditedStatePointers.Count,
            "every retail room state has an FX classification");
        AssertKnownRetailLiquidExamples(knownExamples);
        Suite(nameof(VerifyRetailLiquidRise), () => VerifyRetailLiquidRise(bus, states, doors, RetailRoomFxDefinitions.RisingLavaRoom21));
        Suite(nameof(VerifyRetailLiquidRise), () => VerifyRetailLiquidRise(bus, states, doors, RetailRoomFxDefinitions.RisingLavaRoom28));
        Suite(nameof(VerifyRetailRisingLavaFeedback), () => VerifyRetailRisingLavaFeedback(bus, doors));
        Suite(nameof(VerifyRetailFxTransitionReset), () => VerifyRetailFxTransitionReset(bus, states));

        string inventoryPath = Path.GetFullPath(
            Path.Combine("csharp", "retail-room-fx-inventory.tsv"));
        File.WriteAllLines(inventoryPath, inventoryLines);

        Console.WriteLine(
            $"  Room FX inventory: audited {roomPointers.Length} rooms, {states.Length} states, " +
            $"{doors.Length} physical doors, {auditedEntryPaths} state/entry paths, and " +
            $"{doorSpecificRecords} door-specific FX records ({tidalEntryPaths} tidal paths); " +
            string.Join(", ", classifications.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={pair.Value}")) +
            $". Inventory: {Path.GetRelativePath(Directory.GetCurrentDirectory(), inventoryPath)}.");
    }

    /// <summary>The door an entry path names, or none for path zero, which matches only default records.</summary>
    private static DoorDefinition? EntryDoor(ushort doorPointer) =>
        doorPointer == 0 ? null : DoorDefinitions.Get(doorPointer);

    /// <summary>Compares independent ROM FX selection and liquid fields with the production owner for one room and entry door.</summary>
    private static RetailFxPathAudit AuditRetailFxEntryPath(
        SuperMetroidAddressSpace bus,
        SelectedRoom room,
        ushort doorPointer)
    {
        ushort expectedRecord = SelectFxRecordIndependently(
            bus,
            room.State.FxPointer,
            doorPointer);
        if (expectedRecord != 0)
        {
            byte blend = ReadRetailFxRecordByte(bus, expectedRecord,
                RoomFxRomDataRecordTooling.PaletteBlendOffset);
            AssertTrue(blend == 0 || RoomFxPaletteBlendDefinitions.Ids.Contains(FxNativeWords.PaletteBlend(blend)),
                $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
                $"$83:{doorPointer:X4} uses an extracted FX palette blend (${blend:X2})");
        }
        RoomFxType expectedType = expectedRecord == 0
            ? RoomFxType.None
            : SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Decode(
                ReadRetailFxRecordByte(
                    bus,
                    expectedRecord,
                    RoomFxRomDataRecordTooling.TypeOffset),
                $"retail room {room.Identity} state $8F:{room.State.Pointer:X4} " +
                $"entry $83:{doorPointer:X4}");
        ushort expectedBase = expectedRecord == 0
            ? (ushort)0
            : ReadRetailFxRecordWord(
                bus,
                expectedRecord,
                RoomFxRomDataRecordTooling.BaseYPositionOffset);
        ushort expectedTarget = expectedRecord == 0
            ? (ushort)0
            : ReadRetailFxRecordWord(
                bus,
                expectedRecord,
                RoomFxRomDataRecordTooling.TargetYPositionOffset);
        ushort expectedVelocity = expectedRecord == 0
            ? (ushort)0
            : ReadRetailFxRecordWord(
                bus,
                expectedRecord,
                RoomFxRomDataRecordTooling.YVelocityOffset);

        var vram = new SnesVram();
        var cgram = new GameplayColours(new SpriteColourSets());
        SeedVisibleRoomFxPalette(cgram);
        var actualSession = CreateRetailFxSession(bus);
        var actual = RoomLayer3FxState.ForRoom(actualSession, cgram.BackgroundSets,
            room.State.Fx,
            EntryDoor(doorPointer),
            randomNumber: 0);
        AssertEqual(expectedRecord,
            RoomFxDefinitions.SelectedRecordPointer(room.State.Fx, EntryDoor(doorPointer)),
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} record selection");
        AssertEqual(expectedType, actual.Type,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX type");
        AssertEqual(expectedBase, actual.BaseY.Whole,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX base Y");
        AssertEqual(expectedTarget, actual.TargetSurface.Y,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX target Y");
        AssertEqual(expectedVelocity, actual.YVelocity.RawValue,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX velocity");

        bool liquid = expectedType is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid;
        // The audit report prints the record's liquid-options byte as the cartridge stores it.
        ushort liquidOptions = actual.LiquidOptions.ToNativeWord();
        RetailLiquidClassification classification = !liquid
            ? expectedType == RoomFxType.None
                ? RetailLiquidClassification.None
                : RetailLiquidClassification.NonLiquidFx
            : unchecked((short)expectedBase) < 0
                ? expectedType == RoomFxType.Water
                    ? RetailLiquidClassification.HiddenWater
                    : RetailLiquidClassification.HiddenLavaOrAcid
                : expectedType == RoomFxType.Water
                    ? RetailLiquidClassification.VisibleWater
                    : RetailLiquidClassification.VisibleLavaOrAcid;
        var audit = new RetailFxPathAudit(
            expectedRecord,
            expectedType,
            actual.Type,
            expectedBase,
            actual.BaseY.Whole,
            expectedTarget,
            expectedVelocity,
            liquidOptions,
            classification);
        if (!liquid)
            return audit;

        if (unchecked((short)expectedBase) < 0)
        {
            actual.PrimeViewport(new Position16<RoomPixels>(0, 0));
            actual.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
            RoomLayer3FxRenderSnapshot hiddenSnapshot = actual.CaptureForDisplay()
                ?? throw new InvalidDataException(
                    $"Room {room.Identity} negative liquid record $83:{expectedRecord:X4} " +
                    "published no render snapshot.");
            RgbaPixel[] hiddenFrame = RenderRoomFx(vram, cgram, hiddenSnapshot, EffectPlane(actual));
            AssertTrue(hiddenFrame.SequenceEqual(RenderRoomFxBackdrop(cgram)),
                $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
                $"$83:{doorPointer:X4} negative liquid surface is visually absent");
            return audit;
        }

        // Place every nonnegative surface inside a deterministic gameplay viewport. This
        // verifies the selected production record can actually reach the compositor; real
        // entry-camera and timed-rise behavior receive separate cartridge tests below.
        ushort cameraY = expectedBase > RetailRoomFxDefinitions.AuditSurfaceScreenY
            ? unchecked((ushort)(expectedBase - RetailRoomFxDefinitions.AuditSurfaceScreenY))
            : (ushort)0;
        actual.PrimeViewport(new Position16<RoomPixels>(0, cameraY));
        var liquidFrames = new ArtworkFrameSelections();
        actual.Step(liquidFrames, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, cameraY), TimeIsFrozen = false, MainGameLoopCarry = true });
        // The liquid's frames are selected, and BG3's pictures show the selected frame: the render reads it where the cartridge copies it.
        if (RoomFxAnimatedTilesState.ObjectOf(expectedType) is var liquidTiles and not AnimatedTileObject.None)
            AnimatedTileFrameArtwork.ShowIn(vram, liquidFrames, actualSession.AnimatedTileArtwork
                ?? throw new InvalidOperationException("The retail FX session has no animated tiles."), liquidTiles);

        if (expectedType is RoomFxType.Lava or RoomFxType.Acid)
        {
            RoomLayer3FxRenderSnapshot visibleSnapshot = actual.CaptureForDisplay()
                ?? throw new InvalidDataException(
                    $"Room {room.Identity} visible liquid record $83:{expectedRecord:X4} " +
                    "published no render snapshot.");
            RgbaPixel[] visibleFrame = RenderRoomFx(vram, cgram, visibleSnapshot, EffectPlane(actual));
            AssertTrue(!visibleFrame.SequenceEqual(RenderRoomFxBackdrop(cgram)),
                $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
                $"$83:{doorPointer:X4} visibly composes {expectedType}");
        }

        return audit;
    }

    /// <summary>Checks the native animated-tile headers used by water, lava, and other room-FX objects.</summary>
    private static void VerifyRetailRoomFxAnimatedTileHeaders(SuperMetroidAddressSpace bus)
    {
        Suite(nameof(VerifyRetailRoomFxAnimatedTileHeader), () => VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            FxNativeWords.AnimatedTileObjects.Word(AnimatedTileObject.Lava),
            RoomFxNativeSources.LavaFirstInstruction,
            RoomFxAnimatedTileMechanicsDefinitions.Require(AnimatedTileObject.Lava).FrameByteCount,
            AnimatedTileVramDestinations.WordOf(AnimatedTileObject.Lava),
            RoomFxNativeSources.LavaFirstFrame,
            "lava"));
        Suite(nameof(VerifyRetailRoomFxAnimatedTileHeader), () => VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            FxNativeWords.AnimatedTileObjects.Word(AnimatedTileObject.Acid),
            RoomFxNativeSources.AcidFirstInstruction,
            RoomFxAnimatedTileMechanicsDefinitions.Require(AnimatedTileObject.Lava).FrameByteCount,
            AnimatedTileVramDestinations.WordOf(AnimatedTileObject.Lava),
            RoomFxNativeSources.AcidFirstFrame,
            "acid"));
        Suite(nameof(VerifyRetailRoomFxAnimatedTileHeader), () => VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            FxNativeWords.AnimatedTileObjects.Word(AnimatedTileObject.Rain),
            RoomFxNativeSources.RainFirstInstruction,
            RoomFxAnimatedTileMechanicsDefinitions.Require(AnimatedTileObject.Rain).FrameByteCount,
            AnimatedTileVramDestinations.WordOf(AnimatedTileObject.Rain),
            RoomFxNativeSources.RainFirstFrame,
            "rain"));
    }

    /// <summary>
    /// Reproduces issue #274 against Business Center's real lava record, tilemap, and
    /// bank-$87 animation. The body alone is insufficient: the first source row begins one
    /// tile above the liquid coordinate and must remain visible as the bubbling boundary.
    /// </summary>
    private static void VerifyRetailLavaSurface(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states)
    {
        SelectedRoom room = LoadRoomByIdentity(
            bus,
            states,
            RetailRoomFxDefinitions.VisibleLavaRoom);
        var vram = new SnesVram();
        var cgram = new GameplayColours(new SpriteColourSets());
        var fxSession = CreateRetailFxSession(bus);
        var fx = RoomLayer3FxState.ForRoom(fxSession, cgram.BackgroundSets, room.State.Fx, door: null, randomNumber: 0);

        // Give the three nontransparent two-bit values distinct colors so the assertion
        // observes changes in the ROM pixels, not merely a still opaque rectangle.
        cgram.AsPaletteColors().SetColor(25, RgbColor.FromWord(0x001f));
        cgram.AsPaletteColors().SetColor(26, RgbColor.FromWord(0x03e0));
        cgram.AsPaletteColors().SetColor(27, RgbColor.FromWord(0x7c00));
        ushort cameraY = unchecked((ushort)(
            fx.BaseY.Whole - RetailRoomFxDefinitions.AuditSurfaceScreenY));
        fx.PrimeViewport(new Position16<RoomPixels>(0, cameraY));
        // The lava's frames are selected, and BG3's pictures show the selected frame: the render reads it where the cartridge copies it.
        var lavaFrames = new ArtworkFrameSelections();
        RoomFxAnimatedTileAtlas lavaArtwork = fxSession.AnimatedTileArtwork ?? throw new InvalidOperationException("The retail FX session has no animated tiles.");
        fx.Step(lavaFrames, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, cameraY), TimeIsFrozen = false, MainGameLoopCarry = true });
        AnimatedTileFrameArtwork.ShowIn(vram, lavaFrames, lavaArtwork, AnimatedTileObject.Lava);

        RoomLayer3FxRenderSnapshot firstSnapshot = fx.CaptureForDisplay()
            ?? throw new InvalidDataException("Business Center lava published no render snapshot.");
        // The BG3 plane the room shows under a blank HUD: the lava page its effect chose (#1348).
        Layer3Plane plane = EffectPlane(fx);
        RgbaPixel[] first = RenderRoomFx(vram, cgram, firstSnapshot, plane);
        RgbaPixel[] backdrop = RenderRoomFxBackdrop(cgram);
        int surfaceFirstRow = firstSnapshot.WaterSurfaceScreenY -
            (ScreenGeometry.BackgroundTileSizePixels - 1);
        RgbaPixel[] firstSurface = ReadRows(
            first,
            surfaceFirstRow,
            firstSnapshot.WaterSurfaceScreenY + 1);
        AssertTrue(!firstSurface.SequenceEqual(ReadRows(backdrop, surfaceFirstRow, firstSnapshot.WaterSurfaceScreenY + 1)),
            "room $02/$01 renders the animated lava surface through its production compositor");
        AssertTrue(!ReadRows(
                first,
                firstSnapshot.WaterSurfaceScreenY + 1,
                firstSnapshot.WaterSurfaceScreenY + 9)
            .SequenceEqual(ReadRows(backdrop, firstSnapshot.WaterSurfaceScreenY + 1, firstSnapshot.WaterSurfaceScreenY + 9)),
            "room $02/$01 renders the lava body below its surface");

        for (int frame = 0;
             frame < RetailRoomFxDefinitions.LavaSurfaceFrameDuration;
             frame++)
        {
            fx.Step(lavaFrames, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, cameraY), TimeIsFrozen = false, MainGameLoopCarry = true });
        }
        AnimatedTileFrameArtwork.ShowIn(vram, lavaFrames, lavaArtwork, AnimatedTileObject.Lava);
        RgbaPixel[] second = RenderRoomFx(
            vram,
            cgram,
            (fx.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires fx.CaptureForDisplay().")),
            plane);
        RgbaPixel[] secondSurface = ReadRows(
            second,
            surfaceFirstRow,
            firstSnapshot.WaterSurfaceScreenY + 1);
        AssertTrue(!firstSurface.SequenceEqual(secondSurface),
            "room $02/$01 lava surface advances to the next bank-$87 animation frame");
    }

    /// <summary>
    /// The frame the display renderer draws of a captured BG3 FX layer alone over the backdrop, with its current VRAM image
    /// and colours, for visible-surface comparisons.
    /// </summary>
    private static RgbaPixel[] RenderRoomFx(
        SnesVram vram,
        GameplayColours cgram,
        RoomLayer3FxRenderSnapshot snapshot,
        Layer3Plane plane)
    {
        FxPlaneBlendRenderLayer? layer = GameplayEffectCapture.CaptureRoomLayer3Fx(snapshot,
            Layer3CellsFixtures.FromWords(plane.Words, BgArtworkFixtures.ComposedVram(vram, new BgArtworkBytes()).Bytes, cgram.Colors));
        return DisplayRendering.Render(new LayeredScene(new FramePaint(cgram.Colors, []),
            layer is null ? [] : new RenderLayer[] { layer }, FadeLevel.Full));
    }

    /// <summary>The frame the display renderer draws of the backdrop alone, which an absent FX layer leaves unchanged.</summary>
    private static RgbaPixel[] RenderRoomFxBackdrop(GameplayColours cgram) =>
        DisplayRendering.Render(new LayeredScene(new FramePaint(cgram.Colors, []), [], FadeLevel.Full));

    /// <summary>The BG3 plane a room effect shows under a blank HUD: the page its effect chose (#1348).</summary>
    private static Layer3Plane EffectPlane(RoomLayer3FxState fx) =>
        Layer3Plane.Compose(new ushort[Layer3Plane.HudRows * Layer3Plane.Columns], fx.EffectPageWords());

    /// <summary>Copies a vertical frame band so the audit can compare the animated liquid boundary.</summary>
    private static RgbaPixel[] ReadRows(RgbaPixel[] frame, int firstRow, int endRow) =>
        frame.AsSpan(
            firstRow * DisplayScreen.Width,
            (endRow - firstRow) * DisplayScreen.Width)
        .ToArray();

    /// <summary>Compares an animated FX object's instruction pointer, transfer size, destination, and first source with ROM.</summary>
    private static void VerifyRetailRoomFxAnimatedTileHeader(
        SuperMetroidAddressSpace bus,
        ushort objectPointer,
        ushort expectedInstruction,
        int expectedSize,
        ushort expectedDestination,
        ushort expectedFirstSource,
        string name)
    {
        int header = RoomFxNativeSources.AnimatedTilesBank | objectPointer;
        AssertEqual(expectedInstruction, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), header),
            $"retail {name} animated-tile instruction list");
        AssertEqual(expectedSize, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), header + 2),
            $"retail {name} animated-tile frame size");
        AssertEqual(expectedDestination, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), header + 4),
            $"retail {name} animated-tile VRAM destination");
        AssertEqual(expectedFirstSource,
            RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus),
                RoomFxNativeSources.AnimatedTilesBank | expectedInstruction + 2),
            $"retail {name} animated-tile first source frame");
    }

    /// <summary>Reads a native FX byte directly from the import-only cartridge source.</summary>
    private static byte ReadRetailFxRecordByte(SuperMetroidAddressSpace bus, ushort record, int fieldOffset)
    {
        if ((uint)fieldOffset >= RoomFxToolingRomData.Record.ByteCount)
            throw new ArgumentOutOfRangeException(nameof(fieldOffset));
        return CartridgeImportSource.Require(bus).ReadCartridgeByte(
            RoomFxRomDataBanksTooling.RoomDefinitions | (record + fieldOffset));
    }

    /// <summary>Reads a native FX word directly from the import-only cartridge source.</summary>
    private static ushort ReadRetailFxRecordWord(SuperMetroidAddressSpace bus, ushort record, int fieldOffset)
    {
        if ((uint)fieldOffset > RoomFxToolingRomData.Record.ByteCount - sizeof(ushort))
            throw new ArgumentOutOfRangeException(nameof(fieldOffset));
        return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            RoomFxRomDataBanksTooling.RoomDefinitions | (record + fieldOffset));
    }

    /// <summary>
    /// Selects an FX record without calling the production selector, providing an
    /// independent expected value for every door path in the cartridge audit.
    /// </summary>
    private static ushort SelectFxRecordIndependently(
        SuperMetroidAddressSpace bus,
        ushort listPointer,
        ushort doorPointer)
    {
        if (listPointer == 0)
            return 0;

        ushort cursor = listPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidateDoor = ReadRetailFxRecordWord(
                bus,
                cursor,
                RoomFxRomDataRecordToolingConstants.DoorPointerOffset);
            if (candidateDoor == RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                return 0;
            if (candidateDoor == 0 || candidateDoor == doorPointer)
                return cursor;
            cursor = unchecked((ushort)(cursor + RoomFxToolingRomData.Record.ByteCount));
        }

        throw new InvalidDataException(
            $"Independent room-FX selector did not terminate list $83:{listPointer:X4}.");
    }

    /// <summary>Yields door identities explicitly named by a native FX record list, stopping at its terminator.</summary>
    private static IEnumerable<ushort> EnumerateSpecificFxRecordDoors(
        SuperMetroidAddressSpace bus,
        ushort listPointer)
    {
        if (listPointer == 0)
            yield break;

        ushort cursor = listPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort doorPointer = ReadRetailFxRecordWord(
                bus,
                cursor,
                RoomFxRomDataRecordToolingConstants.DoorPointerOffset);
            if (doorPointer is 0 or RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                yield break;
            yield return doorPointer;
            cursor = unchecked((ushort)(cursor + RoomFxToolingRomData.Record.ByteCount));
        }

        throw new InvalidDataException(
            $"Room-FX inventory did not terminate list $83:{listPointer:X4}.");
    }

    /// <summary>Checks the authored timer, velocity, and target Y of a known rising-lava door path.</summary>
    private static void VerifyRetailLiquidRise(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states,
        IReadOnlyList<DoorDefinition> doors,
        RisingLiquidAuditDefinition definition)
    {
        RetailFxRoomState namedState = states.Single(state =>
        {
            SelectedRoom header = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, state.RoomPointer);
            return header.Identity == definition.Room;
        });
        SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, namedState.RoomPointer) with
        {
            State = SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.Load(bus, namedState.StatePointer),
        };
        DoorDefinition door = doors.Single(candidate =>
            candidate.Pointer == definition.EntryDoor);
        AssertEqual(room.Definition, door.DestinationRoom,
            $"rising-lava door $83:{door.Pointer:X4} destination");

        var vram = new SnesVram();
        var cgram = new GameplayColours(new SpriteColourSets());
        SeedVisibleRoomFxPalette(cgram);
        var fxSession = CreateRetailFxSession(bus);
        var fx = RoomLayer3FxState.ForRoom(fxSession, cgram.BackgroundSets,
            room.State.Fx,
            door,
            randomNumber: 0,
            room: room.Definition);
        AssertEqual(RoomFxType.Lava, fx.Type, $"room {definition.Room} rising FX type");
        AssertEqual(definition.InitialY, fx.BaseY.Whole,
            $"room {definition.Room} initial lava Y");
        AssertEqual(definition.TargetY, fx.TargetSurface.Y,
            $"room {definition.Room} target lava Y");

        ushort cameraY = door.DestinationScreen.ToRoomPixels().Y;
        fx.PrimeViewport(new Position16<RoomPixels>(0, cameraY));
        AssertTrue((fx.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires fx.CaptureForDisplay().")).WaterSurfaceScreenY >=
                DisplayScreen.Height,
            $"room {definition.Room} entry lava begins below the viewport");

        int movingFrames = 0;
        var risenFrames = new ArtworkFrameSelections();
        for (; movingFrames < RetailRoomFxDefinitions.MaximumRiseAuditFrames; movingFrames++)
        {
            fx.Step(risenFrames, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, cameraY), TimeIsFrozen = false, MainGameLoopCarry = true });
            if (fx.YVelocity.RawValue == 0)
                break;
        }
        AssertTrue(movingFrames < RetailRoomFxDefinitions.MaximumRiseAuditFrames,
            $"room {definition.Room} lava reaches its cartridge target");
        AssertEqual(definition.TargetY, fx.BaseY.Whole,
            $"room {definition.Room} lava stops at its cartridge target");

        AnimatedTileFrameArtwork.ShowIn(vram, risenFrames, fxSession.AnimatedTileArtwork
            ?? throw new InvalidOperationException("The retail FX session has no animated tiles."), AnimatedTileObject.Lava);
        Layer3Plane finalPlane = EffectPlane(fx);
        RoomLayer3FxRenderSnapshot finalSnapshot = fx.CaptureForDisplay()
            ?? throw new InvalidDataException(
                $"Room {definition.Room} rising lava published no final snapshot.");
        AssertTrue(finalSnapshot.WaterSurfaceScreenY is >= DisplayScreen.HudHeight and
                < DisplayScreen.Height,
            $"room {definition.Room} risen lava enters the gameplay viewport");
        RgbaPixel[] frame = RenderRoomFx(vram, cgram, finalSnapshot, finalPlane);
        AssertTrue(!frame.SequenceEqual(RenderRoomFxBackdrop(cgram)),
            $"room {definition.Room} risen lava is visibly composed");
    }

    /// <summary>
    /// Drives room $02/$28 through its real $83:929A entry and asserts the bank-$88
    /// audiovisual outputs at the full runtime seam. This specifically guards the missing
    /// rumble and shake report rather than accepting liquid position as a proxy.
    /// </summary>
    private static void VerifyRetailRisingLavaFeedback(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<DoorDefinition> doors)
    {
        DoorDefinition door = doors.Single(candidate =>
            candidate.Pointer == RetailRoomFxDefinitions.RisingLavaRoom28.EntryDoor);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(door);
        if (runtime.Player.Samus is null)
            throw new InvalidDataException("Room $02/$28 runtime load omitted Samus.");
        runtime.Player.Samus.Control.LockHandlers();

        int referenceSoundTimer = 0, referenceSoundIndex = 0, referenceSoundCount = 0;
        void StepAndVerifySound(int frame)
        {
            // Verify the authored timer on every frame rather than freezing a
            // total from the old global RNG sequence. The bank-$88 HDMA prologue
            // samples the shared RNG before swapping it and before the main loop
            // advances it.
            int sampledRandom = runtime.System.RandomNumber;
            int expectedRequests = 0;
            if (frame >= 1 && frame <= RetailRoomFxDefinitions.Room28TargetFrame && --referenceSoundTimer < 0)
            {
                int[] nativeBaseTimers = [1, 3, 2, 1, 1, 2, 2, 1]; // $88:B256 interleaved timer words.
                // ADC at $88:B245 also adds the preserved main-loop carry.
                referenceSoundTimer = nativeBaseTimers[referenceSoundIndex++ % 8] + (sampledRandom & 3) +
                    (runtime.System.MainGameLoopCarry ? 1 : 0);
                expectedRequests = 1;
                referenceSoundCount++;
            }
            runtime.StepFrame(0);
            // Lava's ambient sound ($88:B421, #1269) shares the request list; this checks the
            // earthquake cadence only.
            AssertEqual(expectedRequests, runtime.RoomLayer3Fx.SoundRequests.Count(request =>
                    request.SoundEffect == Library2Sound.Lavaquake),
                $"room $02/$28 earthquake sound cadence frame {frame}");
        }

        // $88:C3E9 installs the BG3 callback on the room's first HDMA pass; the
        // verification load skips the door fade that would otherwise run it.
        // HandleRoomShaking leaves the default (zero) delta on a frame it does not shake;
        // the rising-liquid type's BG1/BG2 X displacement words are nonzero, so every
        // accepted frame of this effect publishes a non-default delta.
        static bool Shook(RoomShakeFrameResult shake) => shake != default;

        runtime.StepFrame(0);
        AssertTrue(!Shook(runtime.Enemies.Earthquake.LastRoomShake), "room $02/$28 callback install pass does not shake");
        AssertEqual(0, runtime.RoomLayer3Fx.SoundRequests.Count, "room $02/$28 callback install pass is silent");

        StepAndVerifySound(0);
        RoomShakeFrameResult dormantShake = runtime.Enemies.Earthquake.LastRoomShake;
        AssertTrue(!Shook(dormantShake),
            "room $02/$28 dormant rise initializer does not shake one frame early");
        AssertEqual(0, runtime.RoomLayer3Fx.SoundRequests.Count,
            "room $02/$28 dormant rise initializer does not sound one frame early");

        StepAndVerifySound(1);
        RoomShakeFrameResult firstActiveShake = runtime.Enemies.Earthquake.LastRoomShake;
        AssertEqual(1, runtime.RoomLayer3Fx.SoundRequests.Count,
            "room $02/$28 first wait frame emits one earthquake sound");
        SoundRequest sound = runtime.RoomLayer3Fx.SoundRequests[0];
        AssertEqual(Library2Sound.Lavaquake, sound.SoundEffect,
            "room $02/$28 earthquake sound identity");
        AssertEqual(6, sound.MaximumQueued.Requests,
            "room $02/$28 earthquake sound queue cap");
        AssertEqual(RoomEarthquakeDefinitions.RisingLiquidType, runtime.Enemies.Earthquake.EarthquakeType,
            "room $02/$28 earthquake type");
        AssertTrue(Shook(firstActiveShake) &&
            runtime.Enemies.Earthquake.EarthquakeType >= RoomEarthquakeDefinitions.FirstEnemyShakingType,
            "room $02/$28 first wait frame shakes backgrounds and enemies");

        int table = RoomFxNativeSources.EarthquakeDisplacementTable +
            RoomEarthquakeDefinitions.RisingLiquidType *
            RoomFxNativeSources.EarthquakeDisplacementRecordBytes;
        AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table)),
            firstActiveShake.Bg1.X, "room $02/$28 first shake BG1 X");
        AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + 2)),
            firstActiveShake.Bg1.Y, "room $02/$28 first shake BG1 Y");
        AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + 4)),
            firstActiveShake.Bg2.X, "room $02/$28 first shake BG2 X");
        AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + 6)),
            firstActiveShake.Bg2.Y, "room $02/$28 first shake BG2 Y");

        StepAndVerifySound(2);
        RoomShakeFrameResult secondActiveShake = runtime.Enemies.Earthquake.LastRoomShake;
        AssertEqual(unchecked((short)-firstActiveShake.Bg1.X), secondActiveShake.Bg1.X,
            "room $02/$28 alternating shake BG1 X");
        AssertEqual(unchecked((short)-firstActiveShake.Bg1.Y), secondActiveShake.Bg1.Y,
            "room $02/$28 alternating shake BG1 Y");
        AssertEqual(unchecked((short)-firstActiveShake.Bg2.X), secondActiveShake.Bg2.X,
            "room $02/$28 alternating shake BG2 X");
        AssertEqual(unchecked((short)-firstActiveShake.Bg2.Y), secondActiveShake.Bg2.Y,
            "room $02/$28 alternating shake BG2 Y");

        int appliedFrames = 2;
        int EarthquakeSounds() => runtime.RoomLayer3Fx.SoundRequests.Count(request =>
            request.SoundEffect == Library2Sound.Lavaquake);
        int emittedSounds = 1 + EarthquakeSounds();
        int firstMovementFrame = -1;
        int targetFrame = -1;
        int lastShakeFrame = 2;
        ushort initialY = runtime.RoomLayer3Fx.BaseY.Whole;
        for (int frame = 3; frame < RetailRoomFxDefinitions.MaximumRiseAuditFrames; frame++)
        {
            StepAndVerifySound(frame);
            if (Shook(runtime.Enemies.Earthquake.LastRoomShake))
            {
                appliedFrames++;
                lastShakeFrame = frame;
            }
            emittedSounds += EarthquakeSounds();
            if (firstMovementFrame < 0 && runtime.RoomLayer3Fx.BaseY.Whole != initialY)
                firstMovementFrame = frame;
            if (targetFrame < 0 && runtime.RoomLayer3Fx.YVelocity.RawValue == 0)
                targetFrame = frame;
            if (targetFrame >= 0 && runtime.Enemies.Earthquake.EarthquakeTimer.IsZero)
                break;
        }

        AssertEqual(RetailRoomFxDefinitions.Room28FirstMovementFrame, firstMovementFrame,
            "room $02/$28 first lava movement frame");
        AssertEqual(RetailRoomFxDefinitions.Room28TargetFrame, targetFrame,
            "room $02/$28 lava target frame");
        AssertEqual(RetailRoomFxDefinitions.Room28LastShakeFrame, lastShakeFrame,
            "room $02/$28 final shake frame");
        AssertEqual(RetailRoomFxDefinitions.Room28ShakeFrameCount, appliedFrames,
            "room $02/$28 total shake frames");
        AssertEqual(referenceSoundCount, emittedSounds,
            "room $02/$28 deterministic earthquake sound count");
    }

    /// <summary>Advances a native tide through both waveform halves and checks its bounded surface displacement.</summary>
    private static void VerifyRetailLiquidTide(
        SuperMetroidAddressSpace bus,
        SelectedRoom room,
        ushort doorPointer)
    {
        var fxSession = CreateRetailFxSession(bus);
        RoomLayer3FxState fx;
        fx = RoomLayer3FxState.ForRoom(fxSession, new BackgroundColourSets(),
            room.State.Fx,
            EntryDoor(doorPointer),
            randomNumber: 0);
        fx.PrimeViewport(new Position16<RoomPixels>(0, 0));

        int maximumAbsoluteOffset = 0;
        for (int frame = 0; frame < RetailRoomFxDefinitions.TideAuditFrames; frame++)
        {
            // A fresh VRAM object is sufficient here because the assertion targets the
            // liquid position, not animation output. The production animated-tile owner
            // still executes and therefore remains covered by fail-loud dispatch.
            fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
            int offset = unchecked((short)(fx.Surface.Y - fx.BaseY.Whole));
            maximumAbsoluteOffset = Math.Max(maximumAbsoluteOffset, Math.Abs(offset));
        }

        int expectedMaximum =
            (fx.LiquidOptions & RoomLiquidOptions.SmallTide) != 0
                ? LiquidTideDefinitions.SmallTideScale
                : LiquidTideDefinitions.LargeTideScale;
        AssertTrue(maximumAbsoluteOffset > 0 && maximumAbsoluteOffset <= expectedMaximum,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} tide moves within ±{expectedMaximum} pixels");
    }

    /// <summary>Checks that moving from visible lava to a hidden-liquid room clears the old BG3 presentation.</summary>
    private static void VerifyRetailFxTransitionReset(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states)
    {
        SelectedRoom visibleRoom = LoadRoomByIdentity(
            bus,
            states,
            RetailRoomFxDefinitions.VisibleLavaRoom);
        SelectedRoom hiddenRoom = LoadRoomByIdentity(
            bus,
            states,
            RetailRoomFxDefinitions.HiddenLavaRoom);

        var vram = new SnesVram();
        var cgram = new GameplayColours(new SpriteColourSets());
        SeedVisibleRoomFxPalette(cgram);
        var fxSession = CreateRetailFxSession(bus);
        var fx = RoomLayer3FxState.ForRoom(fxSession, cgram.BackgroundSets, visibleRoom.State.Fx, door: null, randomNumber: 0);
        fx.PrimeViewport(new Position16<RoomPixels>(0, 0));
        fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
        AssertEqual(RoomFxType.Lava, fx.Type, "transition source owns visible lava");

        // Loading a no-FX room must clear the owner even though the old tilemap and
        // animated characters remain in VRAM until another producer overwrites them.
        fx = RoomLayer3FxState.ForRoom(fxSession, cgram.BackgroundSets, fx: null, door: null, randomNumber: 0);
        AssertTrue(fx.CaptureForDisplay() is null,
            "transition to no-FX room clears the prior liquid snapshot");

        // Loading a heat-only $FFFF record retains the lava type and BG2/palette owners,
        // but the stale visible liquid plane must remain suppressed.
        fx = RoomLayer3FxState.ForRoom(fxSession, cgram.BackgroundSets, hiddenRoom.State.Fx, door: null, randomNumber: 0);
        Layer3Plane hiddenPlane = EffectPlane(fx);
        fx.PrimeViewport(new Position16<RoomPixels>(0, 0));
        fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
        RgbaPixel[] frame = RenderRoomFx(vram, cgram,
            (fx.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires fx.CaptureForDisplay().")), hiddenPlane);
        AssertTrue(frame.SequenceEqual(RenderRoomFxBackdrop(cgram)),
            "transition to $FFFF heat FX cannot carry visible lava from the source room");
    }

    /// <summary>Loads the named retail room with the exact state pointer found in the FX inventory.</summary>
    private static SelectedRoom LoadRoomByIdentity(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states,
        RoomIdentity identity)
    {
        RetailFxRoomState state = states.First(candidate =>
            SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, candidate.RoomPointer).Identity == identity);
        return SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, state.RoomPointer) with
        {
            State = SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.Load(bus, state.StatePointer),
        };
    }

    /// <summary>Checks reported heat-only and visible-lava rooms retain their classification on every entry path.</summary>
    private static void AssertKnownRetailLiquidExamples(
        Dictionary<RoomIdentity, HashSet<RetailLiquidClassification>> examples)
    {
        foreach (RoomIdentity room in RetailRoomFxDefinitions.HiddenLavaExamples)
        {
            AssertTrue(examples.TryGetValue(room, out HashSet<RetailLiquidClassification>? values) &&
                    values.SetEquals([RetailLiquidClassification.HiddenLavaOrAcid]),
                $"known heat-only room {room} is classified as hidden lava on every entry path");
        }
        foreach (RoomIdentity room in RetailRoomFxDefinitions.VisibleLavaExamples)
        {
            AssertTrue(examples.TryGetValue(room, out HashSet<RetailLiquidClassification>? values) &&
                    values.SetEquals([RetailLiquidClassification.VisibleLavaOrAcid]),
                $"known lava room {room} is classified as visible lava on every entry path");
        }
    }

    /// <summary>Seeds nonzero BG3 colors before FX-specific blends so visibility does not depend on a room graphics set.</summary>
    private static void SeedVisibleRoomFxPalette(GameplayColours cgram)
    {
        // Standard room palettes normally provide these colors before LoadFXHeader applies
        // its optional three-color blend. Seeding every two-bit BG3 palette makes the audit
        // independent of a particular graphics set without masking record-owned overrides.
        for (int color = 1; color < 32; color++)
            cgram.AsPaletteColors().SetColor(color, RgbColor.FromWord(0x7fff));
    }

    /// <summary>Extracts a bank-$8F room and state pair from a named disassembly symbol line.</summary>
    private static RetailFxRoomState? ParseRetailFxRoomState(string line)
    {
        int separator = line.IndexOf(' ');
        const string symbolPrefix = "kRoomState_";
        if (separator < 0 ||
            !line[(separator + 1)..].StartsWith(symbolPrefix, StringComparison.Ordinal) ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int stateAddress))
        {
            return null;
        }

        string symbol = line[(separator + 1)..].Trim();
        if (symbol.Length < symbolPrefix.Length + 9 ||
            !ushort.TryParse(
                symbol.AsSpan(symbolPrefix.Length, 4),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out ushort roomPointer))
        {
            throw new InvalidDataException($"Malformed retail room-state symbol: {line}");
        }

        return new RetailFxRoomState(roomPointer, unchecked((ushort)stateAddress));
    }

    /// <summary>Visible and hidden liquid categories used to compare every retail entry path.</summary>
    internal enum RetailLiquidClassification
    {
        /// <summary>No selected FX record or liquid behavior.</summary>
        None,
        /// <summary>A selected atmospheric or palette effect with no liquid surface.</summary>
        NonLiquidFx,
        /// <summary>Water mechanics selected while BG3 surface art stays hidden.</summary>
        HiddenWater,
        /// <summary>Water mechanics and visible BG3 surface art selected.</summary>
        VisibleWater,
        /// <summary>Lava or acid mechanics selected while the surface art stays hidden.</summary>
        HiddenLavaOrAcid,
        /// <summary>Lava or acid mechanics and visible BG3 surface art selected.</summary>
        VisibleLavaOrAcid,
    }

    /// <summary>Bank-$8F room header and selected state identity parsed from the pinned retail inventory.</summary>
    internal readonly record struct RetailFxRoomState(
        ushort RoomPointer,
        ushort StatePointer);

    /// <summary>Independent expected and production FX fields captured for one physical room-entry path.</summary>
    internal readonly record struct RetailFxPathAudit(
        ushort SelectedRecord,
        RoomFxType ExpectedType,
        RoomFxType ActualType,
        ushort ExpectedBaseY,
        ushort ActualBaseY,
        ushort TargetY,
        ushort Velocity,
        ushort LiquidOptions,
        RetailLiquidClassification Classification);

    /// <summary>Known room, door, and endpoint values for a retail rising-liquid trace.</summary>
    internal readonly record struct RisingLiquidAuditDefinition(
        RoomIdentity Room,
        ushort EntryDoor,
        ushort InitialY,
        ushort TargetY);

    /// <summary>Retail identities and exact cartridge fields asserted by the FX inventory.</summary>
    internal static class RetailRoomFxDefinitions
    {
        /// <summary>Developer debug room excluded by every retail-state audit.</summary>
        public const ushort DebugRoomHeader = 0xe82c;

        /// <summary>Chosen screen-relative surface position for path-independent visibility.</summary>
        public const ushort AuditSurfaceScreenY = 96;

        /// <summary>Frame duration of each lava character set in list <c>$87:8293</c>.</summary>
        public const int LavaSurfaceFrameDuration = 13;

        /// <summary>Guard comfortably above the slowest retail liquid rise.</summary>
        public const int MaximumRiseAuditFrames = 20000;

        /// <summary>Enough frames to traverse both halves of every native tide waveform.</summary>
        public const int TideAuditFrames = 768;

        /// <summary>First full-runtime frame whose room-$28 lava whole Y position changes.</summary>
        public const int Room28FirstMovementFrame = 69;

        /// <summary>Full-runtime frame on which room-$28 lava reaches target Y <c>$00C0</c>.</summary>
        public const int Room28TargetFrame = 637;

        /// <summary>Final frame reached by the draining TSB-fed room-$28 quake timer.</summary>
        public const int Room28LastShakeFrame = 672;

        /// <summary>Applied shake frames from frame one through frame 672 inclusive.</summary>
        public const int Room28ShakeFrameCount = 672;

        /// <summary>Business Center, whose default record exposes lava at world Y $01B1.</summary>
        public static RoomIdentity VisibleLavaRoom { get; } = new(AreaId.Norfair, 0x01);

        /// <summary>Bubble Mountain, whose $FFFF surface intentionally hides lava BG3.</summary>
        public static RoomIdentity HiddenLavaRoom { get; } = new(AreaId.Norfair, 0x03);

        /// <summary>Known false-positive rooms from issue #272.</summary>
        public static IReadOnlySet<RoomIdentity> HiddenLavaExamples { get; } =
            new HashSet<RoomIdentity>
            {
                new(AreaId.Norfair, 0x03),
                new(AreaId.Norfair, 0x1a),
                new(AreaId.Norfair, 0x24),
            };

        /// <summary>Known missing-lava rooms from issue #275.</summary>
        public static IReadOnlySet<RoomIdentity> VisibleLavaExamples { get; } =
            new HashSet<RoomIdentity>
            {
                new(AreaId.Norfair, 0x21),
                new(AreaId.Norfair, 0x28),
            };

        /// <summary>All user-reported rooms whose classifications must be retained.</summary>
        public static HashSet<RoomIdentity> KnownExampleRooms { get; } =
            new HashSet<RoomIdentity>(HiddenLavaExamples.Concat(VisibleLavaExamples));

        /// <summary>
        /// Room $02/$21 entered through door $83:9672: lava starts at $02E0 and rises to
        /// $0260 after timer $40 using velocity $FFF6.
        /// </summary>
        public static RisingLiquidAuditDefinition RisingLavaRoom21 { get; } =
            new(new RoomIdentity(AreaId.Norfair, 0x21), 0x9672, 0x02e0, 0x0260);

        /// <summary>
        /// Room $02/$28 entered through door $83:929A: lava starts at $0108 and rises to
        /// $00C0 after timer $40 using velocity $FFE0.
        /// </summary>
        public static RisingLiquidAuditDefinition RisingLavaRoom28 { get; } =
            new(new RoomIdentity(AreaId.Norfair, 0x28), 0x929a, 0x0108, 0x00c0);
    }

    /// <summary>Runs the full named-state and physical-door FX inventory verification command.</summary>
    [VerificationCommand("--room-fx-retail-inventory")]
    internal static int RoomFxRetailInventoryCommand()
    {
        Suite(nameof(VerifyRetailRoomFxInventory), () => VerifyRetailRoomFxInventory());
        return 0;
    }
}
