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
    /// <summary>
    /// <c>$A7:C3F0</c>, the final SpawnHardcodedPLM call in KraidDeath_Initialisation:
    /// start the animated spike sweep at (5,27), independently of defeated-room clearing.
    /// </summary>
    public static readonly KraidPlmRequest LiveDeathSpikes = new(0x05, 0x1b, RoomPlmHeaders.CrumbleKraidSpikes);

    /// <summary>$A7:C168 SpawnPLMToClearTheCeiling: clear ceiling blocks from (2,18).</summary>
    private static KraidPlmRequest ClearCeiling => new(0x02, 0x12, RoomPlmHeaders.ClearKraidCeiling);

    /// <summary>$A7:C171 SpawnPLMToClearTheSpikes: clear spike blocks from (5,27).</summary>
    private static KraidPlmRequest ClearSpikes => new(0x05, 0x1b, RoomPlmHeaders.ClearKraidSpikes);

    /// <summary>The nine calls in <c>$A7:AC4D</c>, indexed by Kraid variable F / 2.</summary>
    public static IReadOnlyList<KraidPlmRequest> GrowthCeiling { get; } = new GrowthCeilingSequence();

    /// <summary>
    /// The hardcoded PLMs spawned by defeated-room initialization at
    /// <c>$A7:C168</c> (<c>Kraid_SpawnPlmToClearCeiling</c>) and <c>$A7:C171</c>
    /// (<c>Kraid_ClearSomeSpikes</c>), in native call order.
    /// </summary>
    public static IReadOnlyList<KraidPlmRequest> DefeatedRoom { get; } = new DefeatedRoomSequence();

    private sealed class DefeatedRoomSequence : IReadOnlyList<KraidPlmRequest>
    {
        public int Count => 2;
        public KraidPlmRequest this[int index] => index switch
        {
            0 => ClearCeiling,
            1 => ClearSpikes,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $A7:ACC5 selects the ceiling callbacks at ACD7..AD3A. Each block lies
    /// directly above its paired rock center; row18 is constant. Column2 uses
    /// the left-edge background, then odd/even columns alternate variants2/3.
    /// </summary>
    private sealed class GrowthCeilingSequence : IReadOnlyList<KraidPlmRequest>
    {
        public int Count => 9;
        public KraidPlmRequest this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                byte column = (byte)(KraidCeilingRockPositions.AtByteOffset(2 * index) >> 4);
                ushort header = column == 2 ? RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1
                    : (column & 1) == 0 ? RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3
                    : RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2;
                return new(column, 0x12, header);
            }
        }
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
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
