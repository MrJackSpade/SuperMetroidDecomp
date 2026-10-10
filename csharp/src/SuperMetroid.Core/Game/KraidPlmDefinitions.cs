using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>One hardcoded bank-$84 PLM request issued by Kraid's bank-$A7 AI.</summary>
/// <param name="BlockX">Horizontal block coordinate where the room PLM is applied.</param>
/// <param name="BlockY">Vertical block coordinate where the room PLM is applied.</param>
/// <param name="Header">Room PLM header selecting the block mutation or animation.</param>
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

    /// <summary>Provides the two PLMs used to clear Kraid's ceiling and spikes, in native callback order.</summary>
    private sealed class DefeatedRoomSequence : IReadOnlyList<KraidPlmRequest>
    {
        /// <summary>Gets the number of hardcoded requests in the defeated-room sequence.</summary>
        public int Count => 2;

        /// <summary>Gets the ceiling-clear request first and the spike-clear request second.</summary>
        /// <param name="index">Zero-based position in the native call order.</param>
        /// <returns>The hardcoded request at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0 or 1.</exception>
        public KraidPlmRequest this[int index] => index switch
        {
            0 => ClearCeiling,
            1 => ClearSpikes,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        /// <summary>Enumerates ceiling and spike clearing requests in the order Kraid's initialization issues them.</summary>
        /// <returns>An enumerator over the two hardcoded requests.</returns>
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $A7:ACC5 selects the ceiling callbacks at ACD7..AD3A. Each block lies
    /// directly above its paired rock center; row18 is constant. Column2 uses
    /// the left-edge background, then odd/even columns alternate variants2/3.
    /// </summary>
    private sealed class GrowthCeilingSequence : IReadOnlyList<KraidPlmRequest>
    {
        /// <summary>Gets the number of ceiling PLM requests in Kraid's growth callback table.</summary>
        public int Count => 9;

        /// <summary>Builds the ceiling PLM request associated with one entry in the native rock-position table.</summary>
        /// <param name="index">Zero-based callback entry, from 0 through 8.</param>
        /// <returns>A request for the ceiling block above that entry's rock column.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the callback table.</exception>
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

        /// <summary>Enumerates all nine growth ceiling requests in native callback-table order.</summary>
        /// <returns>An enumerator over the generated ceiling requests.</returns>
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Returns the platform mutation paired with a sinking-table callback.</summary>
    public static KraidPlmRequest? ForSinkCallback(ushort callback)
    {
        if (callback == KraidSinkCallbacks.NoOperation) return null;
        byte column = callback switch
        {
            KraidSinkCallbacks.CrumbleLeftPlatformLeft => 7,
            KraidSinkCallbacks.CrumbleLeftPlatformMiddle => 8,
            KraidSinkCallbacks.CrumbleLeftPlatformRight => 9,
            KraidSinkCallbacks.CrumbleRightPlatformLeft => 14,
            KraidSinkCallbacks.CrumbleRightPlatformMiddle => 15,
            KraidSinkCallbacks.CrumbleRightPlatformRight => 16,
            _ => throw new InvalidDataException(
                $"Kraid sinking callback $A7:{callback:X4} has no hardcoded PLM definition."),
        };
        // The surviving platforms share the ceiling row and alternate background
        // blocks by column parity, as in the native C691..C714 inline arguments.
        return new(column, 0x12, (column & 1) != 0
            ? RoomPlmHeaders.CrumbleKraidPlatformVariant1
            : RoomPlmHeaders.CrumbleKraidPlatformVariant2);
    }
}
