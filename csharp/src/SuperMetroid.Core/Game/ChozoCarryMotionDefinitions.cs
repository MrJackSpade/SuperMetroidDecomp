using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Statue movement and Samus joint offsets for Wrecked Ship and Lower Norfair.</summary>
public static class ChozoCarryMotionDefinitions
{
    /// <summary>$AA:E630: X velocity in 8.8; its absolute magnitude also drives downward collision.</summary>
    /// <remarks>
    /// All 32 signed words match the pinned NTSC J/U v1.0 ROM. The nonzero
    /// first-half words are negative; the second half is their positive mirror.
    /// Each half holds four stationary poses, repeats the stride
    /// <c>0200,0300,0E00,0800</c> burst twice, then holds four more zeros.
    /// Adjacent stock foot origins derive planted-foot displacement. Transfer advance2
    /// and final plant slide1 specify the authored pose-transition trajectory: changing
    /// them changes the foot-transfer advance and final planted-foot shift. They are not
    /// continuous physical rates. No independent planted3/14/7 magnitude is retained.
    /// Investigation: #625 / #665.
    /// </remarks>
    private const int FootTransferAdvance = 2, FinalPlantSlide = 1;

    /// <summary>Calculates one pose's 8.8 horizontal movement magnitude from the statue's support-foot stride.</summary>
    /// <param name="local">Pose index within one 16-record facing half of the native movement table.</param>
    /// <returns>The positive movement magnitude during the stride, or zero for stationary poses.</returns>
    private static short Magnitude(int local)
    {
        if (local is < 4 or >= 12) return 0;
        int phase = local % 4;
        int pixels = phase == 0 ? FootTransferAdvance :
            ChozoStrideGeometryDefinitions.SupportFootX(phase) -
            ChozoStrideGeometryDefinitions.SupportFootX(phase - 1) +
            (phase == 3 ? FinalPlantSlide : 0);
        return (short)(pixels << 8);
    }
    /// <summary>
    /// $AA:E6B0: Samus's vertical hand offset, shared by both facing halves.
    /// The two acquisition/release poses place Samus32 and25 pixels above the statue.
    /// The carried pose is23 pixels above, with a two-pixel triangular bob during the
    /// two four-pose stride cycles at local indices4..11. Remaining poses hold still.
    /// Native movement instructions in $AA:E4C1-$E54D traverse those stride poses;
    /// $AA:E55B-$E573 reverses the acquisition poses when releasing Samus.
    /// </summary>
    private static short SamusYOffset(int local)
    {
        if (local == 0)
            return -32;
        if (local == 1)
            return -25;
        int bob = local is >= 4 and < 12 ? 2 - Math.Abs(2 - local % 4) : 0;
        return (short)(-23 - bob);
    }
    /// <summary>$AA:E630/E670/E6B0: one of 32 velocity/X-joint/Y-joint records, selected by an even byte offset.</summary>
    /// <remarks>
    /// The carried-Samus X field at <c>$AA:E670</c> is exactly
    /// <c>sign * (local == 0 ? 28 : local == 1 ? 30 : 32)</c>, where
    /// <c>local = (byteOffset / 2) &amp; 15</c> and sign is negative for the
    /// first sixteen records, positive for the second. All 32 native words
    /// match the pinned NTSC J/U v1.0 ROM. The caller adds this signed whole
    /// pixel offset after moving the statue, with 16-bit position wrapping.
    /// Odd and post-table offsets are rejected. Investigation: #625 / #666.
    /// </remarks>
    public static (short Velocity, short SamusX, short SamusY) Read(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset > 62)
            throw new InvalidDataException($"Chozo carry offset {byteOffset:X4} is outside its 32 records.");
        int index = byteOffset >> 1;
        int local = index & 15;
        int sign = index < 16 ? -1 : 1;
        int x = local == 0 ? 28 : local == 1 ? 30 : 32;
        return ((short)(sign * Magnitude(local)), (short)(sign * x), SamusYOffset(local));
    }
}
