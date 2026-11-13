using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How the Ceres self-destruct readies its warning (#1406, #1463, #1538): the updates it spends bringing up the escape
/// timer and warning, which the Ridley arena's backgrounds then show, and the updates it then spends readying the arena's
/// backgrounds, which show nothing new; and the look the arena shows once the warning is up.
/// </summary>
internal static class CeresEscapeReadyingDefinitions
{
    /// <summary>
    /// The updates the self-destruct spends bringing up the escape timer and warning, one update each for the timer's two
    /// parts and the warning's five, as the game's warning setup takes: the warning look shows from the last.
    /// </summary>
    internal const int WarningReadyingDuration = 7;

    /// <summary>
    /// The updates the self-destruct then spends readying the arena's backgrounds for the warning text and the doors, one
    /// each for eight, as the game's warning setup takes; the arena shows nothing of them.
    /// </summary>
    internal const int BackgroundReadyingDuration = 8;

    /// <summary>The look the Ridley arena's backgrounds show once the escape timer and warning are up.</summary>
    internal static EncounterLook WarningLook { get; } = new(EncounterArtwork.CeresEscape, "Warning");
}
