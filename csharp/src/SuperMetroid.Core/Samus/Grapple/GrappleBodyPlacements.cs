using static SuperMetroid.Core.Samus.Grapple.GrappleBodyPlacementDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Where Samus's body sits on the grapple at each angle.</summary>
internal static class GrappleBodyPlacements
{
    /// <summary>$9B:C1C2 GrappleSwingSamusXYOffsets: native 256-entry selector is nearest eight-angle bucket, wrapping at 32.</summary>
    internal static Offset16<RoomPixels> Offset(ByteAngle mirroredAngle, bool left)
    {
        int pair = ((mirroredAngle.RawValue + 4) >> 3) & 31;
        return (left ? LeftOffsets : RightOffsets)[pair];
    }
}
