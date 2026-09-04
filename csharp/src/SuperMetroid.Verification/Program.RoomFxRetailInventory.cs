using System.Globalization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private const int RetailRoomFxRoomCount = 262;
    private const int RetailRoomFxStateCount = 323;
    private const int RetailRoomFxDoorCount = 597;

    /// <summary>
    /// Exhaustively compares every named retail room state and every physical entry door
    /// with the FX record selected by the production owner. The named-state and physical-
    /// door cardinalities are fixed cartridge facts, so silently omitting an alternate
    /// boss/event state or an unlabelled door header fails the suite.
    /// </summary>
    private static void VerifyRetailRoomFxInventory()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room FX inventory: exhaustive retail audit skipped (private inputs absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailFxRoomState[] states = File.ReadLines(symbolPath)
            .Select(ParseRetailFxRoomState)
            .Where(state => state.HasValue)
            .Select(state => state!.Value)
            .Where(state => state.RoomPointer != RetailRoomFxDefinitions.DebugRoomHeader)
            .DistinctBy(state => state.StatePointer)
            .OrderBy(state => state.RoomPointer)
            .ThenBy(state => state.StatePointer)
            .ToArray();
        ushort[] roomPointers = states
            .Select(state => state.RoomPointer)
            .Distinct()
            .ToArray();
        CartridgeDoorHeader[] doors = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => CartridgeDoorHeader.Load(bus, pointer))
            .ToArray();

        AssertEqual(RetailRoomFxRoomCount, roomPointers.Length,
            "retail room-FX inventory room count");
        AssertEqual(RetailRoomFxStateCount, states.Length,
            "retail room-FX inventory state count");
        AssertEqual(RetailRoomFxDoorCount, doors.Length,
            "retail room-FX inventory physical door count");
        VerifyRetailRoomFxAnimatedTileHeaders(bus);

        var doorByPointer = doors.ToDictionary(door => door.Pointer);
        ILookup<ushort, CartridgeDoorHeader> incomingDoors = doors
            .ToLookup(door => door.DestinationRoomPointer);
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
            CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, namedState.RoomPointer);
            CartridgeRoomHeader room = defaultRoom with
            {
                State = CartridgeRoomState.Load(bus, namedState.StatePointer),
            };
            AssertTrue(auditedStatePointers.Add(room.State.Pointer),
                $"room-FX state $8F:{room.State.Pointer:X4} audited once");

            foreach (ushort recordDoor in EnumerateSpecificFxRecordDoors(
                         bus,
                         room.State.FxPointer))
            {
                doorSpecificRecords++;
                AssertTrue(doorByPointer.TryGetValue(recordDoor, out CartridgeDoorHeader? fxDoor),
                    $"room {room.Identity} state $8F:{room.State.Pointer:X4} FX-specific " +
                    $"door $83:{recordDoor:X4} is a physical retail door");
                AssertEqual(room.Pointer, fxDoor!.DestinationRoomPointer,
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
                        (RoomFxRomData.ReadRecordByte(
                            bus,
                            selectedRecord,
                            RoomFxRomData.Record.LiquidOptionsOffset) &
                            (RoomFxRomData.LiquidTide.SmallTideOption |
                             RoomFxRomData.LiquidTide.LargeTideOption)) != 0)
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
        VerifyRetailLiquidRise(bus, states, doors, RetailRoomFxDefinitions.RisingLavaRoom21);
        VerifyRetailLiquidRise(bus, states, doors, RetailRoomFxDefinitions.RisingLavaRoom28);
        VerifyRetailFxTransitionReset(bus, states);

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

    private static RetailFxPathAudit AuditRetailFxEntryPath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        ushort doorPointer)
    {
        ushort expectedRecord = SelectFxRecordIndependently(
            bus,
            room.State.FxPointer,
            doorPointer);
        RoomFxType expectedType = expectedRecord == 0
            ? RoomFxType.None
            : RoomFxTypes.FromCartridge(
                RoomFxRomData.ReadRecordByte(
                    bus,
                    expectedRecord,
                    RoomFxRomData.Record.TypeOffset),
                $"retail room {room.Identity} state $8F:{room.State.Pointer:X4} " +
                $"entry $83:{doorPointer:X4}");
        ushort expectedBase = expectedRecord == 0
            ? (ushort)0
            : RoomFxRomData.ReadRecordWord(
                bus,
                expectedRecord,
                RoomFxRomData.Record.BaseYPositionOffset);
        ushort expectedTarget = expectedRecord == 0
            ? (ushort)0
            : RoomFxRomData.ReadRecordWord(
                bus,
                expectedRecord,
                RoomFxRomData.Record.TargetYPositionOffset);
        ushort expectedVelocity = expectedRecord == 0
            ? (ushort)0
            : RoomFxRomData.ReadRecordWord(
                bus,
                expectedRecord,
                RoomFxRomData.Record.YVelocityOffset);

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        SeedVisibleRoomFxPalette(cgram);
        var actual = new RoomLayer3FxState();
        actual.Load(
            bus,
            vram,
            cgram,
            room.State.FxPointer,
            doorPointer,
            randomNumber: 0);
        AssertEqual(expectedRecord,
            RoomFxRomData.SelectRecord(bus, room.State.FxPointer, doorPointer),
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} record selection");
        AssertEqual(expectedType, actual.Type,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX type");
        AssertEqual(expectedBase, actual.BaseYPosition,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX base Y");
        AssertEqual(expectedTarget, actual.TargetYPosition,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX target Y");
        AssertEqual(expectedVelocity, actual.PackedYVelocity,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} FX velocity");

        bool liquid = expectedType is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid;
        ushort liquidOptions = actual.LiquidOptions;
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
            actual.BaseYPosition,
            expectedTarget,
            expectedVelocity,
            liquidOptions,
            classification);
        if (!liquid)
            return audit;

        if (unchecked((short)expectedBase) < 0)
        {
            actual.PrimeViewport(cameraX: 0, cameraY: 0);
            actual.Step(bus, vram, cameraX: 0, cameraY: 0, timeIsFrozen: false);
            RoomLayer3FxRenderSnapshot hiddenSnapshot = actual.CaptureForDisplay()
                ?? throw new InvalidDataException(
                    $"Room {room.Identity} negative liquid record $83:{expectedRecord:X4} " +
                    "published no render snapshot.");
            var hiddenFrame = new Rgba32[
                SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
                hiddenFrame,
                vram,
                cgram,
                hiddenSnapshot);
            AssertTrue(hiddenFrame.All(pixel => pixel == default),
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
        actual.PrimeViewport(cameraX: 0, cameraY);
        actual.Step(bus, vram, cameraX: 0, cameraY, timeIsFrozen: false);

        if (expectedType is RoomFxType.Lava or RoomFxType.Acid)
        {
            RoomLayer3FxRenderSnapshot visibleSnapshot = actual.CaptureForDisplay()
                ?? throw new InvalidDataException(
                    $"Room {room.Identity} visible liquid record $83:{expectedRecord:X4} " +
                    "published no render snapshot.");
            var visibleFrame = new Rgba32[
                SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
                visibleFrame,
                vram,
                cgram,
                visibleSnapshot);
            AssertTrue(visibleFrame.Any(pixel => pixel != default),
                $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
                $"$83:{doorPointer:X4} visibly composes {expectedType}");
        }

        return audit;
    }

    private static void VerifyRetailRoomFxAnimatedTileHeaders(SuperMetroidAddressSpace bus)
    {
        VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            AnimatedTileObjectPointers.Lava,
            RoomFxRomData.Layer3AnimatedTiles.LavaFirstInstruction,
            RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            RoomFxRomData.Layer3AnimatedTiles.LavaFirstFrame,
            "lava");
        VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            AnimatedTileObjectPointers.Acid,
            RoomFxRomData.Layer3AnimatedTiles.AcidFirstInstruction,
            RoomFxRomData.Layer3AnimatedTiles.LiquidFrameByteCount,
            RoomFxRomData.Layer3AnimatedTiles.LiquidDestinationWord,
            RoomFxRomData.Layer3AnimatedTiles.AcidFirstFrame,
            "acid");
        VerifyRetailRoomFxAnimatedTileHeader(
            bus,
            AnimatedTileObjectPointers.Rain,
            RoomFxRomData.Layer3AnimatedTiles.RainFirstInstruction,
            RoomFxRomData.Layer3AnimatedTiles.RainFrameByteCount,
            RoomFxRomData.Layer3AnimatedTiles.RainDestinationWord,
            RoomFxRomData.Layer3AnimatedTiles.RainFirstFrame,
            "rain");
    }

    private static void VerifyRetailRoomFxAnimatedTileHeader(
        SuperMetroidAddressSpace bus,
        ushort objectPointer,
        ushort expectedInstruction,
        ushort expectedSize,
        ushort expectedDestination,
        ushort expectedFirstSource,
        string name)
    {
        int header = RoomFxRomData.Banks.AnimatedTiles | objectPointer;
        AssertEqual(expectedInstruction, RomDataReader.ReadWordFixedBank(bus, header),
            $"retail {name} animated-tile instruction list");
        AssertEqual(expectedSize, RomDataReader.ReadWordFixedBank(bus, header + 2),
            $"retail {name} animated-tile frame size");
        AssertEqual(expectedDestination, RomDataReader.ReadWordFixedBank(bus, header + 4),
            $"retail {name} animated-tile VRAM destination");
        AssertEqual(expectedFirstSource,
            RomDataReader.ReadWordFixedBank(
                bus,
                RoomFxRomData.Banks.AnimatedTiles | expectedInstruction + 2),
            $"retail {name} animated-tile first source frame");
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
            ushort candidateDoor = RoomFxRomData.ReadRecordWord(
                bus,
                cursor,
                RoomFxRomData.Record.DoorPointerOffset);
            if (candidateDoor == 0 || candidateDoor == doorPointer)
                return cursor;
            if (candidateDoor == RoomFxRomData.Record.TerminatorDoorPointer)
                return 0;
            cursor = unchecked((ushort)(cursor + RoomFxRomData.Record.ByteCount));
        }

        throw new InvalidDataException(
            $"Independent room-FX selector did not terminate list $83:{listPointer:X4}.");
    }

    private static IEnumerable<ushort> EnumerateSpecificFxRecordDoors(
        SuperMetroidAddressSpace bus,
        ushort listPointer)
    {
        if (listPointer == 0)
            yield break;

        ushort cursor = listPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort doorPointer = RoomFxRomData.ReadRecordWord(
                bus,
                cursor,
                RoomFxRomData.Record.DoorPointerOffset);
            if (doorPointer == 0 || doorPointer == RoomFxRomData.Record.TerminatorDoorPointer)
                yield break;
            yield return doorPointer;
            cursor = unchecked((ushort)(cursor + RoomFxRomData.Record.ByteCount));
        }

        throw new InvalidDataException(
            $"Room-FX inventory did not terminate list $83:{listPointer:X4}.");
    }

    private static void VerifyRetailLiquidRise(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states,
        IReadOnlyList<CartridgeDoorHeader> doors,
        RisingLiquidAuditDefinition definition)
    {
        RetailFxRoomState namedState = states.Single(state =>
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.Load(bus, state.RoomPointer);
            return header.Identity == definition.Room;
        });
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, namedState.RoomPointer) with
        {
            State = CartridgeRoomState.Load(bus, namedState.StatePointer),
        };
        CartridgeDoorHeader door = doors.Single(candidate =>
            candidate.Pointer == definition.EntryDoor);
        AssertEqual(room.Pointer, door.DestinationRoomPointer,
            $"rising-lava door $83:{door.Pointer:X4} destination");

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        SeedVisibleRoomFxPalette(cgram);
        var fx = new RoomLayer3FxState();
        fx.Load(bus, vram, cgram, room.State.FxPointer, door.Pointer, randomNumber: 0);
        AssertEqual(RoomFxType.Lava, fx.Type, $"room {definition.Room} rising FX type");
        AssertEqual(definition.InitialY, fx.BaseYPosition,
            $"room {definition.Room} initial lava Y");
        AssertEqual(definition.TargetY, fx.TargetYPosition,
            $"room {definition.Room} target lava Y");

        ushort cameraY = unchecked((ushort)(door.DestinationScreenY * 256));
        fx.PrimeViewport(cameraX: 0, cameraY);
        AssertTrue(fx.CaptureForDisplay()!.Value.WaterSurfaceScreenY >=
                SnesGameplayFrameRenderer.Height,
            $"room {definition.Room} entry lava begins below the viewport");

        int movingFrames = 0;
        for (; movingFrames < RetailRoomFxDefinitions.MaximumRiseAuditFrames; movingFrames++)
        {
            fx.Step(bus, vram, cameraX: 0, cameraY, timeIsFrozen: false);
            if (fx.PackedYVelocity == 0)
                break;
        }
        AssertTrue(movingFrames < RetailRoomFxDefinitions.MaximumRiseAuditFrames,
            $"room {definition.Room} lava reaches its cartridge target");
        AssertEqual(definition.TargetY, fx.BaseYPosition,
            $"room {definition.Room} lava stops at its cartridge target");

        RoomLayer3FxRenderSnapshot finalSnapshot = fx.CaptureForDisplay()
            ?? throw new InvalidDataException(
                $"Room {definition.Room} rising lava published no final snapshot.");
        AssertTrue(finalSnapshot.WaterSurfaceScreenY >= SnesGameplayFrameRenderer.HudHeight &&
                finalSnapshot.WaterSurfaceScreenY < SnesGameplayFrameRenderer.Height,
            $"room {definition.Room} risen lava enters the gameplay viewport");
        var frame = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
        SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(frame, vram, cgram, finalSnapshot);
        AssertTrue(frame.Any(pixel => pixel != default),
            $"room {definition.Room} risen lava is visibly composed");
    }

    private static void VerifyRetailLiquidTide(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        ushort doorPointer)
    {
        var fx = new RoomLayer3FxState();
        var vram = new SnesVram();
        fx.Load(
            bus,
            vram,
            new SnesCgram(),
            room.State.FxPointer,
            doorPointer,
            randomNumber: 0);
        fx.PrimeViewport(cameraX: 0, cameraY: 0);

        int maximumAbsoluteOffset = 0;
        for (int frame = 0; frame < RetailRoomFxDefinitions.TideAuditFrames; frame++)
        {
            // A fresh VRAM object is sufficient here because the assertion targets the
            // liquid position, not animation output. The production animated-tile owner
            // still executes and therefore remains covered by fail-loud dispatch.
            fx.Step(
                bus,
                vram,
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false);
            int offset = unchecked((short)(fx.CurrentYPosition - fx.BaseYPosition));
            maximumAbsoluteOffset = Math.Max(maximumAbsoluteOffset, Math.Abs(offset));
        }

        int expectedMaximum =
            (fx.LiquidOptions & RoomFxRomData.LiquidTide.SmallTideOption) != 0
                ? RoomFxRomData.LiquidTide.SmallTideScale
                : RoomFxRomData.LiquidTide.LargeTideScale;
        AssertTrue(maximumAbsoluteOffset > 0 && maximumAbsoluteOffset <= expectedMaximum,
            $"room {room.Identity} state $8F:{room.State.Pointer:X4} entry " +
            $"$83:{doorPointer:X4} tide moves within ±{expectedMaximum} pixels");
    }

    private static void VerifyRetailFxTransitionReset(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states)
    {
        CartridgeRoomHeader visibleRoom = LoadRoomByIdentity(
            bus,
            states,
            RetailRoomFxDefinitions.VisibleLavaRoom);
        CartridgeRoomHeader hiddenRoom = LoadRoomByIdentity(
            bus,
            states,
            RetailRoomFxDefinitions.HiddenLavaRoom);

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        SeedVisibleRoomFxPalette(cgram);
        var fx = new RoomLayer3FxState();
        fx.Load(bus, vram, cgram, visibleRoom.State.FxPointer, doorPointer: 0, randomNumber: 0);
        fx.PrimeViewport(cameraX: 0, cameraY: 0);
        fx.Step(bus, vram, cameraX: 0, cameraY: 0, timeIsFrozen: false);
        AssertEqual(RoomFxType.Lava, fx.Type, "transition source owns visible lava");

        // Loading a no-FX room must clear the owner even though the old tilemap and
        // animated characters remain in VRAM until another producer overwrites them.
        fx.Load(bus, vram, cgram, fxPointer: 0, doorPointer: 0, randomNumber: 0);
        AssertTrue(fx.CaptureForDisplay() is null,
            "transition to no-FX room clears the prior liquid snapshot");

        // Loading a heat-only $FFFF record retains the lava type and BG2/palette owners,
        // but the stale visible liquid plane must remain suppressed.
        fx.Load(bus, vram, cgram, hiddenRoom.State.FxPointer, doorPointer: 0, randomNumber: 0);
        fx.PrimeViewport(cameraX: 0, cameraY: 0);
        fx.Step(bus, vram, cameraX: 0, cameraY: 0, timeIsFrozen: false);
        var frame = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
        SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
            frame,
            vram,
            cgram,
            fx.CaptureForDisplay()!.Value);
        AssertTrue(frame.All(pixel => pixel == default),
            "transition to $FFFF heat FX cannot carry visible lava from the source room");
    }

    private static CartridgeRoomHeader LoadRoomByIdentity(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<RetailFxRoomState> states,
        RoomIdentity identity)
    {
        RetailFxRoomState state = states.First(candidate =>
            CartridgeRoomHeader.Load(bus, candidate.RoomPointer).Identity == identity);
        return CartridgeRoomHeader.Load(bus, state.RoomPointer) with
        {
            State = CartridgeRoomState.Load(bus, state.StatePointer),
        };
    }

    private static void AssertKnownRetailLiquidExamples(
        IReadOnlyDictionary<RoomIdentity, HashSet<RetailLiquidClassification>> examples)
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

    private static void SeedVisibleRoomFxPalette(SnesCgram cgram)
    {
        // Standard room palettes normally provide these colors before LoadFXHeader applies
        // its optional three-color blend. Seeding every two-bit BG3 palette makes the audit
        // independent of a particular graphics set without masking record-owned overrides.
        for (int color = 1; color < 32; color++)
            cgram.SetColor(color, 0x7fff);
    }

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

    private enum RetailLiquidClassification
    {
        None,
        NonLiquidFx,
        HiddenWater,
        VisibleWater,
        HiddenLavaOrAcid,
        VisibleLavaOrAcid,
    }

    private readonly record struct RetailFxRoomState(
        ushort RoomPointer,
        ushort StatePointer);

    private readonly record struct RetailFxPathAudit(
        ushort SelectedRecord,
        RoomFxType ExpectedType,
        RoomFxType ActualType,
        ushort ExpectedBaseY,
        ushort ActualBaseY,
        ushort TargetY,
        ushort Velocity,
        ushort LiquidOptions,
        RetailLiquidClassification Classification);

    private readonly record struct RisingLiquidAuditDefinition(
        RoomIdentity Room,
        ushort EntryDoor,
        ushort InitialY,
        ushort TargetY);

    /// <summary>Retail identities and exact cartridge fields asserted by the FX inventory.</summary>
    private static class RetailRoomFxDefinitions
    {
        /// <summary>Developer debug room excluded by every retail-state audit.</summary>
        public const ushort DebugRoomHeader = 0xe82c;

        /// <summary>Chosen screen-relative surface position for path-independent visibility.</summary>
        public const ushort AuditSurfaceScreenY = 96;

        /// <summary>Guard comfortably above the slowest retail liquid rise.</summary>
        public const int MaximumRiseAuditFrames = 20000;

        /// <summary>Enough frames to traverse both halves of every native tide waveform.</summary>
        public const int TideAuditFrames = 768;

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
}
