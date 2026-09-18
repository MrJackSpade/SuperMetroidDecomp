namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed timing definitions for Draygon's opening Evir dance.</summary>
internal static class DraygonIntroDanceDefinitions
{
    /// <summary>Native start of <c>DraygonFightIntroDanceData</c> at <c>$A5:CE07</c>.</summary>
    public const int NativeMovementStreamAddress = 0xa5ce07;

    /// <summary>
    /// The four reachable words of <c>MovementLatencyForEachEvirSpriteObject</c> at
    /// <c>$A5:A19F-$A5:A1A6</c>, ordered by sprite slots 28 through 31.
    /// </summary>
    private static ReadOnlySpan<short> MovementLatencies =>
        [-0x0380, -0x0300, -0x0280, -0x0200];

    /// <summary>Native byte-index advance after each dance update at <c>$A5:A188</c>.</summary>
    public const ushort StreamIndexAdvance = 4;

    /// <summary>Native dance duration checked at <c>$A5:8790</c>.</summary>
    public const ushort DurationFrames = 0x04d0;

    /// <summary>
    /// Last four-byte-aligned movement offset reachable by the four Evir actors during
    /// the native 1,232-frame intro. Each packed byte stores signed X and Y deltas as
    /// biased nibbles; <c>FF</c> is the cartridge's <c>80 80</c> delete sentinel. The
    /// authored deltas occupy only -4 through +5, so the sentinel cannot collide.
    /// </summary>
    public const ushort LastMovementStreamOffset = 0x113c;

    private const byte DeleteSentinel = 0xff;

    private const string PackedMovementHex =
        "B8B8B8B8B7A7A6A6A59596969898898A8A8A7B8B8B8C8C7C7C8D7C7C7D7C7C7B7B7B7B7A8989788787878685958584949484959595A59595A695A696A6A7B6B6" +
        "B6B6B6B6B7A7A7A8A8A8998A8A8B7B7B6B6B6B6B6B6A5A69696959596857575757667696A6B6C7B8B8B8AAAAAAAA9B9B9BAB9A9B9B9B9AAAAAAAAAA9A9B9B9B8" +
        "B9A8A8A898979797A89797A797A79797979797979696969686869687878686768776767677776778786969697979797A7979898A7A8A8A8A89898A8A898A8A99" +
        "89898A9999A899A9A9A9A9A89898A8A8A8A8A8989897A7989897A79797979797969796868686878696969696969696868687879786879787898989898A8A8A89" +
        "8A8A898A8A8A8A89898A89898A898A898999898A899A8999999998999998A8A89897979797979797979797979797979797979797879797878797979796868787" +
        "96968696868686868686868686868687877787878778898979798A7A7A798A797A798A8A8A89898979898A8A89898A898999898A89898A89898A8989898A8A8A" +
        "8A8A898A7A897989788787868686868696858686859696879697979797979898A9999999A999999A9A8A8A8A8A8A7A79797A797978686768777889999A9A9A99" +
        "A9999898A8A7A8989897A8A896A796A6A796A6A6A6A5A5959595959585959586868687788979797A6A7A7B7B8B7B7B8B8B8B8B8B9B9A9A9AA99999A8A8A79796" +
        "969695A595959595959585968576767676776767585969697A9AA9A9A9B8B8A8A8A8A898989898A898989898989898989898A898989897989898989798989897" +
        "989897979898989897979797979797988798979897979897979787979787878787878777787777787989797989898A79897A8989898989998989898999898989" +
        "8989898A8989898989898989798989898A89898989897989898989898989898989898989898989898989898A89898A8989797879796978777868787787878787" +
        "87879787979897989898A8989899989998989999999899999999999889999999998998989898989899989897A898988787979797978787868687878796978787" +
        "9787978787787989897979788989898989897989898A898A898A89998A8989899999989887978798979797878787879697878786968686878786878777777878" +
        "787879787979798989999898989898989898979897979797978797988989898A8989898A8A7A8A8A898989897989898989998998989898989897979787979697" +
        "86979796878787968786878787978789898989898989998999898999998999899998999998999999999998999899989898989797979797978787879787978787" +
        "87878787778687868787777787877778787878897989898979797989897989898A898A8989898A89898999999989FFFF979797A7A79796A69796969695969696" +
        "95969595959585968777777879797A6A6B6A6B6A7B7C7B7B8B8B8B8B9B8A9B9A9A9AA9A9A9A898A797A797A7A6A6A69596959595848585858575867666766677" +
        "777879696A7A7B8B7A8A8B9A9AAA9AAA";

    private static readonly byte[] PackedMovement = Convert.FromHexString(PackedMovementHex);

    /// <summary>Returns the signed stream latency for native sprite slot 28 through 31.</summary>
    public static short MovementLatencyForSlot(int slotIndex)
    {
        int index = slotIndex - 28;
        if ((uint)index >= MovementLatencies.Length)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        return MovementLatencies[index];
    }

    /// <summary>
    /// Resolves one normally reachable, four-byte-aligned Evir movement record from the
    /// compiled <c>$A5:CE07-$A5:DF44</c> trajectory. The native 1,232-frame owner can
    /// select exactly these 1,104 records after applying the four sprite latencies.
    /// A restored index outside that domain is corrupt state, not permission to interpret
    /// adjacent executable or presentation bytes as signed movement.
    /// </summary>
    public static DraygonIntroMovement ResolveMovement(ushort streamOffset)
    {
        if ((streamOffset & 3) != 0 || streamOffset > LastMovementStreamOffset)
        {
            throw new InvalidDataException(
                $"Draygon intro movement offset ${streamOffset:X4} is outside the compiled " +
                "four-byte-aligned retail trajectory.");
        }

        byte packed = PackedMovement[streamOffset / 4];
        if (packed == DeleteSentinel)
            return new DraygonIntroMovement(0, 0, DeletesSprite: true);

        return new DraygonIntroMovement(
            unchecked((sbyte)((packed >> 4) - 8)),
            unchecked((sbyte)((packed & 0x0f) - 8)),
            DeletesSprite: false);
    }
}

/// <summary>
/// One decoded Evir movement record from Draygon's fixed opening dance stream.
/// The delete sentinel is kept distinct from the otherwise valid zero-delta record.
/// </summary>
internal readonly record struct DraygonIntroMovement(
    sbyte XDelta,
    sbyte YDelta,
    bool DeletesSprite);
