using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;
using static AssertionsVerification;

/// <summary>Represents spatial debugger migrations verification.</summary>
internal static class SpatialDebuggerMigrationsVerification
{
    /// <summary>
    /// Confirms the #628 debugger migrations of the camera, background-scroll and door-opening
    /// owners: an older capture's parallel scalar words restore into the typed spatial values
    /// exactly, a capture missing one component is rejected, and current graphs round-trip.
    /// </summary>
    private static void VerifySpatialDebuggerMigrations()
    {
        var camera = (ScrollBoundaryCamera)RuntimeHelpers.GetUninitializedObject(typeof(ScrollBoundaryCamera));
        (string, object)[] cameraWords =
        [
            ("<XPosition>k__BackingField", (ushort)0x0123), ("<XSubposition>k__BackingField", (ushort)0x4567),
            ("<YPosition>k__BackingField", (ushort)0x0890), ("<YSubposition>k__BackingField", (ushort)0xabcd),
            ("<IdealXPosition>k__BackingField", (ushort)0x0111), ("<IdealYPosition>k__BackingField", (ushort)0x0222),
            ("<CameraXSpeed>k__BackingField", (ushort)0x0002), ("<CameraXSubspeed>k__BackingField", (ushort)0x8000),
            ("<CameraYSpeed>k__BackingField", (ushort)0xffff), ("<CameraYSubspeed>k__BackingField", (ushort)0x0001),
        ];
        string[] cameraFields = ["<Position>k__BackingField", "<IdealPosition>k__BackingField", "<Speed>k__BackingField",
            "<PreviousSamusPoint>k__BackingField"];
        var restoredCamera = (ScrollBoundaryCamera)RestoreSpatialLegacy(camera, cameraFields, cameraWords,
            retiredCheckpoint: ("<PreviousSamusPoint>k__BackingField", [0x0201, 0x1234, 0x0405, 0xfedc]));
        AssertEqual(FixedPosition<RoomPixels>.FromWords(0x0123, 0x4567, 0x0890, 0xabcd), restoredCamera.Position,
            "legacy camera words restore one 16.16 layer-one position");
        AssertEqual(new Position16<RoomPixels>(0x0111, 0x0222), restoredCamera.IdealPosition,
            "legacy ideal camera words restore one position");
        AssertEqual(FixedVector<RoomPixels>.FromWords(0x0002, 0x8000, 0xffff, 0x0001), restoredCamera.Speed,
            "legacy speed words restore their exact native word pattern");
        AssertEqual<FixedPosition<RoomPixels>?>(FixedPosition<RoomPixels>.FromWords(0x0201, 0x1234, 0x0405, 0xfedc),
            restoredCamera.PreviousSamusPoint, "retired SamusCameraPoint restores as the typed checkpoint");
        AssertThrows<InvalidDataException>(() => RestoreSpatialLegacy(camera, cameraFields,
                cameraWords.Where(word => word.Item1 != "<YSubposition>k__BackingField").ToArray()),
            "a legacy camera missing one component is rejected rather than defaulted");

        var scroll = new BackgroundScrollState();
        (string, object)[] scrollWords =
        [
            ("<Layer1XPosition>k__BackingField", (ushort)0x1234), ("<Layer1YPosition>k__BackingField", (ushort)0x0200),
            ("<Layer2XPosition>k__BackingField", (ushort)0x091a), ("<Layer2YPosition>k__BackingField", (ushort)0x0080),
            ("<Layer2ScrollX>k__BackingField", (byte)0x81), ("<Layer2ScrollY>k__BackingField", (byte)1),
            ("<Bg1XOffset>k__BackingField", (ushort)0x0010), ("<Bg1YOffset>k__BackingField", (ushort)0xfff0),
            ("<Bg2XOffset>k__BackingField", (ushort)0xff00), ("<Bg2YOffset>k__BackingField", (ushort)0),
            ("<Bg1HorizontalScroll>k__BackingField", (ushort)0x1244), ("<Bg1VerticalScroll>k__BackingField", (ushort)0x01f0),
            ("<Bg2HorizontalScroll>k__BackingField", (ushort)0x081a), ("<Bg2VerticalScroll>k__BackingField", (ushort)0x0080),
            ("<Bg1XBlock>k__BackingField", (ushort)0x0124), ("<Bg1YBlock>k__BackingField", (ushort)0x001f),
            ("<Bg2XBlock>k__BackingField", (ushort)0x0081), ("<Bg2YBlock>k__BackingField", (ushort)0x0008),
            ("<Layer1XBlock>k__BackingField", (ushort)0x0123), ("<Layer1YBlock>k__BackingField", (ushort)0x0020),
            ("<Layer2XBlock>k__BackingField", (ushort)0x0091), ("<Layer2YBlock>k__BackingField", (ushort)0x0008),
            ("<PreviousLayer1XBlock>k__BackingField", (ushort)0xffff), ("<PreviousLayer1YBlock>k__BackingField", (ushort)0x001f),
            ("<PreviousLayer2XBlock>k__BackingField", (ushort)0x0090), ("<PreviousLayer2YBlock>k__BackingField", (ushort)0x0007),
        ];
        string[] scrollFields = ["<Layer1Position>k__BackingField", "<Layer2Position>k__BackingField",
            "<Layer2Scroll>k__BackingField", "<Bg1Offset>k__BackingField", "<Bg2Offset>k__BackingField",
            "<Bg1Scroll>k__BackingField", "<Bg2Scroll>k__BackingField", "<Bg1Block>k__BackingField",
            "<Bg2Block>k__BackingField", "<Layer1Block>k__BackingField", "<Layer2Block>k__BackingField",
            "<PreviousLayer1Block>k__BackingField", "<PreviousLayer2Block>k__BackingField"];
        var restoredScroll = (BackgroundScrollState)RestoreSpatialLegacy(scroll, scrollFields, scrollWords);
        AssertEqual(new Position16<RoomPixels>(0x1234, 0x0200), restoredScroll.Layer1Position, "legacy layer-one words");
        AssertEqual(new Position16<Bg2Pixels>(0x091a, 0x0080), restoredScroll.Layer2Position, "legacy layer-two words");
        AssertEqual(new Layer2ScrollModes(0x81, 1), restoredScroll.Layer2Scroll, "legacy per-axis scroll modes");
        AssertEqual(new Offset16<RoomPixels>(0x0010, -0x0010), restoredScroll.Bg1Offset, "legacy BG1 offset words keep their wrapped pattern");
        AssertEqual(new Offset16<Bg2Pixels>(-0x0100, 0), restoredScroll.Bg2Offset, "legacy BG2 offset words keep their wrapped pattern");
        AssertEqual(new Position16<RoomPixels>(0x1244, 0x01f0), restoredScroll.Bg1Scroll, "legacy BG1 mirrors");
        AssertEqual(new Position16<Bg2Pixels>(0x081a, 0x0080), restoredScroll.Bg2Scroll, "legacy BG2 mirrors");
        AssertEqual(new Position16<TilemapBlocks>(0x0124, 0x001f), restoredScroll.Bg1Block, "legacy BG1 tilemap blocks");
        AssertEqual(new Position16<TilemapBlocks>(0x0081, 0x0008), restoredScroll.Bg2Block, "legacy BG2 tilemap blocks");
        AssertEqual(new Position16<RoomBlocks>(0x0123, 0x0020), restoredScroll.Layer1Block, "legacy layer-one blocks");
        AssertEqual(new Position16<RoomBlocks>(0x0091, 0x0008), restoredScroll.Layer2Block, "legacy layer-two blocks");
        AssertEqual(new Position16<RoomBlocks>(0xffff, 0x001f), restoredScroll.PreviousLayer1Block, "legacy previous layer-one blocks");
        AssertEqual(new Position16<RoomBlocks>(0x0090, 0x0007), restoredScroll.PreviousLayer2Block, "legacy previous layer-two blocks");

        // The old magnitude-only Samus step takes its sign and axis from the saved direction.
        Type doorType = typeof(DoorDirection).Assembly.GetType("SuperMetroid.Core.Runtime.DoorOpeningScrollState", throwOnError: true)!;
        foreach (DoorDirection direction in Enum.GetValues<DoorDirection>())
        {
            object door = RuntimeHelpers.GetUninitializedObject(doorType);
            doorType.GetField("<Direction>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(door, direction);
            (string, object)[] doorWords =
            [
                ("<SamusStep>k__BackingField", 0x0000_c800u),
                ("<CameraX>k__BackingField", (ushort)0x0204), ("<CameraY>k__BackingField", (ushort)0xff20),
                ("<Layer2X>k__BackingField", (ushort)0x0102), ("<Layer2Y>k__BackingField", (ushort)0x0090),
                ("<SamusXFixed>k__BackingField", 0x0218_4000u), ("<SamusYFixed>k__BackingField", 0xff8b_8000u),
                ("<FinalCameraX>k__BackingField", (ushort)0x0300), ("<FinalCameraY>k__BackingField", (ushort)0x0220),
                ("<FinalLayer2X>k__BackingField", (ushort)0x0180), ("<FinalLayer2Y>k__BackingField", (ushort)0x0110),
                ("<FinalSamusXFixed>k__BackingField", 0x0318_4000u), ("<FinalSamusYFixed>k__BackingField", 0x0288_8000u),
            ];
            string[] doorFields = ["<SamusIrqStep>k__BackingField", "<CameraPosition>k__BackingField",
                "<Layer2Position>k__BackingField", "<SamusPosition>k__BackingField", "<FinalCameraPosition>k__BackingField",
                "<FinalLayer2Position>k__BackingField", "<FinalSamusPosition>k__BackingField"];
            object restoredDoor = RestoreSpatialLegacy(door, doorFields, doorWords);
            object? Field(string name) => doorType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(restoredDoor);
            var step = SnesSignedSixteenSixteen.FromRaw(direction is DoorDirection.Right or DoorDirection.Down ? 0xc800 : -0xc800);
            FixedVector<RoomPixels> expectedStep = direction is DoorDirection.Right or DoorDirection.Left
                ? new(step, SnesSignedSixteenSixteen.Zero)
                : new(SnesSignedSixteenSixteen.Zero, step);
            AssertEqual<object?>(expectedStep, Field("<SamusIrqStep>k__BackingField"), $"legacy {direction} door step orientation");
            AssertEqual<object?>(new Position16<RoomPixels>(0x0204, 0xff20), Field("<CameraPosition>k__BackingField"), $"legacy {direction} door camera");
            AssertEqual<object?>(new Position16<Bg2Pixels>(0x0102, 0x0090), Field("<Layer2Position>k__BackingField"), $"legacy {direction} door layer two");
            AssertEqual<object?>(FixedPosition<RoomPixels>.FromWords(0x0218, 0x4000, 0xff8b, 0x8000), Field("<SamusPosition>k__BackingField"),
                $"legacy {direction} door Samus 16.16 words");
            AssertEqual<object?>(new Position16<RoomPixels>(0x0300, 0x0220), Field("<FinalCameraPosition>k__BackingField"), $"legacy {direction} final camera");
            AssertEqual<object?>(new Position16<Bg2Pixels>(0x0180, 0x0110), Field("<FinalLayer2Position>k__BackingField"), $"legacy {direction} final layer two");
            AssertEqual<object?>(FixedPosition<RoomPixels>.FromWords(0x0318, 0x4000, 0x0288, 0x8000), Field("<FinalSamusPosition>k__BackingField"),
                $"legacy {direction} final Samus");
        }

        // Current layouts round-trip every typed component, including fractions and wrapped words.
        var current = new BackgroundScrollState
        {
            Layer1Position = new(0xff04, 0x02fb),
            Layer2Position = new(0x0102, 0x01dc),
            Layer2Scroll = new(0x80, 0x41),
            Bg1Offset = new(-0x7fff, 0x0123),
            Bg2Offset = new(0x0100, -1),
        };
        current.PrimePreviousBlocks();
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, current);
        bytes.Position = 0;
        var roundTrip = DebuggerObjectGraphSerializer.Deserialize<BackgroundScrollState>(bytes);
        AssertEqual((current.Layer1Position, current.Layer2Position, current.Layer2Scroll, current.Bg1Offset, current.Bg2Offset,
                current.Bg1Scroll, current.Bg2Scroll, current.Bg1Block, current.Bg2Block, current.Layer1Block, current.Layer2Block,
                current.PreviousLayer1Block, current.PreviousLayer2Block),
            (roundTrip.Layer1Position, roundTrip.Layer2Position, roundTrip.Layer2Scroll, roundTrip.Bg1Offset, roundTrip.Bg2Offset,
                roundTrip.Bg1Scroll, roundTrip.Bg2Scroll, roundTrip.Bg1Block, roundTrip.Bg2Block, roundTrip.Layer1Block, roundTrip.Layer2Block,
                roundTrip.PreviousLayer1Block, roundTrip.PreviousLayer2Block),
            "background-scroll typed values round-trip through the debugger codec");
        VerifyRenderAndFxSpatialDebuggerMigrations();
        VerifyMotherBrainNeckPointMigration();
        Console.WriteLine("  Spatial debugger migrations: camera, scroll, door, FX and display-packet legacy words restore exactly; current layouts round-trip.");
    }

    /// <summary>
    /// The second #628 slice: scroll-grid size, BG3 room-FX scroll state, the Ceres Mode 7 pivot,
    /// room-shake displacements and every display-packet record a retained capture can hold.
    /// </summary>
    private static void VerifyRenderAndFxSpatialDebuggerMigrations()
    {
        RoomScrollGrid grid = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), new byte[RoomScrollGrid.StorageByteCount],
            new Extent16<RoomScreens>(1, 1));
        var restoredGrid = (RoomScrollGrid)RestoreSpatialLegacy(grid, ["<Size>k__BackingField"],
            [("<WidthInScreens>k__BackingField", 5), ("<HeightInScreens>k__BackingField", 3)]);
        AssertEqual(new Extent16<RoomScreens>(5, 3), restoredGrid.Size, "legacy scroll-grid screen counts restore one size");

        var fx = new RoomLayer3FxState();
        var restoredFx = (RoomLayer3FxState)RestoreSpatialLegacy(fx,
            ["<Scroll>k__BackingField", "drift", "previousCamera"],
            [
                ("<HorizontalScroll>k__BackingField", (ushort)0xfff3), ("<VerticalScroll>k__BackingField", (ushort)0x0123),
                ("horizontalAccumulator", (ushort)0x8040), ("verticalAccumulator", (ushort)0x7fc0),
                ("previousCameraX", (ushort)0x0400), ("previousCameraY", (ushort)0x0200),
            ]);
        AssertEqual(new Position16<Bg3Pixels>(0xfff3, 0x0123), restoredFx.Scroll, "legacy BG3 scroll words");
        AssertEqual<object?>(new Offset16<Bg3Subpixels>(unchecked((short)0x8040), 0x7fc0),
            PrivateState.Field<Offset16<Bg3Subpixels>>(restoredFx, "drift"), "legacy 8.8 drift accumulators keep their words");
        AssertEqual<object?>(new Position16<RoomPixels>(0x0400, 0x0200),
            PrivateState.Field<Position16<RoomPixels>>(restoredFx, "previousCamera"), "legacy rain camera checkpoint");

        object transform = RestoreSpatialLegacy(new SamusMode7Transform(0x0100, 0, 0, default), ["<Center>k__BackingField"],
            [("<CenterX>k__BackingField", (ushort)0x0080), ("<CenterY>k__BackingField", (ushort)0x03f0)]);
        AssertEqual(new SamusMode7Transform(0x0100, 0, 0, new Position16<RoomPixels>(0x0080, 0x03f0)), (SamusMode7Transform)transform,
            "legacy Mode 7 centre words restore the shaft pivot");

        object shake = RestoreSpatialLegacy(default(RoomShakeFrameResult), ["<Bg1>k__BackingField", "<Bg2>k__BackingField"],
            [("<Bg1X>k__BackingField", (short)-2), ("<Bg1Y>k__BackingField", (short)1),
             ("<Bg2X>k__BackingField", (short)3), ("<Bg2Y>k__BackingField", (short)-3)]);
        AssertEqual(new RoomShakeFrameResult(new(-2, 1), new(3, -3)), (RoomShakeFrameResult)shake, "legacy signed shake words");

        object mode7 = RestoreSpatialLegacy(new Mode7RenderRegisters(0x100, 1, -1, 0x100, default, default),
            ["<Center>k__BackingField", "<Scroll>k__BackingField"],
            [("<CenterX>k__BackingField", (short)128), ("<CenterY>k__BackingField", (short)-4096),
             ("<HorizontalOffset>k__BackingField", (short)-13), ("<VerticalOffset>k__BackingField", (short)4095)]);
        AssertEqual(new Mode7RenderRegisters(0x100, 1, -1, 0x100, new(128, -4096), new(-13, 4095)), (Mode7RenderRegisters)mode7,
            "legacy Mode 7 centre and scroll registers keep their signed words");

        var ordinary = new OrdinaryGameplayRegisters(default, default, default, 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1);
        object restoredOrdinary = RestoreSpatialLegacy(ordinary,
            ["<Bg1Scroll>k__BackingField", "<Bg2Scroll>k__BackingField", "<Bg2MapSize>k__BackingField"],
            [("<Bg1X>k__BackingField", (ushort)0x0511), ("<Bg1Y>k__BackingField", (ushort)0xffe0),
             ("<Bg2X>k__BackingField", (ushort)0x0288), ("<Bg2Y>k__BackingField", (ushort)0x0010),
             ("<Bg2WidthTiles>k__BackingField", 32), ("<Bg2HeightTiles>k__BackingField", 64)]);
        AssertEqual(ordinary with
            {
                Bg1Scroll = new(0x0511, 0xffe0), Bg2Scroll = new(0x0288, 0x0010), Bg2MapSize = new(32, 64),
            }, (OrdinaryGameplayRegisters)restoredOrdinary, "legacy gameplay scroll registers and BG2 map size");

        var bg4 = new Bg4BppRenderLayer(0x7000, 0x6000, default, default, true);
        var restoredBg4 = (Bg4BppRenderLayer)RestoreSpatialLegacy(bg4, ["<Scroll>k__BackingField", "<MapSize>k__BackingField"],
            [("<HorizontalScroll>k__BackingField", (ushort)0xfffe), ("<VerticalScroll>k__BackingField", (ushort)0x01ff),
             ("<MapWidthTiles>k__BackingField", 64), ("<MapHeightTiles>k__BackingField", 32)]);
        AssertEqual(bg4 with { Scroll = new(0xfffe, 0x01ff), MapSize = new(64, 32) }, restoredBg4, "legacy 4-bpp layer scroll and map size");

        var floor = new Mode1FloorBand(208, 0x4800, 0x6000, default, default);
        object restoredFloor = RestoreSpatialLegacy(floor, ["<Scroll>k__BackingField", "<MapSize>k__BackingField"],
            [("<HorizontalScroll>k__BackingField", (ushort)0x0101), ("<VerticalScroll>k__BackingField", (ushort)0x0202),
             ("<MapWidthTiles>k__BackingField", 64), ("<MapHeightTiles>k__BackingField", 32)]);
        AssertEqual(floor with { Scroll = new(0x0101, 0x0202), MapSize = new(64, 32) }, (Mode1FloorBand)restoredFloor,
            "legacy Mode 1 floor band scroll and map size");

        var packet = new RenderFrameSnapshot(new(1, 1, 0), new SuperMetroid.Core.Assets.Rgba32(1, 2, 3));
        var restoredPacket = (RenderFrameSnapshot)RestoreSpatialLegacy(packet, ["<Size>k__BackingField"],
            [("<Width>k__BackingField", 256), ("<Height>k__BackingField", 224)]);
        AssertEqual(SnesPpuLayout.ScreenSize, restoredPacket.Size, "legacy packet width and height restore the native size");
        AssertThrows<InvalidDataException>(() => RestoreSpatialLegacy(packet, ["<Size>k__BackingField"],
                [("<Width>k__BackingField", -1), ("<Height>k__BackingField", 224)]),
            "a legacy negative size is rejected rather than wrapped");

        // Per-line scroll registers were arrays of the retired BackgroundLineScroll pair.
        var lines = Enumerable.Range(0, SnesPpuLayout.ScreenHeightPixels)
            .Select(line => new Position16<BgPlanePixels>(unchecked((ushort)(0xfff0 + line)), (ushort)(line * 3))).ToArray();
        var subscreen = new BgSubscreenAddRenderLayer(0x5000, 0x4000, FourBpp: true);
        var restoredSubscreen = (BgSubscreenAddRenderLayer)RestoreSpatialLegacy(subscreen, ["scrolls"],
            [("scrolls", new RetiredLineScrolls(lines))]);
        AssertTrue(restoredSubscreen.Scrolls.SequenceEqual(lines), "retired BackgroundLineScroll arrays restore every typed line register");

        // Current display records round-trip through the production codec unchanged.
        BgSubscreenAddRenderLayer current = subscreen.WithScrolls(lines);
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, current);
        bytes.Position = 0;
        var roundTrip = DebuggerObjectGraphSerializer.Deserialize<BgSubscreenAddRenderLayer>(bytes);
        AssertTrue(roundTrip.Scrolls.SequenceEqual(lines), "typed line registers round-trip through the debugger codec");
    }

    /// <summary>
    /// Mother Brain's five neck joints were stored as the retired <c>MotherBrainNeckPoint</c> pair;
    /// an older capture's joints restore into the typed room positions word for word.
    /// </summary>
    private static void VerifyMotherBrainNeckPointMigration()
    {
        var state = new MotherBrainEnemyState(new RoomEnemySlot(0));
        // Restoration validates closed domains (#627); give the fresh encounter's enum fields a
        // defined member, as a live capture always has.
        for (Type? owner = state.GetType(); owner is not null; owner = owner.BaseType)
            foreach (FieldInfo member in owner.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (member.FieldType.IsEnum && !Enum.IsDefined(member.FieldType, member.GetValue(state)!))
                    member.SetValue(state, Enum.GetValues(member.FieldType).GetValue(0));
        string[] joints = Enumerable.Range(0, 5).Select(index => $"<NeckSegment{index}>k__BackingField").ToArray();
        Position16<RoomPixels>[] expected = Enumerable.Range(0, 5)
            .Select(index => new Position16<RoomPixels>(unchecked((ushort)(0xfff0 + index)), (ushort)(0x0100 * index + 7))).ToArray();
        var restored = (MotherBrainEnemyState)RestoreSpatialLegacy(state, joints,
            joints.Zip(expected, (joint, point) => (joint, (object)new RetiredNeckPoint(point))).ToArray());
        Position16<RoomPixels>[] actual =
            [restored.NeckSegment0, restored.NeckSegment1, restored.NeckSegment2, restored.NeckSegment3, restored.NeckSegment4];
        AssertTrue(expected.SequenceEqual(actual), "retired MotherBrainNeckPoint joints restore as typed room positions");
    }

    /// <summary>A legacy field payload: one retired <c>MotherBrainNeckPoint</c>, written in its historical shape.</summary>
    internal sealed record RetiredNeckPoint(Position16<RoomPixels> Point);

    /// <summary>A legacy field payload: an array of the retired per-line scroll pair, written in its historical shape.</summary>
    internal sealed record RetiredLineScrolls(Position16<BgPlanePixels>[] Lines);

    /// <summary>Writes retired neck point.</summary>
    private static void WriteRetiredNeckPoint(BinaryWriter writer, Stream data, Position16<RoomPixels> point)
    {
        const byte newObjectMarker = 2, fieldPayloadKind = 5;
        const string retired = "SuperMetroid.Core.Game.MotherBrainNeckPoint, SuperMetroid.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        writer.Write(newObjectMarker);
        writer.Write(0);
        writer.Write(retired);
        writer.Write(fieldPayloadKind);
        writer.Write(2);
        foreach ((string name, ushort value) in new[] { ("<X>k__BackingField", point.X), ("<Y>k__BackingField", point.Y) })
        {
            writer.Write(retired);
            writer.Write(name);
            writer.Flush();
            DebuggerObjectGraphSerializer.Serialize(data, value);
        }
    }

    /// <summary>Writes retired line scrolls.</summary>
    private static void WriteRetiredLineScrolls(BinaryWriter writer, Stream data, Position16<BgPlanePixels>[] lines)
    {
        const byte newObjectMarker = 2, arrayPayloadKind = 3, fieldPayloadKind = 5;
        const string assembly = ", SuperMetroid.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        const string element = "SuperMetroid.Core.Rendering.BackgroundLineScroll";
        writer.Write(newObjectMarker);
        writer.Write(0x4000_0000); // A reference id no other object in the envelope uses.
        writer.Write(element + "[]" + assembly);
        writer.Write(arrayPayloadKind);
        writer.Write(1);
        writer.Write(lines.Length);
        writer.Write(0);
        foreach (Position16<BgPlanePixels> line in lines)
        {
            writer.Write(newObjectMarker);
            writer.Write(0);
            writer.Write(element + assembly);
            writer.Write(fieldPayloadKind);
            writer.Write(2);
            foreach ((string name, ushort value) in new[] { ("<X>k__BackingField", line.X), ("<Y>k__BackingField", line.Y) })
            {
                writer.Write(element + assembly);
                writer.Write(name);
                writer.Flush();
                DebuggerObjectGraphSerializer.Serialize(data, value);
            }
        }
    }

    /// <summary>
    /// Writes and restores the older field envelope of <paramref name="instance"/>: every current
    /// field except <paramref name="typedFields"/>, plus <paramref name="legacy"/> scalar fields in
    /// their place, all with the production codec. A retired <c>SamusCameraPoint</c> value is
    /// written under its historical identity.
    /// </summary>
    private static object RestoreSpatialLegacy(object instance, string[] typedFields, (string Name, object Value)[] legacy,
        (string Field, ushort[] Words)? retiredCheckpoint = null)
    {
        const byte newObjectMarker = 2, fieldPayloadKind = 5;
        Type type = instance.GetType();
        var fields = ((FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
                BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [type])!)
            .Where(field => !typedFields.Contains(field.Name)).ToArray();
        string typeName = DebuggerStateTypeIdentity.GetSerializedName(type);
        using var data = new MemoryStream();
        using (var writer = new BinaryWriter(data, Encoding.UTF8, leaveOpen: true))
        {
            // One production graph writer serializes every field value, so reference fields
            // receive distinct ids as they do inside a real capture. The envelope's own id lies
            // above anything that writer assigns.
            Type graphWriterType = typeof(DebuggerObjectGraphSerializer).GetNestedType("GraphWriter", BindingFlags.NonPublic)!;
            object graph = Activator.CreateInstance(graphWriterType, writer)!;
            MethodInfo writeValue = graphWriterType.GetMethod("Write")!;
            void Value(object? value) => writeValue.Invoke(graph, [value]);
            writer.Write(newObjectMarker);
            writer.Write(type.IsValueType ? 0 : 0x3fff_ffff);
            writer.Write(typeName);
            writer.Write(fieldPayloadKind);
            writer.Write(fields.Length + legacy.Length + (retiredCheckpoint is null ? 0 : 1));
            foreach ((string name, object value) in legacy)
            {
                writer.Write(typeName);
                writer.Write(name);
                writer.Flush();
                if (value is RetiredLineScrolls retiredLines)
                    WriteRetiredLineScrolls(writer, data, retiredLines.Lines);
                else if (value is RetiredNeckPoint retiredNeck)
                    WriteRetiredNeckPoint(writer, data, retiredNeck.Point);
                else
                    Value(value);
            }
            if (retiredCheckpoint is { } checkpoint)
            {
                // The removed record's identity, in the shape the old writer produced.
                string retired = "SuperMetroid.Core.Game.SamusCameraPoint, SuperMetroid.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                writer.Write(typeName);
                writer.Write(checkpoint.Field);
                writer.Write(newObjectMarker);
                writer.Write(0);
                writer.Write(retired);
                writer.Write(fieldPayloadKind);
                writer.Write(4);
                string[] names = ["<XPosition>k__BackingField", "<XSubposition>k__BackingField",
                    "<YPosition>k__BackingField", "<YSubposition>k__BackingField"];
                for (int index = 0; index < 4; index++)
                {
                    writer.Write(retired);
                    writer.Write(names[index]);
                    writer.Flush();
                    DebuggerObjectGraphSerializer.Serialize(data, checkpoint.Words[index]);
                }
            }
            foreach (FieldInfo field in fields)
            {
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!));
                writer.Write(field.Name);
                Value(field.GetValue(instance));
            }
        }
        data.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<object>(data);
    }
}
