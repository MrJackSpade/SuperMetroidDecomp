namespace SuperMetroid.Core.Runtime;

/// <summary>Calculated delay ramps and Mode 7 coefficients for the Ceres escape shaft.</summary>
public static class CeresShaftRotationDefinitions
{
    /// <summary>$89:AD5F, RoomCode_CeresElevatorShaft timer/sine/cosine records.</summary>
    /// <remarks>
    /// Issues #625 and #949: all 138 trigonometric words have an exact, correction-free generation model.
    /// For record i=0..68 let n=i-34 and angle=n/256 radians. Independently round
    /// 256*sin(angle) and 256*cos(angle) to nearest integer. Across this bounded domain,
    /// sine simplifies to n and cosine to 256-floor((n*n+255)/512), using integer division.
    /// The latter is also the quadratic small-angle approximation with half-way decrements
    /// rounded downward. It is NOT round(sqrt(65536-n*n)): that alternative returns 255
    /// at n=+/-16, while the native independently quantized coordinates have cosine 256.
    /// LookupTableResearch checks every coefficient against NTSC J/U v1.0 ROM, pinned
    /// RoomMainASM_CeresElevatorShaft assembly, and Read, using decimal Taylor bounds to
    /// certify each trig rounding. All 65,536 phase values are checked, including the 138
    /// valid aliases created by wrapped 16-bit multiplication before indexing.
    /// This proves a compatible generator, not the original authoring tool. The existing
    /// coefficient logic matches it; the separate timer ramp is described below.
    /// </remarks>
    public const int ReferenceAddress = 0x89ad5f;

    /// <summary>$89:AD5F-$AEFC contains 69 records, from sine -34 through +34.</summary>
    public const int RecordCount = 69;

    /// <summary>Quantized delay ramps for absolute sine magnitude zero through34.</summary>
    /// <remarks>
    /// $89:AD5F supplies 69 timers symmetric in absolute sine, represented here by
    /// 35 values. Native DEC/BPL makes a loaded timer last timer+1 room-main calls.
    /// The delay slope doubles across four linear bands: one tick per four magnitude
    /// steps through15 (ceiling), one per two at16..21 (floor), one at22..26,
    /// and two at27..33. The last three bands start from delay5 at16, delay8 at22,
    /// and delay12 at26 respectively. Zero keeps the minimum one-tick delay.
    /// Magnitude34 uses delay60 at each turnaround. This exact bounded piecewise
    /// construction is independently compared with all69 native timer words and both
    /// wrapped phase aliases; it does not assert which historical tool authored the ramps.
    /// </remarks>
    private static ushort Timer(int magnitude) => magnitude switch
    {
        0 => 1,
        <= 15 => (ushort)((magnitude + 3) / 4),
        <= 21 => (ushort)(5 + (magnitude - 16) / 2),
        <= 26 => (ushort)(8 + magnitude - 22),
        <= 33 => (ushort)(12 + 2 * (magnitude - 26)),
        34 => 60,
        _ => throw new ArgumentOutOfRangeException(nameof(magnitude)),
    };

    /// <summary>Resolves the native wrapping six-byte offset, including encoded reverse phases.</summary>
    public static (ushort Timer, ushort Sine, ushort Cosine) Read(ushort phase)
    {
        // Multiplication wraps before indexing: $8044 selects record 68, not a
        // negative index. Reject non-record offsets instead of inventing ROM data.
        int offset = unchecked((ushort)(6 * phase));
        int record = offset / 6;
        if (offset % 6 != 0 || record >= RecordCount)
            throw new InvalidDataException($"Ceres shaft rotation phase ${phase:X4} does not select an authored record.");
        int sine = record - 34;
        int magnitude = Math.Abs(sine);
        ushort cosine = (ushort)(256 - (magnitude * magnitude + 255) / 512);
        return (Timer(magnitude), unchecked((ushort)sine), cosine);
    }
}
