using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
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
        Console.WriteLine("  Spatial debugger migrations: camera, scroll and door legacy words restore exactly; current layouts round-trip.");
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
            writer.Write(newObjectMarker);
            writer.Write(type.IsValueType ? 0 : 1);
            writer.Write(typeName);
            writer.Write(fieldPayloadKind);
            writer.Write(fields.Length + legacy.Length + (retiredCheckpoint is null ? 0 : 1));
            foreach ((string name, object value) in legacy)
            {
                writer.Write(typeName);
                writer.Write(name);
                writer.Flush();
                DebuggerObjectGraphSerializer.Serialize(data, value);
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
                writer.Flush();
                object? value = field.GetValue(instance);
                if (value is null)
                    writer.Write((byte)0);
                else
                    DebuggerObjectGraphSerializer.Serialize(data, value);
            }
        }
        data.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<object>(data);
    }
}
