using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>One hardcoded bank-$84 PLM request issued by Kraid's bank-$A7 AI.</summary>
public readonly record struct KraidPlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>
/// Literal hardcoded PLM arguments embedded in Kraid's growth and sinking callbacks.
/// Their irregular order is visible: the ceiling breaks from scattered points as the
/// body rises, rather than sweeping monotonically across the room.
/// </summary>
public static class KraidPlmDefinitions
{
    private static readonly KraidPlmRequest[] DefeatedRoomRequests =
    [
        new(0x02, 0x12, RoomPlmHeaders.ClearKraidCeiling),
        new(0x05, 0x1b, RoomPlmHeaders.ClearKraidSpikes),
    ];

    private static readonly KraidPlmRequest[] GrowthCeilingRequests =
    [
        new(0x06, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3),
        new(0x0d, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2),
        new(0x02, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1),
        new(0x0a, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3),
        new(0x05, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2),
        new(0x0c, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3),
        new(0x03, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2),
        new(0x0b, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2),
        new(0x04, 0x12, RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3),
    ];

    /// <summary>The nine calls in <c>$A7:AC4D</c>, indexed by Kraid variable F / 2.</summary>
    public static IReadOnlyList<KraidPlmRequest> GrowthCeiling => GrowthCeilingRequests;

    /// <summary>
    /// The hardcoded PLMs spawned by defeated-room initialization at
    /// <c>$A7:C168</c> (<c>Kraid_SpawnPlmToClearCeiling</c>) and <c>$A7:C171</c>
    /// (<c>Kraid_ClearSomeSpikes</c>), in native call order.
    /// </summary>
    public static IReadOnlyList<KraidPlmRequest> DefeatedRoom => DefeatedRoomRequests;

    /// <summary>Returns the platform mutation paired with a sinking-table callback.</summary>
    public static KraidPlmRequest? ForSinkCallback(ushort callback) => callback switch
    {
        KraidSinkCallbacks.NoOperation => null,
        KraidSinkCallbacks.CrumbleLeftPlatformLeft =>
            new(0x07, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant1),
        KraidSinkCallbacks.CrumbleRightPlatformMiddle =>
            new(0x0f, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant1),
        KraidSinkCallbacks.CrumbleRightPlatformLeft =>
            new(0x0e, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant2),
        KraidSinkCallbacks.CrumbleLeftPlatformRight =>
            new(0x09, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant1),
        KraidSinkCallbacks.CrumbleLeftPlatformMiddle =>
            new(0x08, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant2),
        KraidSinkCallbacks.CrumbleRightPlatformRight =>
            new(0x10, 0x12, RoomPlmHeaders.CrumbleKraidPlatformVariant2),
        _ => throw new InvalidDataException(
            $"Kraid sinking callback $A7:{callback:X4} has no hardcoded PLM definition."),
    };
}
