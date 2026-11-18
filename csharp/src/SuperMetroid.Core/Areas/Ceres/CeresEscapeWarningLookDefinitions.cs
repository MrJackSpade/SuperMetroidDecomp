using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The look the Ridley arena shows once the Ceres self-destruct's escape timer and warning are up (#1406, #1463, #1538).</summary>
internal static class CeresEscapeWarningLookDefinitions
{
    /// <summary>The look the Ridley arena's backgrounds show once the escape timer and warning are up.</summary>
    internal static EncounterLook WarningLook { get; } = new(EncounterArtwork.CeresEscape, "Warning");
}
