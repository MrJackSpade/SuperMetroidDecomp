using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each escape gate image (<see cref="MotherBrainEscapeGateDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static class MotherBrainEscapeGateDrawKeys
{
    /// <summary>Each MotherBrainEscapeGateDraw member's installed key.</summary>
    internal static InstalledKeys<MotherBrainEscapeGateDraw> Installed { get; } = new(new Dictionary<MotherBrainEscapeGateDraw, string>
    {
        [MotherBrainEscapeGateDraw.Open] = "open",
        [MotherBrainEscapeGateDraw.HalfClosed] = "half-closed",
        [MotherBrainEscapeGateDraw.Closed] = "closed",
    });
}
