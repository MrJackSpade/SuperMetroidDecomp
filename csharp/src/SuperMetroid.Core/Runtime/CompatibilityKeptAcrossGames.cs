namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The compatibility services whose state outlives each game (#1801): the session keeps them across the gameplay runtimes
/// it creates, and each runtime hands them to the owners that consult them.
/// </summary>
internal sealed record CompatibilityKeptAcrossGames
{
    /// <summary>The session's unauthored Speed Booster cadence, whose delays outlive each runtime.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>When an open save confirmation reads the controller; the runtime's message box consults it.</summary>
    public required SaveChoiceInputCadence SaveChoiceInput { get; init; }

    /// <summary>The camera fractions the previous game left, which the runtime's first room's camera starts from and its release keeps.</summary>
    public required CameraFractionsKeptAcrossGames CameraFractions { get; init; }
}
