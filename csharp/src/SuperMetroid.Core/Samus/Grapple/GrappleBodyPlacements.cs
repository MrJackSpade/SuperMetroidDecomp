using static SuperMetroid.Core.Samus.Grapple.GrappleBodyPlacementDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Where Samus's body sits on the grapple at each angle.</summary>
internal static class GrappleBodyPlacements
{
    /// <summary>Samus's offset from the rope's start while swinging in <paramref name="direction"/>, facing left or right.</summary>
    internal static Offset16<RoomPixels> Offset(GrappleSwingDirection direction, bool left) =>
        (left ? LeftOffsets : RightOffsets)[direction.Number];
}
