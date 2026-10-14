using SuperMetroid.Core.Runtime;

/// <summary>Cartridge values of <see cref="CeresShaftRotationDefinitions"/> that only verification reads.</summary>
internal static class CeresShaftRotationDefinitionsConstants
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
    /// The #1165 research checked every coefficient against NTSC J/U v1.0 ROM, pinned
    /// RoomMainASM_CeresElevatorShaft assembly, and Read, using decimal Taylor bounds to
    /// certify each trig rounding. All 65,536 phase values were checked, including the 138
    /// valid aliases created by wrapped 16-bit multiplication before indexing.
    /// This proves a compatible generator, not the original authoring tool. The existing
    /// coefficient logic matches it; the separate timer ramp is described below.
    /// </remarks>
    public const int ReferenceAddress = 0x89ad5f;

    /// <summary>Provides the native reference address for Ceres shaft rotation data checks.</summary>
    extension(CeresShaftRotationDefinitions)
    {
        /// <inheritdoc cref="CeresShaftRotationDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => CeresShaftRotationDefinitionsConstants.ReferenceAddress;
    }
}
