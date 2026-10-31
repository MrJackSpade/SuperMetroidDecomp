using static SuperMetroid.Core.Areas.Ceres.CeresShaftRotationDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Turns the Ceres escape shaft (<see cref="CeresShaftRotationDefinitions"/>): each rotation step's delay ramp and
/// turning-floor coefficients.
/// </summary>
public static class CeresShaftRotation
{
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
    private static int Timer(int magnitude) => magnitude switch
    {
        0 => 1,
        <= 15 => (magnitude + 3) / 4,
        <= 21 => 5 + (magnitude - 16) / 2,
        <= 26 => 8 + magnitude - 22,
        <= 33 => 12 + 2 * (magnitude - 26),
        34 => 60,
        _ => throw new ArgumentOutOfRangeException(nameof(magnitude)),
    };

    /// <summary>The rotation of <paramref name="step"/>, one of the shaft's <see cref="RecordCount"/> steps.</summary>
    /// <exception cref="InvalidDataException"><paramref name="step"/> is not one of the shaft's rotation steps.</exception>
    public static CeresShaftRotationDefinition Read(int step)
    {
        if ((uint)step >= RecordCount)
            throw new InvalidDataException($"Ceres shaft rotation step {step} is outside its {RecordCount} steps.");
        int sine = step - CenterStep;
        int magnitude = Math.Abs(sine);
        int cosine = 256 - (magnitude * magnitude + 255) / 512;
        return new(Timer(magnitude), sine, cosine);
    }
}
