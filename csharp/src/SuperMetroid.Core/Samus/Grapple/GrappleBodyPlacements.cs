using static SuperMetroid.Core.Samus.Grapple.GrappleBodyPlacementDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Where Samus's body sits on the grapple at each angle.</summary>
internal static partial class GrappleBodyPlacements
{
    /// <summary>
    /// Samus's offset from the rope's start while swinging in <paramref name="direction"/>, facing left or right. The
    /// direction's number is its honest place among the offsets (#2447): the directions are equal shares of the rope's
    /// turn, so the number measures the direction's angle in those shares, and the offsets are laid out one for each share
    /// around the turn.
    /// </summary>
    internal static Offset16<RoomPixels> Offset(GrappleSwingDirection direction, SamusFacingDirection facing) =>
        (facing is SamusFacingDirection.Left ? LeftOffsets : RightOffsets)[direction.Number];
}
