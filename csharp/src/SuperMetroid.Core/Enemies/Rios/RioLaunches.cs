using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Chooses a Rio's launch: a Norfair Rio's downward launch, and the side every Rio swoops toward.</summary>
internal static partial class RioLaunches
{
    /// <summary>The fast or slow downward launch <paramref name="random"/> picks.</summary>
    internal static SignedEightEight NorfairYVelocity(int random) =>
        RioLaunchDefinitions.SlowLaunch.IsTakenBy(random) ? RioLaunchDefinitions.NorfairSlowYVelocity : RioLaunchDefinitions.NorfairFastYVelocity;

    /// <summary>
    /// The side of the Rio at <paramref name="place"/> that Samus lies on, which its swoop launches toward (#2569): left when
    /// her whole X lies left of its own, otherwise right.
    /// </summary>
    internal static HorizontalDirection SideOfSamus(RoomEnemyPlace place, SamusState samus) =>
        samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;
}
