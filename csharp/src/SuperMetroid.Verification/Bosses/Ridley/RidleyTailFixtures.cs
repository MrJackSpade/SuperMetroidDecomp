using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Sets up Ridley's tail links for checks that start from a pose of their own rather than the tail's initial layout.</summary>
internal static class RidleyTailFixtures
{
    /// <summary>
    /// Clears every link of <paramref name="tail"/>: inactive, having started the next link, swinging counterclockwise, at no distance
    /// and no target distance, pointing straight down, with no offset, at the room's origin. Each link keeps its lengths.
    /// </summary>
    internal static void BlankLinks(RidleyTailState tail)
    {
        foreach (RidleyTailLink link in tail.Links)
        {
            link.Active = false;
            link.StartedNextLink = true;
            link.SwingDirection = RotationDirection.Counterclockwise;
            link.Distance = SignedEightEight.Zero;
            link.TargetDistance = null;
            link.Angle = RidleyTailAngle.StraightDown;
            link.Offset = Offset16<RoomPixels>.Zero;
            link.Position = Position16<RoomPixels>.Zero;
        }
    }
}
