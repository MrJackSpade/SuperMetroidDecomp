using System.Reflection;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// #627: the door header's orientation byte is a closed domain. It decodes into a travel
    /// direction and closing behavior, bytes beyond <c>Door_Closing_PLMs</c> are rejected,
    /// elevator pseudo-doors resolve as door-list entries rather than headers, and older
    /// debugger payloads convert through the same validation.
    /// </summary>
    private static void VerifyDoorOrientationDomain()
    {
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            byte value = (byte)raw;
            if (raw > 0x0b)
            {
                AssertThrows<InvalidDataException>(() => CartridgeDoorOrientation.Decode(value),
                    $"orientation ${raw:X2} beyond Door_Closing_PLMs is rejected");
                continue;
            }
            CartridgeDoorOrientation orientation = CartridgeDoorOrientation.Decode(value);
            AssertEqual((DoorDirection)(raw & 3), orientation.Direction, $"orientation ${raw:X2} direction");
            AssertEqual((DoorClosingBehavior)(raw >> 2), orientation.Closing, $"orientation ${raw:X2} closing behavior");
            AssertEqual(value, orientation.Encode(), $"orientation ${raw:X2} round-trips");
            AssertEqual((raw & 2) != 0, orientation.IsVertical, $"orientation ${raw:X2} axis matches native bit 1");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new CartridgeDoorOrientation((DoorDirection)4, DoorClosingBehavior.None),
            "undefined direction is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new CartridgeDoorOrientation(DoorDirection.Up, (DoorClosingBehavior)3),
            "undefined closing behavior is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => ((DoorDirection)4).IsVertical(), "undefined direction has no axis");

        // The pseudo-doors' overlapped bytes ($91/$93 at offset 3) are not orientations.
        foreach (ElevatorPseudoDoorPointer pseudo in Enum.GetValues<ElevatorPseudoDoorPointer>())
        {
            AssertThrows<ArgumentOutOfRangeException>(() => DoorDefinitions.Get((ushort)pseudo), $"pseudo-door ${(int)pseudo:X4} is not a header");
            AssertTrue(DoorListEntry.ElevatorPseudoDoor(pseudo).IsElevatorPseudoDoor, $"pseudo-door ${(int)pseudo:X4} entry");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DoorListEntry.ElevatorPseudoDoor((ElevatorPseudoDoorPointer)0x88fe), "a physical pointer is not a pseudo-door");

        // Debugger boundary: older primitive payloads decode through the domain's validation.
        FieldInfo orientationField = typeof(CartridgeDoorHeader).GetField("<Orientation>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        AssertTrue(DebuggerRetypedFieldDefinitions.TryConvert(orientationField, (byte)0x09, out object converted), "legacy orientation byte converts");
        AssertEqual(new CartridgeDoorOrientation(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses), converted, "legacy orientation $09 decodes");
        AssertThrows<InvalidDataException>(() => DebuggerRetypedFieldDefinitions.TryConvert(orientationField, (byte)0x91, out _),
            "legacy out-of-domain orientation is rejected");
        Type scrollType = typeof(DoorDirection).Assembly.GetType("SuperMetroid.Core.Runtime.DoorOpeningScrollState", throwOnError: true)!;
        FieldInfo directionField = scrollType.GetField("<Direction>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        AssertTrue(DebuggerRetypedFieldDefinitions.TryConvert(directionField, 3, out converted), "legacy int direction converts");
        AssertEqual(DoorDirection.Up, converted, "legacy direction 3 decodes as up");
        AssertThrows<InvalidDataException>(() => DebuggerRetypedFieldDefinitions.TryConvert(directionField, 4, out _),
            "legacy undefined direction is rejected");
        AssertTrue(!DebuggerRetypedFieldDefinitions.TryConvert(directionField, (ushort)3, out _), "unregistered payload types are not coerced");
        Console.WriteLine("Door orientation domain: decode/encode, rejection, pseudo-door entries and debugger boundary agree.");
    }
}
