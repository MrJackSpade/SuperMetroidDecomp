using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// Confirms that door orientation bytes decode as a closed direction and closing-behavior
/// domain, while elevator pseudo-doors remain separate door-list entries.
/// </summary>
internal static class DoorOrientationDomainVerification
{
    /// <summary>
    /// #627: the door header's orientation byte is a closed domain. The import codec decodes it into a travel
    /// direction and closing behavior, bytes beyond <c>Door_Closing_PLMs</c> are rejected, and elevator pseudo-doors
    /// resolve as door-list entries rather than headers. Since #1414 the direction and closing values are arbitrary:
    /// this asserts the decoded domain values per native bit field, not the enum values.
    /// </summary>
    internal static void VerifyDoorOrientationDomain()
    {
        DoorDirection[] directions = [DoorDirection.Right, DoorDirection.Left, DoorDirection.Down, DoorDirection.Up];
        DoorClosingBehavior[] closings = [DoorClosingBehavior.None, DoorClosingBehavior.BlueDoorCloses, DoorClosingBehavior.EscapeGateCloses];
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            byte value = (byte)raw;
            if (raw > 0x0b)
            {
                AssertThrows<InvalidDataException>(() => DoorRecordCodec.DecodeOrientation(value),
                    $"orientation ${raw:X2} beyond Door_Closing_PLMs is rejected");
                continue;
            }
            DoorOrientation orientation = DoorRecordCodec.DecodeOrientation(value);
            AssertEqual(directions[raw & 3], orientation.Direction, $"orientation ${raw:X2} direction");
            AssertEqual(closings[raw >> 2], orientation.Closing, $"orientation ${raw:X2} closing behavior");
            AssertEqual(value, orientation.Encode(), $"orientation ${raw:X2} round-trips");
            AssertEqual((raw & 2) != 0, orientation.IsVertical, $"orientation ${raw:X2} axis matches native bit 1");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new DoorOrientation((DoorDirection)0, DoorClosingBehavior.None),
            "undefined direction is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new DoorOrientation(DoorDirection.Up, (DoorClosingBehavior)0),
            "undefined closing behavior is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => ((DoorDirection)0).IsVertical(), "undefined direction has no axis");

        // The pseudo-doors' overlapped bytes ($91/$93 at offset 3) are not orientations.
        foreach (ushort pseudo in new[] { DoorDefinitionsAccess.SharedElevatorPseudoDoorPointer, DoorDefinitionsAccess.MaridiaTourianElevatorPseudoDoorPointer })
            AssertThrows<ArgumentOutOfRangeException>(() => DoorDefinitions.Get(pseudo), $"pseudo-door ${pseudo:X4} is not a header");
        foreach (ElevatorPseudoDoor pseudo in Enum.GetValues<ElevatorPseudoDoor>())
            AssertTrue(DoorListEntry.ElevatorPseudoDoor(pseudo).IsElevatorPseudoDoor, $"{pseudo} pseudo-door entry");
        AssertThrows<ArgumentOutOfRangeException>(() => DoorListEntry.ElevatorPseudoDoor((ElevatorPseudoDoor)2), "an undefined pseudo-door is rejected");

        Console.WriteLine("Door orientation domain: decode/encode, rejection and pseudo-door entries agree.");
    }
}
