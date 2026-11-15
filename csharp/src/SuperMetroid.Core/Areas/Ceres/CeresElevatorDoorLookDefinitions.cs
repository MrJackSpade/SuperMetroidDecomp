using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The look the Ceres elevator room's blocked door shows once it readies its own artwork (#1406, #1538, #1989).</summary>
internal static class CeresElevatorDoorLookDefinitions
{
    /// <summary>The look the elevator room's blocked door's initialization shows, before its first draw.</summary>
    internal static EncounterLook OverlayLook { get; } = new(EncounterArtwork.CeresElevatorDoor, "Overlay");
}
