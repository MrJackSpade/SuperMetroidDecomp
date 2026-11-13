namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The sideways drift a falling spark takes when its random launch choice lies past the seven authored launches
/// (#1500). The choice is one of eight, so one choice in eight reads the entry after the last launch: the start
/// of the spark's own code, which makes a fast leftward drift. The import pipeline decodes that drift once.
/// </summary>
/// <remarks>
/// Consulted when a spark is spawned, after it takes its random number, on the one choice of eight past the authored
/// launches. Evidence: the spark's initializer indexes the launch table at <c>$86:F3D4</c> by random bits two through
/// four, four bytes a record, and the table holds seven records, so the eighth choice reads the instruction words at
/// <c>$86:F3F0</c> and <c>$86:F3F2</c> as its whole and fractional drift. The falling-spark launch check decodes them
/// from the player's game data, compares them with this service, and spawns a spark for every random number.
/// </remarks>
[CompatibilityService("a falling spark whose random launch choice lies past the seven authored launches drifts by the code stored after them")]
public sealed class FallingSparkUnauthoredLaunch
{
    /// <summary>Keeps the decoded drift.</summary>
    /// <param name="drift">The signed horizontal drift per update, in pixels and sixteen-bit fractions.</param>
    public FallingSparkUnauthoredLaunch(SignedSixteenSixteen drift) => Drift = drift;

    /// <summary>The signed horizontal drift per update the spark launches with.</summary>
    public SignedSixteenSixteen Drift { get; }
}
