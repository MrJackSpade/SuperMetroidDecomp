using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The look the Ceres doors' frames show once the pre-explosion overlay door readies its own artwork (#1406, #1538).</summary>
internal static class CeresDoorLookDefinitions
{
    /// <summary>The look the pre-explosion overlay door's initialization shows, before its first draw.</summary>
    internal static EncounterLook OverlayLook { get; } = new(EncounterArtwork.CeresDoor, "Overlay");
}
