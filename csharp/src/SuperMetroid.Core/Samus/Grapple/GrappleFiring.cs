using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple's launch velocity, angle and hand origin for each firing direction.</summary>
internal static class GrappleFiring
{
    /// <summary>The launch velocity and angle of a beam fired in <paramref name="direction"/>.</summary>
    internal static GrappleLaunchDefinition Launch(SamusProjectileDirection direction) =>
        GrappleFiringDefinitions.Launches.For(direction);

    /// <summary>
    /// Where the beam leaves Samus's hand when fired in <paramref name="aim"/>, running or not. Later graphics and pose
    /// corrections belong to the firing.
    /// </summary>
    internal static Offset16<RoomPixels> Origin(SamusProjectileDirection aim, bool running) =>
        (running ? GrappleFiringDefinitions.RunningOrigins : GrappleFiringDefinitions.StandingOrigins).For(aim);
}
