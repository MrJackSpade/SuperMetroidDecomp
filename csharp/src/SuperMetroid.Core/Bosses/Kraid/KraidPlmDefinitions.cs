using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Kraid;

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
    public static readonly KraidPlmRequest LiveDeathSpikes = new(new(0x05, 0x1b), PlmHeaderId.CrumbleKraidSpikes);

    /// <summary>$A7:C168 SpawnPLMToClearTheCeiling: clear ceiling blocks from (2,18).</summary>
    private static KraidPlmRequest ClearCeiling => new(new(0x02, 0x12), PlmHeaderId.ClearKraidCeiling);

    /// <summary>$A7:C171 SpawnPLMToClearTheSpikes: clear spike blocks from (5,27).</summary>
    private static KraidPlmRequest ClearSpikes => new(new(0x05, 0x1b), PlmHeaderId.ClearKraidSpikes);

    /// <summary>The nine calls in <c>$A7:AC4D</c>, indexed by Kraid variable F / 2.</summary>
    public static IReadOnlyList<KraidPlmRequest> GrowthCeiling { get; } = new GrowthCeilingSequence();

    /// <summary>
    /// The hardcoded PLMs spawned by defeated-room initialization at
    /// <c>$A7:C168</c> (<c>Kraid_SpawnPlmToClearCeiling</c>) and <c>$A7:C171</c>
    /// (<c>Kraid_ClearSomeSpikes</c>), in native call order.
    /// </summary>
    public static IReadOnlyList<KraidPlmRequest> DefeatedRoom { get; } = new DefeatedRoomSequence();

    /// <summary>Enumerates the ceiling and spike cleanup requests in defeated-room call order.</summary>
    private sealed class DefeatedRoomSequence : IReadOnlyList<KraidPlmRequest>
    {
        /// <summary>Defeated-room initialization issues two hardcoded PLM requests.</summary>
        public int Count => 2;
        /// <summary>Selects ceiling cleanup before spike cleanup.</summary>
        public KraidPlmRequest this[int index] => index switch
        {
            0 => ClearCeiling,
            1 => ClearSpikes,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Visits the two cleanup requests in native call order.</summary>
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same cleanup order through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $A7:ACC5 selects the ceiling callbacks at ACD7..AD3A. Each block lies
    /// directly above its paired rock center; row18 is constant. Column2 uses
    /// the left-edge background, then odd/even columns alternate variants2/3.
    /// </summary>
    private sealed class GrowthCeilingSequence : IReadOnlyList<KraidPlmRequest>
    {
        /// <summary>Nine ceiling blocks are paired with the growth-stage rock centers.</summary>
        public int Count => 9;
        /// <summary>Selects the block above one rock center and its column-dependent background variant.</summary>
        public KraidPlmRequest this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                byte column = (byte)(KraidCeilingRockPositionDefinitions.AtByteOffset(2 * index) >> 4);
                PlmHeaderId header = column == 2 ? PlmHeaderId.CrumbleKraidCeilingIntoBackground1
                    : (column & 1) == 0 ? PlmHeaderId.CrumbleKraidCeilingIntoBackground3
                    : PlmHeaderId.CrumbleKraidCeilingIntoBackground2;
                return new(new(column, 0x12), header);
            }
        }
        /// <summary>Visits ceiling mutations in growth-stage callback order.</summary>
        public IEnumerator<KraidPlmRequest> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes growth-stage ordering through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Returns the platform mutation paired with a sinking-table callback.</summary>
    public static KraidPlmRequest? ForSinkCallback(KraidSinkCallback callback)
    {
        byte column;
        switch (callback)
        {
            case KraidSinkCallback.NoOperation: return null;
            case KraidSinkCallback.CrumbleLeftPlatformLeft: column = 7; break;
            case KraidSinkCallback.CrumbleLeftPlatformMiddle: column = 8; break;
            case KraidSinkCallback.CrumbleLeftPlatformRight: column = 9; break;
            case KraidSinkCallback.CrumbleRightPlatformLeft: column = 14; break;
            case KraidSinkCallback.CrumbleRightPlatformMiddle: column = 15; break;
            case KraidSinkCallback.CrumbleRightPlatformRight: column = 16; break;
            default: throw new InvalidOperationException($"Undefined KraidSinkCallback {callback}.");
        }
        // The surviving platforms share the ceiling row and alternate background
        // blocks by column parity, as in the native C691..C714 inline arguments.
        return new(new(column, 0x12), (column & 1) != 0
            ? PlmHeaderId.CrumbleKraidPlatformVariant1
            : PlmHeaderId.CrumbleKraidPlatformVariant2);
    }
}
