namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>How the opening Ceres flight shows its perspective floor.</summary>
internal static class CeresFlightRenderDefinitions
{
    /// <summary>The floor point, in floor pixels, that the opening flight's floor turns and zooms about.</summary>
    internal static Offset16<FloorPixels> FloorCenter => new(56, 24);
}
